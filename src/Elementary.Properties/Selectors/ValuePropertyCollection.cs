using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public sealed class ValuePropertyCollectionLeafNode : IValuePropertyCollectionItem
    {
        internal ValuePropertyCollectionLeafNode(PropertyInfo property)
        {
            this.Property = property;
        }

        public PropertyInfo Property { get; }
    }

    public sealed class ValuePropertyCollectionInnerNode : IValuePropertyCollectionItem
    {
        internal ValuePropertyCollectionInnerNode(PropertyInfo property, IEnumerable<IValuePropertyCollectionItem> subProperties)
        {
            this.Property = property;
            this.ValueProperties = subProperties.ToArray();
        }

        public PropertyInfo Property { get; }

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
        private readonly List<ValuePropertyCollectionInnerNode> includedNodes = new List<ValuePropertyCollectionInnerNode>();

        internal ValuePropertyCollection(IEnumerable<PropertyInfo> properties, IEnumerable<Func<PropertyInfo, bool>> prediates)
        {
            this.leafProperties = properties.Select(pi => (IValuePropertyCollectionItem)new ValuePropertyCollectionLeafNode(pi)).ToArray();
            this.predicates = prediates.ToArray();
        }

        public IEnumerator<IValuePropertyCollectionItem> GetEnumerator()
        {
            var result = this.leafProperties.ToDictionary(keySelector: p => p.Property.Name);
            this.excludedLeaves.ForEach(ex => result.Remove(ex));
            this.includedLeaves.ForEach(inc => result.Add(inc.Name, new ValuePropertyCollectionLeafNode(inc)));
            this.includedNodes.ForEach(i => result.Add(i.Property.Name, i));
            return result.Select(kv => kv.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region IValuePropertyCollectionConfig

        void IValuePropertyCollectionConfig<T>.Exclude(params string[] propertyNames) => this.excludedLeaves.AddRange(propertyNames);

        void IValuePropertyCollectionConfig<T>.IncludeValuesOf(Expression<Func<T, object?>> propertyAccess, Action<IValuePropertyCollectionConfig<T>>? configure)
        {
            var property = Property<T>.Info(propertyAccess);

            var configureDelegateType = typeof(Action<>).MakeGenericType(
                typeof(IValuePropertyCollectionConfig<>).MakeGenericType(property.PropertyType));

            var factoryMethod = typeof(ValueProperty<>)
                .MakeGenericType(property.PropertyType)
                .GetMethod("All", new[] { configureDelegateType, typeof(Func<PropertyInfo, bool>[]) });

            var collection = (IEnumerable<IValuePropertyCollectionItem>)factoryMethod.Invoke(null, new object?[] { configure, this.predicates });

            this.includedNodes.Add(new ValuePropertyCollectionInnerNode(property, collection));
        }

        #endregion IValuePropertyCollectionConfig
    }
}