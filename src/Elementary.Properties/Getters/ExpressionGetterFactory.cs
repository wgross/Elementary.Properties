using System;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace Elementary.Properties.Getters
{
    public class ExpressionGetterFactory
    {
        public static Expression<Func<T, V>> Of<T, V>(Expression<Func<T, V>> propertyAccessExpression)
            => GetPropertyValue<T, V>(PropertyInfo<T>(propertyAccessExpression.MemberName()));

        private static PropertyInfo PropertyInfo<T>(string name) => typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static Expression<Func<T, V>> GetPropertyValue<T, V>(PropertyInfo property) => GetPropertyValue<T, V>(Parameter(typeof(T), "instance"), property);

        private static Expression<Func<T, V>> GetPropertyValue<T, V>(ParameterExpression instance, PropertyInfo property)
           => Expression.Lambda<Func<T, V>>(body: CastPropertyValue<V>(instance, property), parameters: instance);

        private static UnaryExpression CastPropertyValue<V>(ParameterExpression instance, PropertyInfo property)
        {
            if (property.PropertyType.GetTypeInfo().IsValueType)
                return Convert(Property(instance, property), typeof(V));
            else
                return TypeAs(Property(instance, property), typeof(V));
        }
    }
}