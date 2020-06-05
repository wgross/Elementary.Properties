using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Assertions
{
    public class DynamicAssertEqualityFactory
    {
        /// <summary>
        /// Provides a equality comparision call backe based on all value properties which can be read.
        /// By default alle properties are matched which are avaliable on both sides with the same name and the same type.
        /// Teh selection can be adjusted using by providing a callback to <paramref name="configure"/>.
        /// </summary>
        /// <typeparam name="L"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static Func<L, R, bool> Of<L, R>(Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null)
        {
            var propertyPairs = ValuePropertyPair<L, R>.ComparableCollection(configure);
            return AssertEqualiyOperation<L, R>(propertyPairs);
        }

        internal static Func<L, R, bool> AssertEqualiyOperation<L, R>(IEnumerable<IValuePropertyPair> propertyPairs)
        {
            var equalsMethod = new DynamicMethod(
                name: $"Equals_{nameof(L)}_{nameof(R)}",
                returnType: typeof(bool),
                parameterTypes: new[] { typeof(L), typeof(R) },
                typeof(L).Module);

            var builder = equalsMethod.GetILGenerator();

            var scope = DeclareLocal_Scope_From_Arguments<L, R>(builder);

            If_Scope_Is_Null_Both_Sides_Return(builder, scope);

            Compare_Properties(builder, scope, propertyPairs);

            // return true
            Return_True(builder);

            return (Func<L, R, bool>)equalsMethod.CreateDelegate(typeof(Func<L, R, bool>));
        }

        private static void If_Scope_Is_Null_Both_Sides_Return(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope)
        {
            // if(object.ReferenceEquals(left, right))
            //   return true;
            var gotoContinue = builder.DefineLabel();

            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Bne_Un_S, gotoContinue);

            Return_True(builder);

            builder.MarkLabel(gotoContinue);

            // if (left is null || right is null)
            //    return false;
            var gotoReturnFalse = builder.DefineLabel();
            var gotoEnd = builder.DefineLabel();

            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Brfalse_S, gotoReturnFalse);
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Brtrue_S, gotoEnd);

            builder.MarkLabel(gotoReturnFalse);

            Return_False(builder);

            builder.MarkLabel(gotoEnd);
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Arguments<L, R>(ILGenerator builder)
        {
            var left = builder.DeclareLocal(typeof(L));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, left);

            var right = builder.DeclareLocal(typeof(R));
            builder.Emit(OpCodes.Ldarg_1);
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static void Compare_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, IEnumerable<IValuePropertyPair> propertyPairs)
        {
            foreach (var pair in propertyPairs)
            {
                if (pair is ValuePropertyPairValue valuePair)
                {
                    Compare_Value_Properties(builder, scope, valuePair);
                }
                else if (pair is ValuePropertyPairNested nestedPair)
                {
                    Compare_Nested_Properties(builder, scope, nestedPair);
                }
                else throw new InvalidOperationException($"Pairing type {pair.GetType().Name} is unknown");
            }
        }

        private static void Compare_Nested_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyPairNested nestedPair)
        {
            var scope = DeclareLocal_Scope_From_Properties(builder, parentScope, nestedPair);

            var gotoSkipCompare = builder.DefineLabel();

            If_Scope_Is_Null_At_Both_Sides_Skip(builder, scope, gotoSkipCompare);

            Compare_Properties(builder, scope, nestedPair.NestedPropertyPairs);

            builder.MarkLabel(gotoSkipCompare);
        }

        private static void If_Scope_Is_Null_At_Both_Sides_Skip(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, Label gotoEndOfBlock)
        {
            var gotoReturnFalse = builder.DefineLabel();
            var gotoContinue = builder.DefineLabel();

            // if(left == right) skip the whole block
            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Beq_S, gotoEndOfBlock);

            // if(left is null || right is null) return false
            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Brfalse_S, gotoReturnFalse);
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Brtrue_S, gotoContinue);
            builder.MarkLabel(gotoReturnFalse);
            Return_False(builder);

            // continue with comparision
            builder.MarkLabel(gotoContinue);
        }

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyPairNested pair)
        {
            // var left = <property value>
            var left = builder.DeclareLocal(pair.LeftPropertyType);
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, pair.LeftGetter());
            builder.Emit(OpCodes.Stloc, left);

            // var right = <property value>
            var right = builder.DeclareLocal(pair.RightPropertyType);
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, pair.RightGetter());
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static void Compare_Value_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, ValuePropertyPairValue propertyPair)
        {
            var equalityComparer = EqualityComparer(propertyPair.LeftPropertyType);

            // push default comparer to stack
            builder.Emit(OpCodes.Call, equalityComparer.getDefault);

            // get property values
            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Callvirt, propertyPair.LeftGetter());
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Callvirt, propertyPair.RightGetter());

            // compare and jump to end if equal
            var gotoValueAreEqual = builder.DefineLabel();
            builder.Emit(OpCodes.Callvirt, equalityComparer.equals);
            builder.Emit(OpCodes.Brtrue_S, gotoValueAreEqual);

            // Return false if not equal
            Return_False(builder);

            // jump target
            builder.MarkLabel(gotoValueAreEqual);
        }

        private static void Return_False(ILGenerator builder)
        {
            // return false
            builder.Emit(OpCodes.Ldc_I4_0);
            builder.Emit(OpCodes.Ret);
        }

        private static void Return_True(ILGenerator builder)
        {
            // return true
            builder.Emit(OpCodes.Ldc_I4_1);
            builder.Emit(OpCodes.Ret);
        }

        private static (MethodInfo getDefault, MethodInfo equals) EqualityComparer(Type typeToCompare)
        {
            var equalityComparerDefault = EqualityComparerDefault(typeToCompare);

            return (
                equalityComparerDefault.GetGetMethod(),
                EqualityComparerGetter(typeToCompare, equalityComparerDefault)
            );
        }

        private static PropertyInfo EqualityComparerDefault(Type typeToCompare) => typeof(EqualityComparer<>).MakeGenericType(typeToCompare).GetProperty("Default");

        private static MethodInfo EqualityComparerGetter(Type typeToCompare, PropertyInfo equalityComparerDefault)
        {
            return equalityComparerDefault
                .PropertyType
                .GetMethods()
                .Where(m => m.DeclaringType == typeof(EqualityComparer<>).MakeGenericType(typeToCompare))
                .Single(m => m.Name == nameof(object.Equals));
        }
    }
}