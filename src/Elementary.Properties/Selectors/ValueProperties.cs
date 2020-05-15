using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Elementary.Properties.Selectors.PropertyInfos;

namespace Elementary.Properties.Selectors
{
    public enum JoinError
    {
        RightPropertyMissing,
        RightPropertyType,
        LeftPropertyMissing
    }

    public class ValueProperties
    {
        private const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static PropertyInfo Single<T>(Expression<Func<T, object?>> memberAccess)
        {
            if (memberAccess is null)
                throw new ArgumentNullException(nameof(memberAccess));

            return Single(typeof(T), PropertyFromMemberAccess<T>(memberAccess));
        }

        public static PropertyInfo Single<T>(string propertyName) => Single(typeof(T), PropertyInfo<T>(propertyName));

        private static PropertyInfo Single(Type type, PropertyInfo propertyInfo)
        {
            if (propertyInfo is null)
                throw new ArgumentNullException(nameof(propertyInfo));

            if (!IsValueType(propertyInfo))
                throw new InvalidOperationException($"typeof of property(name='{propertyInfo.Name}') isn't a value type or string");

            return propertyInfo;
        }

        public static IEnumerable<PropertyInfo> All<T>(Action<IValuePropertiesCollectionConfig>? configure = null)
        {
            var collection = PropertyInfos(typeof(T), defaultBindingFlags, IsValueType);
            configure?.Invoke(collection);
            return collection;
        }

        public static IEnumerable<PropertyInfo> AllCanRead<T>() => PropertyInfos(typeof(T), defaultBindingFlags, IsValueType, CanRead);

        public static IEnumerable<PropertyInfo> AllCanWrite<T>() => PropertyInfos(typeof(T), defaultBindingFlags, IsValueType, CanWrite);

        public static ValuePropertyPairCollection Join(IEnumerable<PropertyInfo> leftProperties, IEnumerable<PropertyInfo> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null, Action<IValuePropertyJoinConfiguration> configure = null)
        {
            var collection = new ValuePropertyPairCollection(JoinImpl(leftProperties, rightProperties, onError));
            configure?.Invoke(collection);
            return collection;
        }

        private static IEnumerable<ValuePropertyPair> JoinImpl(IEnumerable<PropertyInfo> leftProperties, IEnumerable<PropertyInfo> rightProperties, Action<JoinError, (string name, Type propertyType)>? onError = null)
        {
            onError ??= delegate { };

            var rightProperyMap = rightProperties.ToDictionary(pi => pi.Name);

            foreach (var lpi in leftProperties)
            {
                var exists = rightProperyMap.TryGetValue(lpi.Name, out var rpi);
                if (!exists)
                {
                    onError(JoinError.RightPropertyMissing, (lpi.Name, lpi.PropertyType));
                }
                else if (lpi.PropertyType != rpi.PropertyType)
                {
                    rightProperyMap.Remove(rpi.Name);
                    onError(JoinError.RightPropertyType, (lpi.Name, lpi.PropertyType));
                }
                else
                {
                    rightProperyMap.Remove(rpi.Name);
                    yield return new ValuePropertyPair(lpi, rpi);
                }
            }
            foreach (var rpi in rightProperyMap.Values)
            {
                onError(JoinError.LeftPropertyMissing, (rpi.Name, rpi.PropertyType));
            }
            yield break;
        }

        #region Query properties from a Type

        private static ValuePropertyCollection PropertyInfos(Type type, BindingFlags defaultBindingFlags, params Func<PropertyInfo, bool>[] filters)
        {
            var all = type
                .GetProperties(defaultBindingFlags)
                .AsEnumerable();
            return new ValuePropertyCollection(filters.Aggregate(all, (pi, f) => pi.Where(f)));
        }

        public static object Intersect<T1, T2>()
        {
            throw new NotImplementedException();
        }

        private static bool IsValueType(PropertyInfo pi) => pi.PropertyType.IsValueType || typeof(string).Equals(pi.PropertyType);

        private static bool CanRead(PropertyInfo pi) => pi.CanRead;

        private static bool CanWrite(PropertyInfo pi) => pi.CanWrite;

        private static PropertyInfo PropertyInfo<T>(string propertyName)
            => typeof(T).GetProperty(propertyName, defaultBindingFlags) ?? throw new InvalidOperationException($"typeof of property(name='{propertyName}') doen't exist");

        #endregion Query properties from a Type
    }
}