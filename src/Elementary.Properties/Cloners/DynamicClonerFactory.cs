using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Cloners
{
    public class DynamicClonerFactory
    {
        /// <summary>
        /// Creates a clone operation which by default creatze a shallow clone of the class instance.
        /// By including Reference properies in <typeparamref name="T"/> or under <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Func<T, T> Of<T>(Action<IValuePropertyCollectionConfiguration<T>>? configure = null)
            where T : class
            => CloneOperation<T>(ValueProperty<T>.AllCanReadAndWrite(configure));

        private static Func<T, T> CloneOperation<T>(IEnumerable<IValuePropertyCollectionItem> properties)
        {
            var clone = new DynamicMethod(
              name: $"{typeof(T)}_clone",
              returnType: typeof(T),
              parameterTypes: new[] { typeof(T) },
              typeof(T).Module);

            clone.DefineParameter(0, ParameterAttributes.In, "instance");
            var builder = clone.GetILGenerator(256);

            Clone_Instance<T>(builder, properties);

            return (Func<T, T>)clone.CreateDelegate(typeof(Func<T, T>));
        }

        private static void Clone_Instance<T>(ILGenerator builder, IEnumerable<IValuePropertyCollectionItem> properties)
        {
            var localScope = DeclareLocal_Scope_From_Argument_And_Clone<T>(builder);

            foreach (var property in properties)
            {
                if (property is ValuePropertyScalar valueProperty)
                {
                    Map_Value_Property(builder, localScope, valueProperty);
                }
                else if (property is ValuePropertyNested referenceProperty)
                {
                    Map_Nested_Properties(builder, localScope, referenceProperty);
                }
            }

            builder.Emit(OpCodes.Ldloc, localScope.right);
            builder.Emit(OpCodes.Ret);
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Argument_And_Clone<T>(ILGenerator builder)
        {
            var left = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, left);

            var right = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Newobj, typeof(T).GetConstructor(new Type[0]));
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static void Map_Value_Property(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, ValuePropertyScalar value)
        {
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Callvirt, value.Getter());
            builder.Emit(OpCodes.Callvirt, value.Setter());
        }

        private static void Map_Nested_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyNested referenceProperty)
        {
            var skipMapping = builder.DefineLabel();
            var localScope = DeclareLocal_Scope_From_Reference_Property(builder, parentScope, skipMapping, referenceProperty);

            foreach (var property in referenceProperty.NestedProperties)
            {
                if (property is ValuePropertyScalar valueProperty)
                {
                    Map_Value_Property(builder, localScope, valueProperty);
                }
                else if (property is ValuePropertyNested nestedProperty)
                {
                    Map_Nested_Properties(builder, localScope, nestedProperty);
                }
            }

            builder.MarkLabel(skipMapping);
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Reference_Property(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, Label skipMapping, ValuePropertyNested pair)
        {
            // left.<property> == null ?
            var leftIsNotNull = builder.DefineLabel();
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, pair.Getter());
            builder.Emit(OpCodes.Brtrue, leftIsNotNull);

            // left.<property> == null: set right to null
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Ldnull);
            builder.Emit(OpCodes.Callvirt, pair.Setter());
            builder.Emit(OpCodes.Br, skipMapping);

            // left.<property> != null
            builder.MarkLabel(leftIsNotNull);

            // right.<property> == null ?
            var leftAndRightArentNull = builder.DefineLabel();
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, pair.Getter());
            builder.Emit(OpCodes.Brtrue, leftAndRightArentNull);

            // right.<property> == null: create new instance of R
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Newobj, pair.Ctor());
            builder.Emit(OpCodes.Callvirt, pair.Setter());

            // left.<property> != null && right.<property> != null
            builder.MarkLabel(leftAndRightArentNull);

            // var left = <property value>
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, pair.Getter());
            var left = builder.DeclareLocal(pair.PropertyType);
            builder.Emit(OpCodes.Stloc, left);

            // var right = <property value>
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, pair.Getter());
            var right = builder.DeclareLocal(pair.PropertyType);
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }
    }
}