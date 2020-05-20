using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyMappingConfiguration<S, D> : IValuePropertyPairConfiguration
    {
        /// <summary>
        /// Instead of using a propery directly at the destinatio side, a setter delegate may be speficied.
        /// This is useful if teh value is actually writtene with a methoid isnread of a destination property setter
        /// or requires conversion or any other special handling. Ir might also be used to specify traversal of
        /// child objects during the mapping
        /// </summary>
        /// <param name="rightProperty"></param>
        /// <param name="setter"></param>
        void OverridePairWithDestinationSetter(PropertyInfo rightProperty, Action<D, object> setter);
    }
}