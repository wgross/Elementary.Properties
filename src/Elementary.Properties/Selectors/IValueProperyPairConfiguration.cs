using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyPairConfiguration
    {
        public void ExcludeLeft(params string[] propertyNames);

        void OverridePair(PropertyInfo leftProperty, PropertyInfo rightProperty);
    }
}