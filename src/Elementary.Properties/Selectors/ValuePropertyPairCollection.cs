using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// A value propery pair collection is an inpput to all factories producing operations on property pairs
    /// like comparision assignment.
    /// It allows to manually exclude or override mappings.
    /// </summary>
    public class ValuePropertyPairCollection<L, R> : IEnumerable<IValuePropertyPair>, IValuePropertyPairCollectionConfiguration<L, R>
    {
        private readonly IEnumerable<IValuePropertyPair> propertyPairs;
        private readonly List<string> exclusions = new List<string>();
        private readonly List<IValuePropertyPair> included = new List<IValuePropertyPair>();

        internal ValuePropertyPairCollection(IEnumerable<IValuePropertyPair> propertyPairs) => this.propertyPairs = propertyPairs;

        #region IEnumerable<ValuePropertyPair>

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<IValuePropertyPair> GetEnumerator() => this.MergeResultPairs().GetEnumerator();

        virtual protected IEnumerable<IValuePropertyPair> MergeResultPairs()
        {
            return this.propertyPairs
                .Where(p => !this.exclusions.Contains(p.Left.Name))
                .Where(pp => !this.included.Any(p => p.Left.Equals(pp.Left)))
                .Concat(this.included);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion IEnumerable<ValuePropertyPair>

        #region IValuePropertyJoinConfiguration

        /// <summary>
        /// Exclude property pairs from the collection based on the name of a left side property.
        /// </summary>
        /// <param name="propertyNames"></param>
        void IValuePropertyPairCollectionConfiguration.ExcludeLeft(params string[] propertyNames) => this.exclusions.AddRange(propertyNames);

        /// <summary>
        ///<inheritdoc/>
        /// </summary>
        /// <param name="leftPropertyName"></param>
        /// <param name="rightPropertyName"></param>
        void IValuePropertyPairCollectionConfiguration<L, R>.IncludePair(string leftPropertyName, string rightPropertyName)
            => this.included.Add(new ValuePropertySymmetricPair(Property<L>.Info(leftPropertyName), Property<R>.Info(rightPropertyName)));

        void IValuePropertyPairCollectionConfiguration<L, R>.IncludeNested(Expression<Func<L, object?>> propertyAccess, Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure)
        {
            var leftProperty = Property<L>.Info(propertyAccess);
            var rightProperty = Property<R>.Info(leftProperty.Name);

            var configureDelegateType = typeof(Action<>).MakeGenericType(
                typeof(IValuePropertyPairCollectionConfiguration<,>).MakeGenericType(leftProperty.PropertyType, rightProperty.PropertyType));

            // make factory method call for nested properties
            var factoryMethod = typeof(ValuePropertyPair<,>)
                .MakeGenericType(leftProperty.PropertyType, rightProperty.PropertyType)
                .GetMethod("All", new[] { configureDelegateType });

            var nestedProperties = (IEnumerable<IValuePropertyPair>)factoryMethod.Invoke(null, new object?[] { configure });

            this.included.Add(new ValuePropertyPairNested(leftProperty, rightProperty, nestedProperties));
        }

        #endregion IValuePropertyJoinConfiguration
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    //public sealed class ValuePropertyPairCollection<L, R> : ValuePropertyPairCollection, IValuePropertyPairCollectionConfiguration<L, R>
    //{
    //    private readonly List<ValuePropertyPairWithCustomRightSetter<R>> overridesDestSetter = new List<ValuePropertyPairWithCustomRightSetter<R>>();

    //    internal ValuePropertyPairCollection(IEnumerable<IValuePropertyPair> propertyPairs) : base(propertyPairs)
    //    {
    //    }

    //    void IValuePropertyPairCollectionConfiguration<L, R>.OverridePairWithDestinationSetter(PropertyInfo rightProperty, Action<R, object> setter)
    //    {
    //        this.overridesDestSetter.Add(new ValuePropertyPairWithCustomRightSetter<R>(rightProperty, setter));
    //    }

    //    protected override IEnumerable<IValuePropertyPair> MergeResultPairs()
    //    {
    //        return base.MergeResultPairs().Concat(this.overridesDestSetter);
    //    }
    //}
}