using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public class ValuePropertyCollection : IEnumerable<PropertyInfo>, IValuePropertiesCollectionConfig
    {
        private readonly IEnumerable<PropertyInfo> properties;
        private readonly List<string> excludes = new List<string>();
        private readonly List<PropertyInfo> includes = new List<PropertyInfo>();

        public ValuePropertyCollection(IEnumerable<PropertyInfo> properties)
        {
            this.properties = properties;
        }

        public IEnumerator<PropertyInfo> GetEnumerator()
        {
            var result = this.properties
                .Where(pi => !this.excludes.Contains(pi.Name))
                .Where(pi => !this.includes.Contains(pi))
                .Concat(this.includes)
                .ToList();

            return result.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region IValuePropertiesCollectionConfig

        void IValuePropertiesCollectionConfig.Exclude(params string[] propertyNames) => this.excludes.AddRange(propertyNames);

        void IValuePropertiesCollectionConfig.Include(PropertyInfo properties)
        {
            this.includes.Add(properties);
        }

        #endregion IValuePropertiesCollectionConfig
    }
}