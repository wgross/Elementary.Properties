using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyCollectionItem
    {
        /// <summary>
        /// The <see cref="PropertyInfo"/> which the value property collection item refers to.
        /// </summary>
        PropertyInfo Info { get; }

        /// <summary>
        /// Returns the name of the represented property
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// Returns the Get methods of the representet property. Ge methood may be non public.
        /// </summary>
        /// <returns></returns>
        MethodInfo Getter();

        /// <summary>
        /// Returns the <see cref="Type"/> of the represented property
        /// </summary>
        Type PropertyType { get; }
    }
}