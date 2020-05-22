using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    internal class ValuePropertySymmetricPair : IValuePropertyPair
    {
        internal ValuePropertySymmetricPair(PropertyInfo left, PropertyInfo right)
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

        internal MethodInfo RightGetter() => this.Right.GetGetMethod(nonPublic: true);
    }
}