using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// A value propery pair collection is an inpput to all factories producing operations on property pairs
    /// like comparision assignment.
    /// It allows to manually exclude or override mappings.
    /// </summary>
    public class ValuePropertyPairCollection : IEnumerable<IValuePropertyPair>, IValuePropertyPairConfiguration
    {
        private readonly IEnumerable<IValuePropertyPair> propertyPairs;
        private readonly List<string> exclusions = new List<string>();
        private readonly List<ValuePropertySymmetricPair> overrides = new List<ValuePropertySymmetricPair>();

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
                .Where(pp => !this.overrides.Any(p => p.Left.Equals(pp.Left)))
                .Concat(this.overrides);
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion IEnumerable<ValuePropertyPair>

        #region IValuePropertyJoinConfiguration

        /// <summary>
        /// Exclude property pairs from the collection based on the name of a left side property.
        /// </summary>
        /// <param name="propertyNames"></param>
        void IValuePropertyPairConfiguration.ExcludeLeft(params string[] propertyNames) => this.exclusions.AddRange(propertyNames);

        /// <summary>
        /// Adds a manually configured paring of two properties having. In general these properties have different names
        /// but still must have indentical types.
        /// This excludes all other mappings which the given left side property or right side property participate
        /// </summary>
        /// <param name="leftProperty"></param>
        /// <param name="rightProperty"></param>
        void IValuePropertyPairConfiguration.OverridePair(PropertyInfo leftProperty, PropertyInfo rightProperty) => this.overrides.Add(new ValuePropertySymmetricPair(leftProperty, rightProperty));

        #endregion IValuePropertyJoinConfiguration
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    public sealed class ValuePropertyPairCollection<L, R> : ValuePropertyPairCollection, IValuePropertyMappingConfiguration<L, R>
    {
        private readonly List<ValuePropertyPairWithCustomRightSetter<R>> overridesDestSetter = new List<ValuePropertyPairWithCustomRightSetter<R>>();

        internal ValuePropertyPairCollection(IEnumerable<IValuePropertyPair> propertyPairs) : base(propertyPairs)
        {
        }

        void IValuePropertyMappingConfiguration<L, R>.OverridePairWithDestinationSetter(PropertyInfo rightProperty, Action<R, object> setter)
        {
            this.overridesDestSetter.Add(new ValuePropertyPairWithCustomRightSetter<R>(rightProperty, setter));
        }

        protected override IEnumerable<IValuePropertyPair> MergeResultPairs()
        {
            return base.MergeResultPairs().Concat(this.overridesDestSetter);
        }
    }
}