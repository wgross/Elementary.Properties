using System;
using System.Linq.Expressions;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyCollectionConfig<T>
    {
        /// <summary>
        /// Excludes a property from the collection by name.
        /// </summary>
        /// <param name="propertyNames"></param>
        void Exclude(params string[] propertyNames);

        /// <summary>
        /// Excludes a property from the collection using a member access expression.
        /// The exppression must not be chained.
        /// </summary>
        /// <param name="p"></param>
        void Exclude(Expression<Func<T, object?>> p);

        /// <summary>
        /// Include the value properties of this property in the collection.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="configure"></param>
        void IncludeValuesOf(Expression<Func<T, object?>> property, Action<IValuePropertyCollectionConfig<T>>? configure = null);
    }
}