using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Getters
{
    /// <summary>
    /// Provides a delegate having a body if "return {instance}.{property}" for a selected property
    /// </summary>
    public class ReflectionGetterFactory
    {
        public static Func<T, V> Of<T, V>(Expression<Func<T, V>> propertyAccessExpression)
        {
            var memberName = propertyAccessExpression.MemberName();
            var propertyInfo = typeof(T)
                .GetProperty(memberName)
                ?.GetGetMethod(true);

            return instance => (V)propertyInfo.Invoke(instance, parameters: null);
        }

        internal static IEnumerable<(string name, Func<T, object?> getter)> Of<T>(IEnumerable<PropertyInfo> properties)
        {
            return properties
                .Where(pi => pi.CanRead)
                .Select<PropertyInfo, (string, Func<T, object?>)>(pi => (pi.Name, t => pi.GetGetMethod(true).Invoke(t, null)));
        }
    }
}