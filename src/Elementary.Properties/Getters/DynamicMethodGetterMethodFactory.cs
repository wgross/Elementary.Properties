using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Getters
{
    /// <summary>
    /// Provides a delegate having a body if "return {instance}.{property}" for a selected property
    /// </summary>
    public class DynamicMethodGetterFactory
    {
        #region Get property value with exact type

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

            var ilGen = getProperty.GetILGenerator(256);

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Callvirt, propertyGetMethod);
            ilGen.Emit(OpCodes.Ret);

            return (Func<T, V>)getProperty.CreateDelegate(typeof(Func<T, V>));
        }

        #endregion Get property value with exact type

        #region Get property value boxed

        public static Func<T, object?> Of<T>(Expression<Func<T, object?>> propertyAccessExpression)
            => Of<T>(ValueProperty<T>.Info(propertyAccessExpression));

        public static Func<T, object?> Of<T>(string propertyName)
            => Of<T>(ValueProperty<T>.Info(propertyName));

        internal static Func<T, object?> Of<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
                throw new InvalidOperationException($"property(name='{propertyInfo.Name}') can't read");

            return Of<T>(propertyInfo.GetGetMethod(true));
        }

        private static Func<T, object?> Of<T>(MethodInfo propertyGetMethod)
        {
            DynamicMethod getProperty = new DynamicMethod(
                name: $"{propertyGetMethod.Name}_from_{typeof(T)}_obj",
                returnType: typeof(object),
                parameterTypes: new[] { typeof(T) },
                typeof(T).Module);

            getProperty.DefineParameter(1, ParameterAttributes.In, "instance");

            var ilGen = getProperty.GetILGenerator(256);

            ilGen.Emit(OpCodes.Ldarg_0);
            ilGen.Emit(OpCodes.Callvirt, propertyGetMethod);
            ilGen.Emit(OpCodes.Box, propertyGetMethod.ReturnType);
            ilGen.Emit(OpCodes.Ret);

            return (Func<T, object?>)getProperty.CreateDelegate(typeof(Func<T, object?>));
        }

        #endregion Get property value boxed

        internal static IEnumerable<(string name, Func<T, object?> getter)> Of<T>(IEnumerable<PropertyInfo> propertyInfos)
        {
            return propertyInfos
                .Where(pi => pi.CanRead)
                .Select(pi => (pi.Name, Of<T>(pi.GetGetMethod(true))));
        }

        private static PropertyInfo PropertyInfo(Type t, string pn) => t.GetProperty(pn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static MethodInfo PropertyGetter(Type t, string pn) => PropertyInfo(t, pn).GetGetMethod(true);
    }
}