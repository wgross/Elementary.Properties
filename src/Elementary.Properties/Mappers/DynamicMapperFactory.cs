using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Mappers
{
    public class DynamicMapperFactory
    {
        /// <summary>
        /// Builds a mapping operation which copies the values of all readable value properties of <typeparamref name="S"/> to all writable value properies
        /// of <typeparamref name="d"/> type having the same name and type. The mapping can be adjusted by providing a delegate to <paramref name="configure"/>
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Action<S, D> Of<S, D>(Action<IValuePropertyPairCollectionConfiguration<S, D>>? configure = null)
            where S : class
            where D : class
            => Of<S, D>(ValuePropertyPair<S, D>.MappableCollection(configure));

        private static Action<S, D> Of<S, D>(IEnumerable<IValuePropertyPair> propertyPairs)
        {
            var mapProperty = new DynamicMethod(
               name: $"{typeof(S)}_to_{typeof(D)}",
               returnType: typeof(void),
               parameterTypes: new[] { typeof(S), typeof(D) },
               typeof(S).Module);

            mapProperty.DefineParameter(0, ParameterAttributes.In, "source");
            mapProperty.DefineParameter(1, ParameterAttributes.In, "destination");

            var builder = mapProperty.GetILGenerator(256);

            var scope = DeclareLocal_Scope_From_Arguments<S, D>(builder);

            Map_Argument_Properties(builder, scope, propertyPairs);

            builder.Emit(OpCodes.Ret);

            return (Action<S, D>)mapProperty.CreateDelegate(typeof(Action<S, D>));
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Arguments<S, D>(ILGenerator builder)
        {
            var left = builder.DeclareLocal(typeof(S));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, left);

            var right = builder.DeclareLocal(typeof(D));
            builder.Emit(OpCodes.Ldarg_1);
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static void Map_Argument_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, IEnumerable<IValuePropertyPair> propertyPairs)
        {
            foreach (var pair in propertyPairs.ToArray())
            {
                if (pair is ValuePropertyPairValue valuePair)
                {
                    Map_Value_Property(builder, scope, valuePair);
                }
                else if (pair is ValuePropertyPairNested referenceProperties)
                {
                    Map_Argument_Reference_Properties(builder, scope, referenceProperties);
                }
                else throw new InvalidOperationException($"pairing type ({pair.GetType().Name}) is unkown. Can't generate code");
            }
        }

        private static void Map_Value_Property(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, ValuePropertyPairValue valuePair)
        {
            if (valuePair.LeftPropertyType.Equals(valuePair.RightPropertyType))
            {
                // assign property values
                builder.Emit(OpCodes.Ldloc, scope.right);
                builder.Emit(OpCodes.Ldloc, scope.left);
                builder.Emit(OpCodes.Callvirt, valuePair.LeftGetter());
                builder.Emit(OpCodes.Callvirt, valuePair.RightSetter());
            }
            else if (valuePair.RightPropertyIsNullable)
            {
                // assign nullable of left type to the ride side
                builder.Emit(OpCodes.Ldloc, scope.right);
                builder.Emit(OpCodes.Ldloc, scope.left);
                builder.Emit(OpCodes.Callvirt, valuePair.LeftGetter());
                builder.Emit(OpCodes.Newobj, typeof(Nullable<>).MakeGenericType(valuePair.LeftPropertyType).GetConstructor(new[] { valuePair.LeftPropertyType }));
                builder.Emit(OpCodes.Callvirt, valuePair.RightSetter());
            }
        }

        private static void Map_Argument_Reference_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyPairNested propertyPair)
        {
            var skipMapping = builder.DefineLabel();
            var scope = DeclareLocal_Scope_From_Reference_Properties(builder, parentScope, skipMapping, propertyPair);

            Map_Nested_Properties(builder, scope, propertyPair.NestedPropertyPairs);

            builder.MarkLabel(skipMapping);
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Reference_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, Label skipMapping, ValuePropertyPairNested pair)
        {
            // left.<property> == null ?
            var leftIsNotNull = builder.DefineLabel();
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, pair.LeftGetter());
            builder.Emit(OpCodes.Brtrue, leftIsNotNull);

            // left.<property> == null: set right to null
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Ldnull);
            builder.Emit(OpCodes.Callvirt, pair.RightSetter());
            builder.Emit(OpCodes.Br, skipMapping);

            // left.<property> != null
            builder.MarkLabel(leftIsNotNull);

            // right.<property> == null ?
            var leftAndRightArentNull = builder.DefineLabel();
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, pair.RightGetter());
            builder.Emit(OpCodes.Brtrue, leftAndRightArentNull);

            // right.<property> == null: create new instance of R
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Newobj, pair.RightCtor());
            builder.Emit(OpCodes.Callvirt, pair.RightSetter());

            // left.<property> != null && right.<property> != null
            builder.MarkLabel(leftAndRightArentNull);

            // var left = <property value>
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, pair.LeftGetter());
            var left = builder.DeclareLocal(pair.LeftPropertyType);
            builder.Emit(OpCodes.Stloc, left);

            // var right = <property value>
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, pair.RightGetter());
            var right = builder.DeclareLocal(pair.RightPropertyType);
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static void Map_Nested_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, IEnumerable<IValuePropertyPair> propertyPairs)
        {
            foreach (var pair in propertyPairs.ToArray())
            {
                if (pair is ValuePropertyPairValue valuePair)
                {
                    Map_Value_Property(builder, scope, valuePair);
                }
                else if (pair is ValuePropertyPairNested referenceProperties)
                {
                    Map_Nested_Reference_Properties(builder, scope, referenceProperties);
                }
                else throw new InvalidOperationException($"pairing type ({pair.GetType().Name}) is unkown. Can't generate code");
            }
        }

        private static void Map_Nested_Reference_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyPairNested propertyPair)
        {
            var skipMapping = builder.DefineLabel();
            var scope = DeclareLocal_Scope_From_Reference_Properties(builder, parentScope, skipMapping, propertyPair);

            Map_Nested_Properties(builder, scope, propertyPair.NestedPropertyPairs);

            builder.MarkLabel(skipMapping);
        }
    }
}

//instead fo callvirt?
//  ilGenerator.Emit(OpCodes.Castclass, type);
//  // Call the getter method on the casted instance
//  ilGenerator.EmitCall(OpCodes.Call, nameProperty.GetGetMethod(), null);

// https://codereview.stackexchange.com/questions/126819/runtime-compiler-for-getting-setting-runtime-property-values