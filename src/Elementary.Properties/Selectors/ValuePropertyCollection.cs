using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// Refresents a reference property in a <see cref="ValuePropertyCollection{T}"/>
    /// </summary>
    public sealed class ValuePropertyCollectionReference : IValuePropertyCollectionItem
    {
        internal ValuePropertyCollectionReference(PropertyInfo property, IEnumerable<IValuePropertyCollectionItem> subProperties)
        {
            this.Info = property;
            this.ValueProperties = subProperties.ToArray();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Info { get; }

        /// <summary>
        /// The value properties contained in the referenced instance
        /// </summary>
        public IEnumerable<IValuePropertyCollectionItem> ValueProperties { get; }
    }

    /// <summary>
    /// An editable collection of property to build unary operation for.ed
    /// </summary>
    public class ValuePropertyCollection<T> : IEnumerable<IValuePropertyCollectionItem>, IValuePropertyCollectionConfig<T>
    {
        private readonly IEnumerable<IValuePropertyCollectionItem> leafProperties;
        private readonly Func<PropertyInfo, bool>[] predicates;
        private readonly List<string> excludedLeaves = new List<string>();
        private readonly List<PropertyInfo> includedLeaves = new List<PropertyInfo>();
        private readonly List<ValuePropertyCollectionReference> includedNodes = new List<ValuePropertyCollectionReference>();

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties, IEnumerable<Func<PropertyInfo, bool>> prediates)
        {
            this.leafProperties = properties.Select(pi => (IValuePropertyCollectionItem)new ValuePropertyCollectionValue(pi)).ToArray();
            this.predicates = prediates.ToArray();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IValuePropertyCollectionItem> GetEnumerator()
        {
            var result = this.leafProperties.ToDictionary(keySelector: p => p.Info.Name);
            this.excludedLeaves.ForEach(ex => result.Remove(ex));
            this.includedLeaves.ForEach(inc => result.Add(inc.Name, new ValuePropertyCollectionValue(inc)));
            this.includedNodes.ForEach(i => result.Add(i.Info.Name, i));
            return result.Select(kv => kv.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region IValuePropertyCollectionConfig

        void IValuePropertyCollectionConfig<T>.Exclude(params string[] propertyNames) => this.excludedLeaves.AddRange(propertyNames);

        void IValuePropertyCollectionConfig<T>.Exclude(Expression<Func<T, object?>> propertyAccess) => this.excludedLeaves.Add(Property<T>.Info(propertyAccess).Name);

        void IValuePropertyCollectionConfig<T>.IncludeValuesOf(Expression<Func<T, object?>> propertyAccess, Action<IValuePropertyCollectionConfig<T>>? configure)
        {
            var property = Property<T>.Info(propertyAccess);

            var configureDelegateType = typeof(Action<>).MakeGenericType(
                typeof(IValuePropertyCollectionConfig<>).MakeGenericType(property.PropertyType));

            var factoryMethod = typeof(ValueProperty<>)
                .MakeGenericType(property.PropertyType)
                .GetMethod("All", new[] { configureDelegateType, typeof(Func<PropertyInfo, bool>[]) });

            var collection = (IEnumerable<IValuePropertyCollectionItem>)factoryMethod.Invoke(null, new object?[] { configure, this.predicates });

            this.includedNodes.Add(new ValuePropertyCollectionReference(property, collection));
        }

        #endregion IValuePropertyCollectionConfig
    }
}