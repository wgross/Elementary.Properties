using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Creates collections of matching value property pairs. In general a matching pair has the same name and type.
    /// It is possible to include reference pairs or exclude value pairs explicitly using the config call back of the factory methods.
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class ValuePropertyPair<L, R>
    {
        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> All(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(InnerJoin(ValueProperty<L>.All(), ValueProperty<R>.All()));
            configure?.Invoke(collection);
            return collection;
        }

        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names. All properties qualify if they are readable
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> ComparableCollection(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(InnerJoin(ValueProperty<L>.AllCanRead(), ValueProperty<R>.AllCanRead()));
            configure?.Invoke(collection);
            return collection;
        }

        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> MappableCollection(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(InnerJoin(ValueProperty<L>.AllCanRead(), ValueProperty<R>.AllCanWrite()));
            configure?.Invoke(collection);
            return collection;
        }

        private static IEnumerable<IValuePropertyPair> InnerJoin(IEnumerable<IValuePropertyCollectionItem> left, IEnumerable<IValuePropertyCollectionItem> right)
        {
            return left.Join(
                inner: right,
                outerKeySelector: l => (l.PropertyName, l.PropertyType),
                innerKeySelector: r => (r.PropertyName, r.PropertyType),
                resultSelector: (r, l) => Value(l.Info, r.Info));
        }

        private static ValuePropertyPairValue Value(PropertyInfo left, PropertyInfo right) => new ValuePropertyPairValue(left, right);
    }

    internal class ValuePropertyPair
    {
        internal static ValuePropertyPairCollection MappableCollection(Type left, Type right)
        {
            var configureDelegateType = typeof(Action<>).MakeGenericType(typeof(IValuePropertyPairCollectionConfiguration<,>).MakeGenericType(left, right));
            var factoryMethod = typeof(ValuePropertyPair<,>).MakeGenericType(left, right).GetMethod("MappableCollection", new[] { configureDelegateType });
            return (ValuePropertyPairCollection)factoryMethod.Invoke(null, new object?[] { null });
        }

        /// <summary>
        /// Creates new <see cref="ValuePropertyPairNested"/>. A property of name <paramref name="name"/> must exist on both sides
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static ValuePropertyPairNested Nested(Type left, Type right, string name)
        {
            var leftReference = Property.Info(left, name);
            var rightReference = Property.Info(right, name);

            return new ValuePropertyPairNested(leftReference, rightReference, MappableCollection(leftReference.PropertyType, rightReference.PropertyType));
        }
    }
}