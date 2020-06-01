using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Base type of <see cref="ValuePropertyPairCollection{L, R}"/> containing all functions which
    /// don't require the type parameters.
    /// </summary>
    public abstract class ValuePropertyPairCollection : IEnumerable<IValuePropertyPair>
    {
        private readonly IEnumerable<IValuePropertyPair> propertyPairs;
        private readonly List<string> exclusions = new List<string>();

        internal List<ValuePropertyPairNested> Included { get; } = new List<ValuePropertyPairNested>();

        internal ValuePropertyPairCollection(IEnumerable<IValuePropertyPair> propertyPairs) => this.propertyPairs = propertyPairs;

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IValuePropertyPair> GetEnumerator() => this.MergeResultPairs().GetEnumerator();

        private IEnumerable<IValuePropertyPair> MergeResultPairs()
        {
            return this.propertyPairs
                .Where(p => !this.exclusions.Contains(p.Left.Name))
                .Where(pp => !this.Included.Any(p => p.Left.Equals(pp.Left)))
                .Concat(this.Included);
        }

        internal void Exclude(string propertyName)
        {
            this.exclusions.Add(propertyName);
        }

        internal void Include(ValuePropertyPairNested valuePropertyPairNested)
        {
            this.Included.Add(valuePropertyPairNested);
        }
    }

    /// <summary>
    /// A value propery pair collection is an inpput to all factories producing operations on property pairs
    /// like comparision assignment.
    /// It allows to manually exclude or override mappings.
    /// </summary>
    public class ValuePropertyPairCollection<L, R> : ValuePropertyPairCollection, IValuePropertyPairCollectionConfiguration<L, R>
    {
        internal ValuePropertyPairCollection(IEnumerable<IValuePropertyPair> propertyPairs)
            : base(propertyPairs)
        { }

        #region IValuePropertyJoinConfiguration

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        void IValuePropertyPairCollectionConfiguration<L>.ExcludeLeftValue(Expression<Func<L, object?>> propertyAccess)
        {
            var propertyPath = Property<L>.InfoPath(propertyAccess).ToArray();

            var currentReference = TraverseToNestingParent(
                propertyPath: propertyPath[..^1],
                includeInCurrentCollection: (r, p) => throw new InvalidOperationException($"Exclude property(name='{propertyPath[^1].Name}') failed: Nested property(name='{p.Name}') isn't included"));

            if (currentReference is null)
                this.Exclude(propertyPath[^1].Name);
            else
                currentReference.NestedPairs.Exclude(propertyPath[^1].Name);
        }

        private ValuePropertyPairNested? TraverseToNestingParent(IEnumerable<PropertyInfo> propertyPath, Func<ValuePropertyPairNested?, PropertyInfo, ValuePropertyPairNested> includeInCurrentCollection)
        {
            ValuePropertyPairNested? currentNestingParent = null;

            ValuePropertyPairNested getNestedPair(PropertyInfo property)
            {
                if (currentNestingParent is null)
                {
                    return this.Included.SingleOrDefault(n => n.LeftPropertyName.Equals(property.Name));
                }
                else
                {
                    // check if property is already included
                    return currentNestingParent.NestedPairs.Included.SingleOrDefault(n => n.LeftPropertyName.Equals(property.Name));
                }
            }

            foreach (var property in propertyPath)
            {
                ValuePropertyPairNested? included = getNestedPair(property);

                if (included is null)
                {
                    // property isn't included
                    // => run injected strategy to handle this case.
                    // => strategy must return new nesting parent or throw everything else breaks the loop
                    currentNestingParent = includeInCurrentCollection(currentNestingParent, property);
                }
                else if (included is ValuePropertyPairNested nestingChildren)
                {
                    // the property is already included
                    // => descend into collection of nested properties to include the next property in the property path
                    currentNestingParent = nestingChildren;
                }
                else throw new InvalidOperationException($"Properties of type('{property.PropertyType}') can't be included");
            }

            return currentNestingParent;
        }

        void IValuePropertyPairCollectionConfiguration<L, R>.IncludeNested(Expression<Func<L, object?>> propertyAccess)
        {
            ValuePropertyPairNested includeInCurrentCollection(ValuePropertyPairNested? parentPair, PropertyInfo leftReferenceProperty/*, PropertyInfo rightReferenceProperty*/)
            {
                if (parentPair is null)
                {
                    // the nesting doesn't have a parent pair.
                    // => reference property is immediatly under L
                    var nestedPair = ValuePropertyPair.Nested(typeof(L), typeof(R), leftReferenceProperty.Name);
                    this.Include(nestedPair);
                    return nestedPair;
                }
                else
                {
                    var nestedPair = ValuePropertyPair.Nested(left: parentPair.LeftPropertyType, right: parentPair.RightPropertyType, leftReferenceProperty.Name);
                    parentPair.NestedPairs.Include(nestedPair);
                    return nestedPair;
                }
            }

            // the left property path is leading. It is expeced thet the right side follows the same structure
            var leftPropertyPath = Property<L>.InfoPath(propertyAccess).ToArray();

            var nestingParent = TraverseToNestingParent(
                propertyPath: leftPropertyPath[..^1],
                includeInCurrentCollection: includeInCurrentCollection);

            includeInCurrentCollection(nestingParent, leftPropertyPath[^1]);
        }

        //void IValuePropertyPairCollectionConfiguration<L, R>.IncludeNested(Expression<Func<L, object?>> propertyAccess, Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure)
        //{
        //    var leftProperty = Property<L>.Info(propertyAccess);
        //    var rightProperty = Property<R>.Info(leftProperty.Name);

        //    var configureDelegateType = typeof(Action<>).MakeGenericType(
        //        typeof(IValuePropertyPairCollectionConfiguration<,>).MakeGenericType(leftProperty.PropertyType, rightProperty.PropertyType));

        //    // make factory method call for nested properties
        //    var factoryMethod = typeof(ValuePropertyPair<,>)
        //        .MakeGenericType(leftProperty.PropertyType, rightProperty.PropertyType)
        //        .GetMethod("All", new[] { configureDelegateType });

        //    var nestedProperties = (ValuePropertyPairCollection)factoryMethod.Invoke(null, new object?[] { configure });

        //    this.Include(new ValuePropertyPairNested(leftProperty, rightProperty, nestedProperties));
        //}

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