using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertiesCollectionConfig
    {
        void Exclude(params string[] propertyNames);

        void Include(PropertyInfo property);
    }
}