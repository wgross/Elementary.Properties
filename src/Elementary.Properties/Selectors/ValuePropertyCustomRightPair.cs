using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    internal class ValuePropertyPairWithCustomRightSetter<R> : IValuePropertyPair
    {
        internal ValuePropertyPairWithCustomRightSetter(PropertyInfo left, Action<R, object> setter)
        {
            this.Left = left;
            this.RightSetter = setter;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Left { get; }

        internal Action<R, object> RightSetter { get; }
    }
}