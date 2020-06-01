using System;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Reference a pair of two reference properties defining a set of nested propertiy pairs through their
    /// value properties.
    /// </summary>
    public class ValuePropertyPairNested : IValuePropertyPair
    {
        internal ValuePropertyPairNested(PropertyInfo leftReference, PropertyInfo rightReference, ValuePropertyPairCollection nestedPropertyPairs)
        {
            this.Left = leftReference;
            this.Right = rightReference;
            this.NestedPairs = nestedPropertyPairs;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public PropertyInfo Left { get; }

        /// <summary>
        /// All properties pairs nested unter the left and right reference properties
        /// </summary>
        public ValuePropertyPairCollection NestedPairs { get; }

        internal MethodInfo LeftGetter() => this.Left.GetGetMethod(nonPublic: true);

        internal Type LeftPropertyType => this.Left.PropertyType;

        internal string LeftPropertyName => this.Left.Name;

        internal PropertyInfo Right { get; }

        internal Type RightPropertyType => this.Right.PropertyType;

        internal MethodInfo RightGetter() => this.Right.GetGetMethod(nonPublic: true);

        internal MethodInfo RightSetter() => this.Right.GetSetMethod(nonPublic: true);

        internal ConstructorInfo RightCtor() => this.Right.PropertyType.GetConstructor(new Type[0]) ?? throw new InvalidOperationException($"Right property type(name='{RightPropertyType.Name}' requires default ctor");
    }
}