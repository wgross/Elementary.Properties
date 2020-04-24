using System;
using System.Linq.Expressions;

namespace Elementary.Properties.Setters
{
    public class ReflectionSetterFactory
    {
        public static Action<T, V> Of<T, V>(Expression<Func<T, object>> propertyAccessExpression)
        {
            var memberName = propertyAccessExpression.MemberName();
            var propertyInfo = typeof(T).GetProperty(memberName);
            var setter = propertyInfo?.GetSetMethod(true);

            return (T instance, V value) => setter.Invoke(instance, parameters: new object[] { value });
        }
    }
}