using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public class ValuePropertyPair
    {
        public ValuePropertyPair(PropertyInfo left, PropertyInfo right)
        {
            this.Left = left;
            this.Right = right;
        }

        public PropertyInfo Left { get; }

        public PropertyInfo Right { get; }
    }
}