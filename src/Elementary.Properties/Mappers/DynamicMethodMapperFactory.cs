using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Elementary.Properties.Mappers
{
    public class DynamicMethodMapperFactory
    {
        public static Action<S, D> Of<S, D>(IEnumerable<string> propertyNames)
            => Of<S, D>(PropertyMappings(typeof(S), typeof(D), propertyNames));

        private static Action<S, D> Of<S, D>(IEnumerable<(MethodInfo sourceGetter, MethodInfo destinationSetter)> mappings)
        {
            var mapProperty = new DynamicMethod(
                name: $"{typeof(S)}_to_{typeof(D)}",
                returnType: typeof(void),
                parameterTypes: new[] { typeof(S), typeof(D) },
                typeof(S).Module);

            mapProperty.DefineParameter(0, ParameterAttributes.In, "source");
            mapProperty.DefineParameter(1, ParameterAttributes.In, "destination");

            var ilGen = mapProperty.GetILGenerator(256);
            foreach (var mapping in mappings.ToArray())
            {
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, mapping.sourceGetter);
                ilGen.Emit(OpCodes.Callvirt, mapping.destinationSetter);
            }
            ilGen.Emit(OpCodes.Ret);

            return (Action<S, D>)mapProperty.CreateDelegate(typeof(Action<S, D>));
        }

        private static IEnumerable<(MethodInfo sourceGetter, MethodInfo destinationSetter)> PropertyMappings(Type sourceType, Type destinationType, IEnumerable<string> propertyNames) => propertyNames
            .Select(n => (Property(sourceType, n).GetGetMethod(nonPublic: true), Property(destinationType, n).GetSetMethod(nonPublic: true)));

        private static PropertyInfo Property(Type t, string pn) => t.GetProperty(pn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}

//instead fo callvirt?
//  ilGenerator.Emit(OpCodes.Castclass, type);
//  // Call the getter method on the casted instance
//  ilGenerator.EmitCall(OpCodes.Call, nameProperty.GetGetMethod(), null);

// https://codereview.stackexchange.com/questions/126819/runtime-compiler-for-getting-setting-runtime-property-values
;