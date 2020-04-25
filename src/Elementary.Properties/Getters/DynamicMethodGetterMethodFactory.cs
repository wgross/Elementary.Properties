using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Getters
{
    public class DynamicMethodGetterFactory
    {
        public static Func<T, V> Of<T, V>(Expression<Func<T, V>> propertyAccessExpression) => Of<T, V>(PropertyGetter(typeof(T), propertyAccessExpression.MemberName()));

        public static Func<T, V> Of<T, V>(string propertyName) => Of<T, V>(PropertyGetter(typeof(T), propertyName));

        private static Func<T, V> Of<T, V>(MethodInfo propertyGetMethod)
        {
            DynamicMethod getProperty = new DynamicMethod(
                name: $"{propertyGetMethod.Name}_from_{typeof(T)}",
                returnType: typeof(V),
                parameterTypes: new[] { typeof(T) },
                typeof(T).Module);

            getProperty.DefineParameter(1, ParameterAttributes.In, "instance");

            // implement the method body
            var ilGen = getProperty.GetILGenerator(256);
            ilGen.Emit(OpCodes.Ldarg_0); // instance -> stack
            ilGen.Emit(OpCodes.Callvirt, propertyGetMethod);
            ilGen.Emit(OpCodes.Ret);

            // create a delegate from the method
            return (Func<T, V>)getProperty.CreateDelegate(typeof(Func<T, V>));
        }

        private static MethodInfo PropertyGetter(Type t, string pn) => t.GetProperty(pn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(true);
    }
}