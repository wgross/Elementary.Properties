using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Creates collections of matching value property pairs. In general a matching pair has the same name and type.
    /// It is possible to include reference pairs or exclude value pairs explicitly using the config call back of the factory methods.
    /// </summary>
    /// <typeparam name="L"></typeparam>
    /// <typeparam name="R"></typeparam>
    public class ValuePropertyPair<L, R>
        where L : class
        where R : class
    {
        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names. All properties qualify if they are readable
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> ComparableCollection(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(ComparableCollection, InnerJoin(ValueProperty<L>.AllCanRead(), ValueProperty<R>.AllCanRead()));
            configure?.Invoke(collection);
            return collection;
        }

        /// <summary>
        /// Returns all <see cref="IValuePropertyPair"/> having matching types and names.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyPair> MappableCollection(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var collection = new ValuePropertyPairCollection<L, R>(MappableCollection, InnerJoin(ValueProperty<L>.AllCanRead(), ValueProperty<R>.AllCanWrite()));
            configure?.Invoke(collection);
            return collection;
        }

        #region Handle internal nesting callbacks

        private static ValuePropertyPairCollection ComparableCollection(Type left, Type right) => Collection(left, right);

        private static ValuePropertyPairCollection MappableCollection(Type left, Type right) => Collection(left, right);

        private static ValuePropertyPairCollection Collection(Type left, Type right, [CallerMemberName] string factoryMethodName = null)
        {
            var configureDelegateType = typeof(Action<>).MakeGenericType(typeof(IValuePropertyPairCollectionConfiguration<,>).MakeGenericType(left, right));
            var factoryMethod = typeof(ValuePropertyPair<,>).MakeGenericType(left, right).GetMethod(factoryMethodName, new[] { configureDelegateType });
            return (ValuePropertyPairCollection)factoryMethod.Invoke(null, new object?[] { null });
        }

        #endregion Handle internal nesting callbacks

        private static IEnumerable<IValuePropertyPair> InnerJoin(IEnumerable<IValuePropertyCollectionItem> left, IEnumerable<IValuePropertyCollectionItem> right)
        {
            var propertiesOfSameName = left.Join(
                inner: right,
                outerKeySelector: l => l.PropertyName,
                innerKeySelector: r => r.PropertyName,
                resultSelector: (l, r) => (left: l.Info, right: r.Info));

            foreach (var pair in propertiesOfSameName)
                if (TypesAreMappable(pair.left.PropertyType, pair.right.PropertyType))
                    yield return Value(pair.left, pair.right);
        }

        private static bool TypesAreMappable(Type leftType, Type rightType)
        {
            // accept identical type
            if (leftType == rightType)
                return true;

            // accept Nullable<leftType> at right side
            if (rightType.IsGenericType)
                if (rightType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    if (Nullable.GetUnderlyingType(rightType).Equals(leftType))
                        return true;

            return false;
        }

        private static ValuePropertyPairValue Value(PropertyInfo left, PropertyInfo right) => new ValuePropertyPairValue(left, right);
    }
}