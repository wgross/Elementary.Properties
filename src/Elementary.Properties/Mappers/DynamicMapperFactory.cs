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
        {
            var propertyPairs = ValuePropertyPair<S,D>.Join(leftProperties: ValueProperty<S>.AllCanRead(), rightProperties: ValueProperty<D>.AllCanWrite());
            configure?.Invoke(propertyPairs);
            return DynamicMappingOperation<S, D>(propertyPairs);
        }

        private static Action<S, D> DynamicMappingOperation<S, D>(IEnumerable<IValuePropertyPair> propertyPairs)
        {
            var mapProperty = new DynamicMethod(
               name: $"{typeof(S)}_to_{typeof(D)}",
               returnType: typeof(void),
               parameterTypes: new[] { typeof(S), typeof(D) },
               typeof(S).Module);

            mapProperty.DefineParameter(0, ParameterAttributes.In, "source");
            mapProperty.DefineParameter(1, ParameterAttributes.In, "destination");

            var ilGen = mapProperty.GetILGenerator(256);
            foreach (var pair in propertyPairs.ToArray())
                MapPropertyPair<S, D>(ilGen, pair);

            ilGen.Emit(OpCodes.Ret);

            return (Action<S, D>)mapProperty.CreateDelegate(typeof(Action<S, D>));
        }

        private static void MapPropertyPair<S, D>(ILGenerator ilGen, IValuePropertyPair pair)
        {
            if (pair is ValuePropertySymmetricPair symPair)
            {
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, pair.Left.GetGetMethod(nonPublic: true));
                ilGen.Emit(OpCodes.Callvirt, symPair.Right.GetSetMethod(nonPublic: true));
            }
            else throw new InvalidOperationException($"pairing type ({pair.GetType().Name}) is unkown. Can't generate code");
        }
    }
}

//instead fo callvirt?
//  ilGenerator.Emit(OpCodes.Castclass, type);
//  // Call the getter method on the casted instance
//  ilGenerator.EmitCall(OpCodes.Call, nameProperty.GetGetMethod(), null);

// https://codereview.stackexchange.com/questions/126819/runtime-compiler-for-getting-setting-runtime-property-values