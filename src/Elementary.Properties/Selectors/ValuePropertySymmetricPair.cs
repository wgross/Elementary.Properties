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

        internal PropertyInfo Right { get; }
    }
}