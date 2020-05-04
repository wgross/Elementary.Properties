using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Setters
{
    /// <summary>
    /// Provides a delegate having a body if "{instance}.{property} = {value}" for a selected property
    /// </summary>
    public class ReflectionSetterFactory
    {
        public static Action<T, V> Of<T, V>(Expression<Func<T, object?>> propertyAccessExpression)
        {
            var propertyInfo = ValueProperties.Single<T>(propertyAccessExpression);
            var setter = propertyInfo.GetSetMethod(true) ?? throw new InvalidOperationException($"property(name='{propertyInfo.Name}') hasn't a setter");

            return (T instance, V value) => setter.Invoke(instance, parameters: new object?[] { value });
        }

        internal static IEnumerable<(string name, Action<T, object?> setter)> Of<T>(IEnumerable<PropertyInfo> properties)
        {
            return properties
                .Where(pi => pi.CanWrite)
                .Select<PropertyInfo, (string, Action<T, object?>)>(pi => (pi.Name, (t, v) => pi.GetSetMethod(true).Invoke(t, new[] { v })));
        }
    }
}