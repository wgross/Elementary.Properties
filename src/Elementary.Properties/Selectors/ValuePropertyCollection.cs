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
    public sealed class ValuePropertyNested : IValuePropertyCollectionItem
    {
        internal ValuePropertyNested(PropertyInfo referenceProperty, ValuePropertyCollection nestedProperties)
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
        private Func<PropertyInfo, bool>[] predicates;

        internal List<ValuePropertyNested> Included { get; } = new List<ValuePropertyNested>();

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties)
        {
            this.leafProperties = properties.Select(pi => (IValuePropertyCollectionItem)new ValuePropertyCollectionValue(pi)).ToArray();
        }

        public ValuePropertyCollection(IEnumerable<PropertyInfo> properties, Func<PropertyInfo, bool>[] predicates) : this(properties)
        {
            this.predicates = predicates;
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

        internal void Include(ValuePropertyNested nestedPropeties) => this.Included.Add(nestedPropeties);
    }

    /// <summary>
    /// An editable collection of properties to build unary operation for
    /// </summary>
    public class ValuePropertyCollection<T> : ValuePropertyCollection, IValuePropertyCollectionConfiguration<T>
    {
        private readonly Func<PropertyInfo, bool>[] predicates;
        private readonly Func<Type, ValuePropertyCollection> createPropertyCollection;

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties, IEnumerable<Func<PropertyInfo, bool>> predicates, Func<Type, ValuePropertyCollection> createPropertyCollection = null)
            : base(properties)
        {
            this.predicates = predicates.ToArray();
            this.createPropertyCollection = createPropertyCollection;
        }

        #region IValuePropertyCollectionConfig<T>

        void IValuePropertyCollectionConfiguration<T>.ExcludeValue(Expression<Func<T, object?>> propertyAccess)
        {
            var propertyPath = Property<T>.InfoPath(propertyAccess).ToArray();

            var currentReference = TraverseToNestingParent(
                propertyPath: propertyPath[..^1],
                includeInCurrentCollection: (r, p) => throw new InvalidOperationException($"Exclude property(name='{propertyPath[^1].Name}') failed: Nested property(name='{p.Name}') isn't included"));

            if (currentReference is null)
                this.Exclude(propertyPath[^1].Name);
            else
                currentReference.NestedProperties.Exclude(propertyPath[^1].Name);
        }

        void IValuePropertyCollectionConfiguration<T>.IncludeNested(Expression<Func<T, object?>> propertyAccess)
        {
            ValuePropertyNested includeInCurrentCollection(ValuePropertyNested? parent, PropertyInfo referenceProperty)
            {
                if (parent is null)
                {
                    var nested = this.NewValuePropertyNested(typeof(T), referenceProperty.Name);
                    this.Include(nested);
                    return nested;
                }
                else
                {
                    var nested = this.NewValuePropertyNested(type: parent.PropertyType, referenceProperty.Name);
                    parent.NestedProperties.Include(nested);
                    return nested;
                }
            }

            var propertyPath = Property<T>.InfoPath(propertyAccess).ToArray();
            var nestingParent = TraverseToNestingParent(propertyPath: propertyPath[..^1], includeInCurrentCollection: includeInCurrentCollection);

            includeInCurrentCollection(nestingParent, propertyPath[^1]);
        }

        private ValuePropertyNested? TraverseToNestingParent(IEnumerable<PropertyInfo> propertyPath, Func<ValuePropertyNested?, PropertyInfo, ValuePropertyNested> includeInCurrentCollection)
        {
            ValuePropertyNested? currentNestingParent = null;

            ValuePropertyNested getNested(PropertyInfo property)
            {
                if (currentNestingParent is null)
                {
                    return this.Included.SingleOrDefault(n => n.PropertyName.Equals(property.Name));
                }
                else
                {
                    // check if property is already included
                    return currentNestingParent.NestedProperties.Included.SingleOrDefault(n => n.PropertyName.Equals(property.Name));
                }
            }

            foreach (var property in propertyPath)
            {
                ValuePropertyNested? included = getNested(property);

                if (included is null)
                {
                    // property isn't included
                    // => run injected strategy to handle this case.
                    // => strategy must return new nesting parent or throw everything else breaks the loop
                    currentNestingParent = includeInCurrentCollection(currentNestingParent, property);
                }
                else if (included is ValuePropertyNested nestingChildren)
                {
                    // the property is already included
                    // => descend into collection of nested properties to include the next property in the property path
                    currentNestingParent = nestingChildren;
                }
                else throw new InvalidOperationException($"Properties of type('{property.PropertyType}') can't be included");
            }

            return currentNestingParent;
        }

        private ValuePropertyNested NewValuePropertyNested(Type type, string propertyName)
        {
            var referenceProperty = Property.Info(type, propertyName);

            return new ValuePropertyNested(referenceProperty, this.createPropertyCollection(referenceProperty.PropertyType));
        }

        #endregion IValuePropertyCollectionConfig<T>
    }
}