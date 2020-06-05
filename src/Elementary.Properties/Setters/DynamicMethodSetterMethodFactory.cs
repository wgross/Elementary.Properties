using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Setters
{
    /// <summary>
    /// Provides a delegate having a body if "{instance}.{property} = {value}" for a selected property
    /// </summary>
    public class DynamicMethodSetterFactory
    {
        #region Set property value with exact type

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

        #endregion Set property value with exact type

        #region Set property value boxed

        public static Action<T, object?> Of<T>(Expression<Func<T, object?>> propertyAccessExpression)
            where T : class
            => Of<T>(ValueProperty<T>.Info(propertyAccessExpression));

        public static Action<T, object?> Of<T>(string propertyName)
            where T : class
            => Of<T>(ValueProperty<T>.Info(propertyName));

        internal static Action<T, object?> Of<T>(PropertyInfo property)
        {
            if (!property.CanWrite)
                throw new InvalidOperationException($"property(name='{property.Name}') can't write");

            return Of<T>(property.GetSetMethod(true));
        }

        private static Action<T, object?> Of<T>(MethodInfo propertySetMethod)
        {
            DynamicMethod setProperty = new DynamicMethod(
                name: $"{propertySetMethod.Name}_at_{typeof(T)}_obj",
                returnType: typeof(void),
                parameterTypes: new[] { typeof(T), typeof(object) },
                typeof(T).Module);

            setProperty.DefineParameter(1, ParameterAttributes.In, "instance");
            setProperty.DefineParameter(2, ParameterAttributes.In, "value");

            var ilGen = setProperty.GetILGenerator(256);
            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Ldarg_1);
            ilGen.Emit(OpCodes.Unbox_Any, propertySetMethod.GetParameters().Single().ParameterType);
            ilGen.Emit(OpCodes.Callvirt, propertySetMethod);
            ilGen.Emit(OpCodes.Ret);

            return (Action<T, object?>)setProperty.CreateDelegate(typeof(Action<T, object?>));
        }

        #endregion Set property value boxed

        internal static IEnumerable<(string name, Action<T, object?> setter)> Of<T>(IEnumerable<PropertyInfo> properties)
        {
            return properties
                .Where(p => p.CanWrite)
                .Select(p => (p.Name, Of<T>(p.GetSetMethod(true))));
        }

        private static MethodInfo PropertySetter(Type t, string pn) => PropertyInfo(t, pn).GetSetMethod(true);

        private static PropertyInfo PropertyInfo(Type t, string pn) => t.GetProperty(pn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}