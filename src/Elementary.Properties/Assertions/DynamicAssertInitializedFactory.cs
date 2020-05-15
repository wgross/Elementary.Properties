using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Assertions
{
    public class DynamicAssertInitializedFactory
    {
        /// <summary>
        /// Creates an operations dynamically which checks if all properties of an instance of
        /// of <typeparamref name="T"/> has any unitialized properties (value == default(T)).
        /// The delegate will return false if any uninitialized property was found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Func<T, bool> Of<T>(Action<IValuePropertiesCollectionConfig> configure = null)
        {
            return AssertIntializedOperation<T>(ValueProperties.AllCanRead<T>());
        }

        private static Func<T, bool> AssertIntializedOperation<T>(IEnumerable<PropertyInfo> properties)
        {
            var assertInitialized = new DynamicMethod(
              name: $"{typeof(T)}_assert_initialized",
              returnType: typeof(bool),
              parameterTypes: new[] { typeof(T) },
              typeof(T).Module);

            assertInitialized.DefineParameter(0, ParameterAttributes.In, "instance");
            var builder = assertInitialized.GetILGenerator(256);

            foreach (var pair in properties.ToArray())
                AssertIntializedProperty(builder, pair);

            builder.Emit(OpCodes.Ldc_I4_1);
            builder.Emit(OpCodes.Ret);

            return (Func<T, bool>)assertInitialized.CreateDelegate(typeof(Func<T, bool>));
        }

        private static void AssertIntializedProperty(ILGenerator builder, PropertyInfo property)
        {
            var endOfCondition = builder.DefineLabel();
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Callvirt, property.GetGetMethod(nonPublic: true));
            builder.Emit(OpCodes.Brtrue_S, endOfCondition);
            builder.Emit(OpCodes.Ldc_I4_0);
            builder.Emit(OpCodes.Ret);
            builder.MarkLabel(endOfCondition);
        }
    }
}