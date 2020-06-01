using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Represents a value property property in a <see cref="ValuePropertyCollection{T}"/>
    /// </summary>
    public sealed class ValuePropertyCollectionValue : IValuePropertyCollectionItem
    {
        internal ValuePropertyCollectionValue(PropertyInfo property)
        {
            this.Info = property;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Info { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string PropertyName => this.Info.Name;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type PropertyType => this.Info.PropertyType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInfo Getter() => this.Info.GetGetMethod(nonPublic: true);
    }

    /// <summary>
    /// Refresents a reference property in a <see cref="ValuePropertyCollection{T}"/>
    /// </summary>
    public sealed class ValuePropertyCollectionReference : IValuePropertyCollectionItem
    {
        internal ValuePropertyCollectionReference(PropertyInfo referenceProperty, ValuePropertyCollection nestedProperties)
        {
            this.Info = referenceProperty;
            this.NestedProperties = nestedProperties;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Info { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string PropertyName => this.Info.Name;

        /// <summary>
        /// The value properties contained in the referenced instance
        /// </summary>
        public ValuePropertyCollection NestedProperties { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Type PropertyType => this.Info.PropertyType;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public MethodInfo Getter() => this.Info.GetGetMethod(nonPublic: true);
    }

    /// <summary>
    /// An uneditable collection of properties to build unary operation for
    /// </summary>
    public class ValuePropertyCollection : IEnumerable<IValuePropertyCollectionItem>
    {
        private readonly IEnumerable<IValuePropertyCollectionItem> leafProperties;
        private readonly List<string> excludedLeaves = new List<string>();

        internal List<ValuePropertyCollectionReference> Included { get; } = new List<ValuePropertyCollectionReference>();

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties)
        {
            this.leafProperties = properties.Select(pi => (IValuePropertyCollectionItem)new ValuePropertyCollectionValue(pi)).ToArray();
        }

        #region IEnumerable<IValuePropertyCollectionItem>

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IValuePropertyCollectionItem> GetEnumerator()
        {
            var result = this.leafProperties.ToDictionary(keySelector: p => p.Info.Name);
            this.excludedLeaves.ForEach(ex => result.Remove(ex));
            this.Included.ForEach(i => result.Add(i.Info.Name, i));
            return result.Select(kv => kv.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #endregion IEnumerable<IValuePropertyCollectionItem>

        internal void Exclude(string propertyName)
        {
            if (this.leafProperties.Any(pi => pi.PropertyName.Equals(propertyName)))
            {
                this.excludedLeaves.Add(propertyName);
            }
            else throw new ArgumentException($"Property '{propertyName}' doesn't exist in collection.", nameof(propertyName));
        }

        internal void Include(ValuePropertyCollectionReference nestedPropeties) => this.Included.Add(nestedPropeties);
    }

    /// <summary>
    /// An editable collection of properties to build unary operation for
    /// </summary>
    public class ValuePropertyCollection<T> : ValuePropertyCollection, IValuePropertyCollectionConfig<T>
    {
        private readonly Func<PropertyInfo, bool>[] predicates;

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties, IEnumerable<Func<PropertyInfo, bool>> predicates)
            : base(properties)
        {
            this.predicates = predicates.ToArray();
        }

        #region IValuePropertyCollectionConfig<T>

        void IValuePropertyCollectionConfig<T>.ExcludeValue(Expression<Func<T, object?>> propertyAccess)
        {
            var propertyPath = Property<T>.InfoPath(propertyAccess).ToArray();

            var currentReference = TraverseToNestingParent(
                propertyPath: propertyPath[..^1],
                includeInCurrentCollection: (r, p) => throw new InvalidOperationException($"Exclude property(name='{propertyPath[^1].Name}') failed: Nested property(name='{p.Name}') isn't included"));

            if (currentReference is null)
                this.Exclude(propertyPath[^1].Name);
            else
                currentReference.Exclude(propertyPath[^1].Name);
        }

        void IValuePropertyCollectionConfig<T>.IncludeNested(Expression<Func<T, object?>> propertyAccess)
        {
            ValuePropertyCollection includeInCurrentCollection(ValuePropertyCollection? currentReference, PropertyInfo property)
            {
                var newNestedProperties = new ValuePropertyCollectionReference(
                    referenceProperty: property,
                    nestedProperties: ValueProperty.All(property.PropertyType, this.predicates));

                if (currentReference is null)
                    this.Include(newNestedProperties);
                else
                    currentReference.Include(newNestedProperties);

                return newNestedProperties.NestedProperties;
            }

            var propertyPath = Property<T>.InfoPath(propertyAccess).ToArray();

            var nestingParent = TraverseToNestingParent(
                propertyPath: propertyPath[..^1],
                includeInCurrentCollection: includeInCurrentCollection);

            includeInCurrentCollection(nestingParent, propertyPath[^1]);
        }

        private ValuePropertyCollection? TraverseToNestingParent(IEnumerable<PropertyInfo> propertyPath, Func<ValuePropertyCollection?, PropertyInfo, ValuePropertyCollection> includeInCurrentCollection)
        {
            ValuePropertyCollection currentNestingParent = this;

            foreach (var property in propertyPath)
            {
                // check if property is already included
                var included = currentNestingParent
                    .Included
                    .SingleOrDefault(n => n.PropertyName.Equals(property.Name))
                    ?.NestedProperties;

                if (included is null)
                {
                    // property isn't included
                    // => run injected strategy to handle this case.
                    // => strategy must return new nesting parent or throw everything else breaks the loop
                    currentNestingParent = includeInCurrentCollection(currentNestingParent, property);
                }
                else if (included is ValuePropertyCollection nestingChildren)
                {
                    // the property is already included
                    // => descend into collection of nested properties to include the next property in the property path
                    currentNestingParent = nestingChildren;
                }
                else throw new InvalidOperationException($"Properties of type('{property.PropertyType}') can't be included");
            }

            return currentNestingParent;
        }

        #endregion IValuePropertyCollectionConfig<T>
    }
}