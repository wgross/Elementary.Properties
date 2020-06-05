using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Elementary.Properties.Selectors
{
    public enum JoinError
    {
        RightPropertyMissing,
        RightPropertyType,
        LeftPropertyMissing
    }

    /// <summary>
    /// Retruevs several sets of properties from type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ValueProperty<T>
    {
        private const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Select a property info of the property specified by the given member access expression <paramref name="memberAccess"/> from type
        /// <typeparamref name="T"/>
        /// </summary>
        public static PropertyInfo Info(Expression<Func<T, object?>> memberAccess) => Info(Property<T>.Info(memberAccess));

        /// <summary>
        /// Select a property info of the property specified by the given name <paramref name="propertyName"/> from type
        /// <typeparamref name="T"/>. the properties type mist be a value type or string.
        /// </summary>
        public static PropertyInfo Info(string propertyName) => Info(Property<T>.Info(propertyName));

        private static PropertyInfo Info(PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (!IsValueType(propertyInfo))
                throw new InvalidOperationException($"typeof of property(name='{propertyInfo.Name}') isn't a value type or string");

            return propertyInfo;
        }

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have public or non-public getter.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanRead(Action<IValuePropertyCollectionConfiguration<T>>? configure = null)
            => All(AllCanRead, configure, IsValueType, CanRead);

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have public or non-public setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure">edit the property list before is will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanWrite(Action<IValuePropertyCollectionConfiguration<T>>? configure = null)
            => All(AllCanWrite, configure, IsValueType, CanWrite);

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have bothe getter and setter (public or non-public).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure">edit the property list before is will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanReadAndWrite(Action<IValuePropertyCollectionConfiguration<T>>? configure = null)
            => All(AllCanReadAndWrite, configure, IsValueType, CanRead, CanWrite);

        public static IEnumerable<IValuePropertyCollectionItem> All(Action<IValuePropertyCollectionConfiguration<T>>? configure, params Func<PropertyInfo, bool>[] predicates)
        {
            var collection = new ValuePropertyCollection<T>(Property<T>.Infos(predicates), predicates, null);
            configure?.Invoke(collection);
            return collection;
        }

        public static IEnumerable<IValuePropertyCollectionItem> All(Func<Type, ValuePropertyCollection> createNestedCollection, Action<IValuePropertyCollectionConfiguration<T>>? configure, params Func<PropertyInfo, bool>[] predicates)
        {
            var collection = new ValuePropertyCollection<T>(Property<T>.Infos(predicates), predicates, createNestedCollection);
            configure?.Invoke(collection);
            return collection;
        }

        #region Handle internal nesting callbacks

        private static ValuePropertyCollection AllCanRead(Type type) => Collection(type);

        private static ValuePropertyCollection AllCanWrite(Type type) => Collection(type);

        private static ValuePropertyCollection AllCanReadAndWrite(Type type) => Collection(type);

        private static ValuePropertyCollection Collection(Type type, [CallerMemberName] string? factoryMethodName = null)
        {
            var configureDelegateType = typeof(Action<>).MakeGenericType(typeof(IValuePropertyCollectionConfiguration<>).MakeGenericType(type));
            var factoryMethod = typeof(ValueProperty<>).MakeGenericType(type).GetMethod(factoryMethodName, new[] { configureDelegateType });
            return (ValuePropertyCollection)factoryMethod.Invoke(null, new object?[] { null });
        }

        #endregion Handle internal nesting callbacks

        #region Query properties from a Type

        private static bool IsValueType(PropertyInfo pi) => pi.PropertyType.IsValueType || typeof(string).Equals(pi.PropertyType);

        private static bool CanRead(PropertyInfo pi) => pi.CanRead;

        private static bool CanWrite(PropertyInfo pi) => pi.CanWrite;

        #endregion Query properties from a Type
    }
}