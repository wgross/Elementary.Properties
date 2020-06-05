using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    internal class ValuePropertyPairValue : IValuePropertyPair
    {
        internal ValuePropertyPairValue(PropertyInfo left, PropertyInfo right)
        {
            this.Left = left;
            this.Right = right;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Left { get; }

        internal Type LeftPropertyType => this.Left.PropertyType;

        internal MethodInfo LeftGetter() => this.Left.GetGetMethod(nonPublic: true);

        internal PropertyInfo Right { get; }

        internal Type RightPropertyType => this.Right.PropertyType;

        internal bool RightPropertyIsNullable
        {
            get
            {
                if (this.RightPropertyType.IsGenericType)
                    if (this.RightPropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        if (Nullable.GetUnderlyingType(this.RightPropertyType).Equals(this.LeftPropertyType))
                            return true;
                return false;
            }
        }

        internal MethodInfo RightGetter() => this.Right.GetGetMethod(nonPublic: true);

        internal MethodInfo RightSetter() => this.Right.GetSetMethod(nonPublic: true);
    }
}