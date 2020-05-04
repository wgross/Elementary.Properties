using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyJoinConfiguration
    {
        public void ExcludeLeft(params string[] propertyNames);

        void OverridePair(PropertyInfo leftProperty, PropertyInfo rightProperty);
    }
}