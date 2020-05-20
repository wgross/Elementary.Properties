using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
    public class ValueProperty<T>
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
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> All(Action<IValuePropertyCollectionConfig<T>>? configure = null)
            => All(configure, IsValueType);

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have public or non-public getter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure">edit the property list before is will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanRead(Action<IValuePropertyCollectionConfig<T>>? configure = null)
            => All(configure, IsValueType, CanRead);

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have public or non-public setter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure">edit the property list before is will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanWrite(Action<IValuePropertyCollectionConfig<T>>? configure = null)
            => All(configure, IsValueType, CanWrite);

        /// <summary>
        /// Retrieves all value (or string) type properties of <typeparamref name="T"/> which have bothe getter and setter (public or non-public).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure">edit the property list before is will be returned</param>
        /// <returns></returns>
        public static IEnumerable<IValuePropertyCollectionItem> AllCanReadAndWrite(Action<IValuePropertyCollectionConfig<T>>? configure = null)
            => All(configure, IsValueType, CanRead, CanWrite);

        public static IEnumerable<IValuePropertyCollectionItem> All(Action<IValuePropertyCollectionConfig<T>>? configure, params Func<PropertyInfo, bool>[] predicates)
        {
            var collection = new ValuePropertyCollection<T>(Property<T>.Infos(predicates), predicates);
            configure?.Invoke(collection);
            return collection;
        }

        public static ValuePropertyPairCollection<T, D> Join<D>(IEnumerable<IValuePropertyCollectionItem> leftProperties, IEnumerable<IValuePropertyCollectionItem> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null, Action<IValuePropertyPairConfiguration> configure = null)
        {
            var collection = new ValuePropertyPairCollection<T, D>(JoinImpl(leftProperties, rightProperties, onError));
            configure?.Invoke(collection);
            return collection;
        }

        private static IEnumerable<ValuePropertySymmetricPair> JoinImpl(IEnumerable<IValuePropertyCollectionItem> leftProperties, IEnumerable<IValuePropertyCollectionItem> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null)
        {
            onError ??= delegate { };

            var rightProperyMap = rightProperties.ToDictionary(pi => pi.Info.Name);

            foreach (var lpi in leftProperties)
            {
                var exists = rightProperyMap.TryGetValue(lpi.Info.Name, out var rpi);
                if (!exists)
                {
                    onError(JoinError.RightPropertyMissing, (lpi.Info.Name, lpi.Info.PropertyType));
                }
                else if (lpi.Info.PropertyType != rpi.Info.PropertyType)
                {
                    rightProperyMap.Remove(rpi.Info.Name);
                    onError(JoinError.RightPropertyType, (lpi.Info.Name, lpi.Info.PropertyType));
                }
                else
                {
                    rightProperyMap.Remove(rpi.Info.Name);
                    yield return new ValuePropertySymmetricPair(lpi.Info, rpi.Info);
                }
            }
            foreach (var rpi in rightProperyMap.Values)
            {
                onError(JoinError.LeftPropertyMissing, (rpi.Info.Name, rpi.Info.PropertyType));
            }
            yield break;
        }

        #region Query properties from a Type

        private static bool IsValueType(PropertyInfo pi) => pi.PropertyType.IsValueType || typeof(string).Equals(pi.PropertyType);

        private static bool CanRead(PropertyInfo pi) => pi.CanRead;

        private static bool CanWrite(PropertyInfo pi) => pi.CanWrite;

        #endregion Query properties from a Type
    }
}