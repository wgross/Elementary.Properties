using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Elementary.Properties;

namespace Elementary.Properties.Setters
{
    public class DynamicMethodSetterFactory
    {
        public static Action<T, V> Of<T, V>(Expression<Func<T, V>> propertyAccessExpression) => Of<T, V>(PropertySetter(typeof(T), propertyAccessExpression.MemberName()));

        public static Action<T, V> Of<T, V>(string propertyName) => Of<T, V>(PropertySetter(typeof(T), propertyName));

        private static Action<T, V> Of<T, V>(MethodInfo propertySetMethod)
        {
            DynamicMethod setProperty = new DynamicMethod(
                name: $"{propertySetMethod.Name}_at_{typeof(T)}",
                returnType: typeof(void),
                parameterTypes: new[] { typeof(T), typeof(V) },
                typeof(T).Module);

            setProperty.DefineParameter(1, ParameterAttributes.In, "instance");
            setProperty.DefineParameter(2, ParameterAttributes.In, "value");

            // implement the method body
            var ilGen = setProperty.GetILGenerator(256);
            ilGen.Emit(OpCodes.Ldarg_0); // instance -> stack
            ilGen.Emit(OpCodes.Ldarg_1); // value -> stack
            ilGen.Emit(OpCodes.Callvirt, propertySetMethod);
            ilGen.Emit(OpCodes.Ret);

            // create a delegate from the method
            return (Action<T, V>)setProperty.CreateDelegate(typeof(Action<T, V>));
        }

        private static MethodInfo PropertySetter(Type t, string pn) => t.GetProperty(pn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetSetMethod(true);
    }
}