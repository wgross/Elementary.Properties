using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Assertions
{
    /// <summary>
    /// Generates a method which checks all properties having a value != default(property type).
    /// </summary>
    public class DynamicAssertInitializedFactory
    {
        /// <summary>
        /// Creates an operation using <see cref="System.Reflection.Emit"/> which checks if any properties of an instance of
        /// of <typeparamref name="T"/> has any unitialized properties (value == default(T)).
        /// The delegate will return false if any uninitialized property was found.
        /// By default only value properties are checked. Reference properties must be included explicitly in the assertion
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Func<T, bool> Of<T>(Action<IValuePropertyCollectionConfiguration<T>>? configure = null)
        {
            return AssertIntializedOperation<T>(ValueProperty<T>.AllCanRead(configure));
        }

        private static Func<T, bool> AssertIntializedOperation<T>(IEnumerable<IValuePropertyCollectionItem> properties)
        {
            var assertInitialized = new DynamicMethod(
              name: $"{typeof(T)}_assert_initialized",
              returnType: typeof(bool),
              parameterTypes: new[] { typeof(T) },
              typeof(T).Module);

            assertInitialized.DefineParameter(0, ParameterAttributes.In, "instance");
            var builder = assertInitialized.GetILGenerator(256);

            Assert_Initialized_Of_Properties(builder, Scope_To_Method_Argument<T>(builder), properties);
            Return_True(builder);

            return (Func<T, bool>)assertInitialized.CreateDelegate(typeof(Func<T, bool>));
        }

        private static void Assert_Initialized_Of_Properties(ILGenerator builder, LocalBuilder scope, IEnumerable<IValuePropertyCollectionItem> properties)
        {
            foreach (var p in properties)
            {
                Use_Scope_if_Not_Null(builder, scope);

                if (p is ValuePropertyCollectionValue valueProperty)
                {
                    If_Property_Is_Default_Return_False(builder, valueProperty);
                }
                else if (p is ValuePropertyNested referenceProperty)
                {
                    Assert_Initialized_Of_Properties(builder, Scope_To_Property_Value(builder, referenceProperty), referenceProperty.NestedProperties);
                }
            }
        }

        private static LocalBuilder Scope_To_Method_Argument<T>(ILGenerator builder)
        {
            var scope = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, scope);
            return scope;
        }

        private static LocalBuilder Scope_To_Property_Value(ILGenerator builder, ValuePropertyNested property)
        {
            var scope = builder.DeclareLocal(property.PropertyType);
            builder.Emit(OpCodes.Callvirt, property.Getter());
            builder.Emit(OpCodes.Stloc, scope);
            return scope;
        }

        private static void Use_Scope_if_Not_Null(ILGenerator builder, LocalBuilder scope)
        {
            // load the scope
            builder.Emit(OpCodes.Ldloc, scope);

            // check if not null
            var endOfCondition = builder.DefineLabel();
            builder.Emit(OpCodes.Brtrue_S, endOfCondition);
            builder.Emit(OpCodes.Ldc_I4_0);
            builder.Emit(OpCodes.Ret);

            // load the scope again for processing
            builder.MarkLabel(endOfCondition);
            builder.Emit(OpCodes.Ldloc, scope);
        }

        private static void If_Property_Is_Default_Return_False(ILGenerator builder, ValuePropertyCollectionValue property)
        {
            // push the property vaue on the stack
            builder.Emit(OpCodes.Callvirt, property.Getter());

            // if null return false
            var endOfCondition = builder.DefineLabel();
            builder.Emit(OpCodes.Brtrue_S, endOfCondition);
            builder.Emit(OpCodes.Ldc_I4_0);
            builder.Emit(OpCodes.Ret);
            builder.MarkLabel(endOfCondition);
        }

        private static void Return_True(ILGenerator builder)
        {
            builder.Emit(OpCodes.Ldc_I4_1);
            builder.Emit(OpCodes.Ret);
        }
    }
}