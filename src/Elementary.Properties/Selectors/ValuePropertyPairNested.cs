using System;
using System.Collections.Generic;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public class ValuePropertyPairNested : IValuePropertyPair
    {
        internal ValuePropertyPairNested(PropertyInfo left, PropertyInfo right, IEnumerable<IValuePropertyPair> nestedPropertyPairs)
        {
            this.Left = left;
            this.Right = right;
            this.NestedPropertyPairs = nestedPropertyPairs;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Left { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        internal MethodInfo LeftGetter() => this.Left.GetGetMethod(nonPublic: true);

        internal Type LeftPropertyType => this.Left.PropertyType;

        internal PropertyInfo Right { get; }

        internal Type RightPropertyType => this.Right.PropertyType;

        internal MethodInfo RightGetter() => this.Right.GetGetMethod(nonPublic: true);

        public IEnumerable<IValuePropertyPair> NestedPropertyPairs { get; }
    }
}