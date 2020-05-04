using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static Elementary.Properties.Selectors.ValueProperties;

namespace Elementary.Properties.Assertions
{
    public class DynamicAssertEqualityFactory
    {
        /// <summary>
        /// Provides a equality comparision call backe based on all value properties which can be read.
        /// By default alle properties are matched which are avaliable on both sides with the same name and the same type.
        /// Teh selection can be adjusted using by providing a callback to <paramref name="configure"/>.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Func<T1, T2, bool> Of<T1, T2>(Action<IValuePropertyJoinConfiguration>? configure = null)
        {
            var propertyPairs = Join(leftProperties: AllCanRead<T1>(), rightProperties: AllCanRead<T2>());
            configure?.Invoke(propertyPairs);
            return AssertEqualiyOperation<T1, T2>(propertyPairs);
        }

        internal static Func<T1, T2, bool> AssertEqualiyOperation<T1, T2>(IEnumerable<ValuePropertyPair> propertyPairs)
        {
            var equalsMethod = new DynamicMethod(
                name: $"Equals_{nameof(T1)}_{nameof(T2)}",
                returnType: typeof(bool),
                parameterTypes: new[] { typeof(T1), typeof(T2) },
                typeof(T1).Module);

            var builder = equalsMethod.GetILGenerator();

            foreach (var pair in propertyPairs)
            {
                // get thet default property of the property types default comparer
                var equalityComparerDefault = EqualityComparerDefault(pair.Left.PropertyType);

                var equalityComparerDefaultEquals = equalityComparerDefault
                    .PropertyType
                    .GetMethods()
                    .Where(m => m.DeclaringType == typeof(EqualityComparer<>).MakeGenericType(pair.Left.PropertyType))
                    .Single(m => m.Name == nameof(object.Equals)); // there can be only one.

                var endOfCondition = builder.DefineLabel();

                builder.Emit(OpCodes.Call, equalityComparerDefault.GetGetMethod());

                // get property values
                builder.Emit(OpCodes.Ldarg_0);
                builder.Emit(OpCodes.Callvirt, pair.Left.GetGetMethod(true));
                builder.Emit(OpCodes.Ldarg_1);
                builder.Emit(OpCodes.Callvirt, pair.Right.GetGetMethod(true));
                // compare
                builder.Emit(OpCodes.Callvirt, equalityComparerDefaultEquals);
                builder.Emit(OpCodes.Brtrue_S, endOfCondition);
                // return false
                builder.Emit(OpCodes.Ldc_I4_0);
                builder.Emit(OpCodes.Ret);
                // jump target
                builder.MarkLabel(endOfCondition);
            }

            // return true
            builder.Emit(OpCodes.Ldc_I4_1);
            builder.Emit(OpCodes.Ret);

            return (Func<T1, T2, bool>)equalsMethod.CreateDelegate(typeof(Func<T1, T2, bool>));
        }

        private static PropertyInfo EqualityComparerDefault(Type type)
        {
            return typeof(EqualityComparer<>).MakeGenericType(type).GetProperty("Default");
        }
    }
}