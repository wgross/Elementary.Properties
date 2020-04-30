using Elementary.Properties.Getters;
using Elementary.Properties.Setters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elementary.Properties.PropertyBags
{
    /// <summary>
    /// Factory for <see cref="PropertyBag{T}"/>
    /// </summary>
    public sealed class PropertyBag
    {
        /// <summary>
        /// Creates a new instance of a property bag inferring the type from the given parameter
        /// The bag is bound to the <paramref name="instance"/> and uses refelection to read or write the
        /// properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static ReflectedPropertyBag<T> Create<T>(T instance) where T : class
        {
            var bag = new ReflectedPropertyBag<T>(instance);
            var properties = Properties<T>().ToArray();
            bag.Init(getters: ReflectionGetterFactory.Of<T>(properties), setters: ReflectionSetterFactory.Of<T>(properties));
            return bag;
        }

        /// <summary>
        /// Creates a new instance of a property bag inferring the type from the given parameter
        /// The bag isn't bound to an instance of <typeparamref name="T"/> and be reused.
        /// It access the properties efficiently using <see cref="DynamicMethod"/> created with <see cref="System.Reflection.Emit"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DynamicPropertyBag<T> Create<T>() where T : class
        {
            var bag = new DynamicPropertyBag<T>();
            var properties = Properties<T>().ToArray();
            bag.Init(DynamicMethodGetterFactory.Of<T>(properties), DynamicMethodSetterFactory.Of<T>(properties));
            return bag;
        }

        private static IEnumerable<PropertyInfo> Properties<T>() => typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }
}