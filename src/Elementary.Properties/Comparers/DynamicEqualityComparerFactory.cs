using Elementary.Properties.Assertions;
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
        public static IEqualityComparer<T> Of<T>(Action<IValuePropertyPairConfiguration>? configure = null)
        {
            var properties = ValueProperty<T>.AllCanRead();
            var propertyPairs = ValueProperty<T>.Join<T>(leftProperties: properties, rightProperties: properties);
            configure?.Invoke(propertyPairs);

            return new DynamicEqualityComparer<T>(
                equals: DynamicAssertEqualityFactory.AssertEqualiyOperation<T, T>(propertyPairs),
                getHashCode: GetHashCodeOperation<T>(propertyPairs.Select(pp => pp.Left)));
        }

        private static Func<T, int> GetHashCodeOperation<T>(IEnumerable<PropertyInfo> properties)
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

            var hashCode = builder.DeclareLocal(typeof(HashCode));

            builder.Emit(OpCodes.Ldloca_S, hashCode);
            builder.Emit(OpCodes.Initobj, typeof(HashCode));

            foreach (var getter in properties)
            {
                HashProperty(builder, addHashCode, hashCode, getter);
            }
            builder.Emit(OpCodes.Ldloca_S, hashCode);
            builder.Emit(OpCodes.Call, typeof(HashCode).GetMethod(nameof(HashCode.ToHashCode)));
            builder.Emit(OpCodes.Ret);

            return (Func<T, int>)getHashCodeMethod.CreateDelegate(typeof(Func<T, int>));
        }

        private static void HashProperty(ILGenerator builder, MethodInfo addHashCode, LocalBuilder hashCode, PropertyInfo getter)
        {
            builder.Emit(OpCodes.Ldloca_S, hashCode);
            // push property value
            builder.Emit(OpCodes.Ldarg_0);
            builder.Emit(OpCodes.Callvirt, getter.GetGetMethod(nonPublic: true));
            // call HashCode.Add on property value
            builder.Emit(OpCodes.Call, addHashCode.MakeGenericMethod(getter.PropertyType));
        }
    }
}