using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Getters
{
    public class ReflectionGetterFactory
    {
        private class PropertyInfoFactory<T>
        {
            /// <summary>
            /// Retrieve the <see cref="PropertyInfo"/> from the property accessed by the specified
            /// expression. The property must be bound to the instance but can have a private setter.
            /// </summary>
            /// <param name="propertyAccessExpression"></param>
            /// <returns></returns>
            public static PropertyInfo Property(Expression<Func<T, object>> propertyAccessExpression)
                => Property<object>(propertyAccessExpression);

            internal static PropertyInfo Property<V>(Expression<Func<T, V>> propertyAccessExpression)
            {
                var depth = 0;
                Expression exp = propertyAccessExpression;
                while (depth < 3) // lambda -> convert -> member
                {
                    switch (exp)
                    {
                        case LambdaExpression lambda:
                            exp = lambda.Body;
                            break;

                        case UnaryExpression convert when (convert.NodeType == ExpressionType.Convert):
                            exp = convert.Operand;
                            break;

                        case MemberExpression memberAccess:
                            return Property(memberAccess.Member.Name);

                        default:
                            throw new InvalidOperationException($"property access contained unexpected expression: {exp.NodeType}");
                    }

                    depth++;
                }
                throw new InvalidOperationException("property access expression was too deep");
            }

            public static PropertyInfo Property(string name) => typeof(T).GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        public static Func<T, V> Of<T, V>(Expression<Func<T, V>> propertyAccessExpression)
        {
            var memberName = propertyAccessExpression.MemberName();
            var propertyInfo = typeof(T)
                .GetProperty(memberName)
                ?.GetGetMethod();

            return instance => (V)propertyInfo.Invoke(instance, parameters: null);
        }
    }
}