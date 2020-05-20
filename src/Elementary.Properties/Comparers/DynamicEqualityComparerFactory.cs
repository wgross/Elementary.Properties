using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Comparers
{
    public class DynamicEqualityComparerFactory
    {
        /// <summary>
        ///  Creates an instance of <see cref="DynamicEqualityComparer{T}"/> which compares the configured properties
        ///  if two instances of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IEqualityComparer<T> Of<T>(Action<IValuePropertyCollectionConfig<T>>? configure = null) where T : class
        {
            var properties = ValueProperty<T>.AllCanRead(configure);

            return new DynamicEqualityComparer<T>(equals: GetEqualsOperation<T>(properties), getHashCode: GetHashCodeOperation<T>(properties));
        }

        #region Emit Equals Operation

        private static Func<T, T, bool> GetEqualsOperation<T>(IEnumerable<IValuePropertyCollectionItem> properties)
        {
            var getHashCodeMethod = new DynamicMethod(
                name: $"Equals_{nameof(T)}",
                returnType: typeof(bool),
                parameterTypes: new[] { typeof(T), typeof(T) },
                typeof(T).Module);

            var builder = getHashCodeMethod.GetILGenerator();

            var scope = DeclareLocal_Scope_From_Arguments<T>(builder);

            Is_Scope_Is_Null_Same_Return(builder, scope);

            Compare_Properties(builder, scope, properties);

            Return_True(builder);

            return (Func<T, T, bool>)getHashCodeMethod.CreateDelegate(typeof(Func<T, T, bool>));
        }

        private static void Compare_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, IEnumerable<IValuePropertyCollectionItem> properties)
        {
            foreach (var p in properties)
            {
                if (p is ValuePropertyCollectionValue valueProperty)
                {
                    Compare_Value_Property(builder, scope, valueProperty);
                }
                else if (p is ValuePropertyCollectionReference referenceProperty)
                {
                    Compare_Reference_Property(builder, scope, referenceProperty);
                }
            };
        }

        private static void Compare_Reference_Property(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, ValuePropertyCollectionReference referenceProperty)
        {
            var scope = DeclareLocal_Scope_From_Properties(builder, parentScope, referenceProperty.Info);
            var gotoSkipCompare = builder.DefineLabel();

            If_Scope_Is_Null_At_Both_Sides_Skip(builder, scope, gotoSkipCompare, referenceProperty.Info);

            Compare_Properties(builder, scope, referenceProperty.ValueProperties);

            builder.MarkLabel(gotoSkipCompare);
        }

        private static void Compare_Value_Property(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, ValuePropertyCollectionValue valueProperty)
        {
            // get property values
            builder.Emit(OpCodes.Ldloc, scope.left);
            builder.Emit(OpCodes.Callvirt, valueProperty.Info.GetGetMethod(nonPublic: true));
            builder.Emit(OpCodes.Ldloc, scope.right);
            builder.Emit(OpCodes.Callvirt, valueProperty.Info.GetGetMethod(nonPublic: true));

            // compare and jump to end if equal
            var gotoValuesAreEqual = builder.DefineLabel();
            builder.Emit(OpCodes.Beq_S, gotoValuesAreEqual);

            // return false if not equal
            Return_False(builder);

            builder.MarkLabel(gotoValuesAreEqual);
        }

        private static void If_Scope_Is_Null_At_Both_Sides_Skip(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope, Label gotoEndOfBlock, PropertyInfo property)
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

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Properties(ILGenerator builder, (LocalBuilder left, LocalBuilder right) parentScope, PropertyInfo info)
        {
            // var left = <property value>
            var left = builder.DeclareLocal(info.PropertyType);
            builder.Emit(OpCodes.Ldloc, parentScope.left);
            builder.Emit(OpCodes.Callvirt, info.GetGetMethod(nonPublic: true));
            builder.Emit(OpCodes.Stloc, left);

            // var right = <property value>
            var right = builder.DeclareLocal(info.PropertyType);
            builder.Emit(OpCodes.Ldloc, parentScope.right);
            builder.Emit(OpCodes.Callvirt, info.GetGetMethod(nonPublic: true));
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        private static PropertyInfo EqualityComparerDefault(Type type)
        {
            return typeof(EqualityComparer<>).MakeGenericType(type).GetProperty("Default");
        }

        private static void Is_Scope_Is_Null_Same_Return(ILGenerator builder, (LocalBuilder left, LocalBuilder right) scope)
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

        private static (LocalBuilder left, LocalBuilder right) DeclareLocal_Scope_From_Arguments<T>(ILGenerator builder)
        {
            var left = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, left);

            var right = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Ldarg_1);
            builder.Emit(OpCodes.Stloc, right);

            return (left, right);
        }

        #endregion Emit Equals Operation

        #region Emit Common Code

        private static void Return_True(ILGenerator builder)
        {
            builder.Emit(OpCodes.Ldc_I4_1);
            builder.Emit(OpCodes.Ret);
        }

        private static void Return_False(ILGenerator builder)
        {
            builder.Emit(OpCodes.Ldc_I4_0);
            builder.Emit(OpCodes.Ret);
        }

        #endregion Emit Common Code

        #region Emit GetHashCode Operation

        private static Func<T, int> GetHashCodeOperation<T>(IEnumerable<IValuePropertyCollectionItem> properties)
        {
            var getHashCodeMethod = new DynamicMethod(
               name: $"GetHashCode_{nameof(T)}",
               returnType: typeof(int),
               parameterTypes: new[] { typeof(T) },
               typeof(T).Module);

            var builder = getHashCodeMethod.GetILGenerator();

            var addHashCode = typeof(HashCode)
                .GetMethods()
                .Single(m => m.Name == nameof(HashCode.Add) && m.GetParameters().Length == 1);

            var hashCode = DeclareLocal_HashCode(builder);
            var scope = DeclareLocal_Scope_From_Argument<T>(builder);

            var gotoContinue = builder.DefineLabel();

            If_Scope_Is_Null_Return_0(builder, scope, gotoContinue);

            builder.MarkLabel(gotoContinue);

            Hash_Properties(builder, hashCode, addHashCode, scope, properties);

            Return_HashCode(builder, hashCode);

            return (Func<T, int>)getHashCodeMethod.CreateDelegate(typeof(Func<T, int>));
        }

        private static void Hash_Properties(ILGenerator builder, LocalBuilder hashCode, MethodInfo addHashCode, LocalBuilder scope, IEnumerable<IValuePropertyCollectionItem> properties)
        {
            foreach (var p in properties)
            {
                if (p is ValuePropertyCollectionValue valueProperty)
                {
                    Hash_Value_Property(builder, addHashCode, hashCode, scope, valueProperty.Info);
                }
                else if (p is ValuePropertyCollectionReference referenceProperty)
                {
                    Hash_Reference_Property(builder, addHashCode, hashCode, scope, referenceProperty);
                }
            }
        }

        private static void If_Scope_Is_Null_Return_0(ILGenerator builder, LocalBuilder scope, Label gotoContinue)
        {
            builder.Emit(OpCodes.Ldloc, scope);
            builder.Emit(OpCodes.Brtrue_S, gotoContinue);

            Return_False(builder);
        }

        private static LocalBuilder DeclareLocal_Scope_From_Argument<T>(ILGenerator builder)
        {
            var local = builder.DeclareLocal(typeof(T));
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Stloc, local);
            return local;
        }

        private static void Hash_Reference_Property(ILGenerator builder, MethodInfo addHashCode, LocalBuilder hashCode, LocalBuilder parentScope, ValuePropertyCollectionReference property)
        {
            var scope = DeclareLocal_Scope_From_Property(builder, parentScope, property.Info);
            var gotoSkipHash = builder.DefineLabel();

            If_Scope_Is_Null_Skip(builder, scope, gotoSkipHash);

            Hash_Properties(builder, hashCode, addHashCode, scope, property.ValueProperties);

            builder.MarkLabel(gotoSkipHash);
        }

        private static void If_Scope_Is_Null_Skip(ILGenerator builder, LocalBuilder scope, Label gotoEnd)
        {
            builder.Emit(OpCodes.Ldloc, scope);
            builder.Emit(OpCodes.Brfalse, gotoEnd);
        }

        private static LocalBuilder DeclareLocal_Scope_From_Property(ILGenerator builder, LocalBuilder parentScope, PropertyInfo info)
        {
            var local = builder.DeclareLocal(info.PropertyType);
            builder.Emit(OpCodes.Ldloc_S, parentScope);
            builder.Emit(OpCodes.Callvirt, info.GetGetMethod(nonPublic: true));
            builder.Emit(OpCodes.Stloc, local);
            return local;
        }

        private static LocalBuilder DeclareLocal_HashCode(ILGenerator builder)
        {
            var hashCode = builder.DeclareLocal(typeof(HashCode));

            builder.Emit(OpCodes.Ldloca_S, hashCode);
            builder.Emit(OpCodes.Initobj, typeof(HashCode));
            return hashCode;
        }

        private static void Hash_Value_Property(ILGenerator builder, MethodInfo addHashCode, LocalBuilder hashCode, LocalBuilder scope, PropertyInfo getter)
        {
            builder.Emit(OpCodes.Ldloca_S, hashCode);
            // push property value
            builder.Emit(OpCodes.Ldloc, scope);
            builder.Emit(OpCodes.Callvirt, getter.GetGetMethod(nonPublic: true));
            // call HashCode.Add on property value
            builder.Emit(OpCodes.Call, addHashCode.MakeGenericMethod(getter.PropertyType));
        }

        private static void Return_HashCode(ILGenerator builder, LocalBuilder hashCode)
        {
            builder.Emit(OpCodes.Ldloca_S, hashCode);
            builder.Emit(OpCodes.Call, typeof(HashCode).GetMethod(nameof(HashCode.ToHashCode)));
            builder.Emit(OpCodes.Ret);
        }

        #endregion Emit GetHashCode Operation
    }
}