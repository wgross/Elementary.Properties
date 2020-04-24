using System;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Expressions.Expression;

namespace Elementary.Properties.Setters
{
    public class ExpressionSetterFactory
    {
        public static Expression<Action<T, V>> Of<T, V>(Expression<Func<T, object>> propertyAccessExpression)
            => SetPropertyValueLambda<T, V>(typeof(T).GetProperty(propertyAccessExpression.MemberName()));

        private static Expression<Action<T, V>> SetPropertyValueLambda<T, V>(PropertyInfo property)
        {
            var parameters = (
                instance: Parameter(typeof(T), "instance"),
                value: Parameter(typeof(V), "value")
            );

            return Lambda<Action<T, V>>(
                body: SetPropertyValueBody(parameters.instance, property, parameters.value),
                parameters.instance,
                parameters.value);
        }

        private static BinaryExpression SetPropertyValueBody(ParameterExpression instance, PropertyInfo property, ParameterExpression value)
          => Assign(Property(instance, property), ConvertValueToPropertyType(value, property.PropertyType.GetTypeInfo()));

        private static UnaryExpression ConvertValueToPropertyType(ParameterExpression value, TypeInfo valueType)
            => valueType.IsValueType ? Convert(value, valueType) : TypeAs(value, valueType);
    }
}