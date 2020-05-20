using Elementary.Properties.Getters;
using Elementary.Properties.Selectors;
using Elementary.Properties.Setters;
using System;
using System.Linq;

namespace Elementary.Properties.PropertyBags
{
    /// <summary>
    /// Factory for <see cref="DynamicPropertyBag{T}"/> and <see cref="ReflectedPropertyBag{T}"/>
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
        /// <param name="configure">declare optional includes or excludes</param>
        /// <returns></returns>
        public static ReflectedPropertyBag<T> Of<T>(T instance, Action<IValuePropertyCollectionConfig<T>>? configure = null) where T : class
        {
            var bag = new ReflectedPropertyBag<T>(instance);
            bag.Init(
                ValueProperty<T>.AllCanReadAndWrite(configure)
                    .Select(pi => (
                        name: pi.Property.Name,
                        getter: ReflectionGetterFactory.Of<T>(pi.Property).getter,
                        setter: ReflectionSetterFactory.Of<T>(pi.Property).setter)));

            return bag;
        }

        /// <summary>
        /// Creates a new instance of a property bag inferring the type from the given parameter
        /// The bag isn't bound to an instance of <typeparamref name="T"/> and be reused.
        /// It access the properties efficiently using <see cref="System.Reflection.Emit.DynamicMethod"/> created with <see cref="System.Reflection.Emit"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DynamicPropertyBag<T> Of<T>(Action<IValuePropertyCollectionConfig<T>>? configure = null) where T : class
        {
            var bag = new DynamicPropertyBag<T>();
            var properties = ValueProperty<T>.All().ToArray();
            bag.Init(
               ValueProperty<T>.AllCanReadAndWrite(configure)
                   .Select(pi => (
                       name: pi.Property.Name,
                       getter: DynamicMethodGetterFactory.Of<T>(pi.Property),
                       setter: DynamicMethodSetterFactory.Of<T>(pi.Property))));
            return bag;
        }
    }
}