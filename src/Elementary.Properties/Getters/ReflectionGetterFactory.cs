using System;
using System.Linq.Expressions;

namespace Elementary.Properties.Getters
{
    public class ReflectionGetterFactory
    {
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