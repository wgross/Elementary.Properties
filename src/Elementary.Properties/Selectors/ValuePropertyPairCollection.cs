using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// A value propery pair collection is an inpput to all factories producing operations on property pairs
    /// like comparision assignment.
    /// It allows to manually exclude or override mappings.
    /// </summary>
    public class ValuePropertyPairCollection : IEnumerable<ValuePropertyPair>, IValuePropertyJoinConfiguration
    {
        private readonly IEnumerable<ValuePropertyPair> propertyPairs;
        private readonly List<string> exclusions = new List<string>();
        private readonly List<ValuePropertyPair> overrides = new List<ValuePropertyPair>();

        internal ValuePropertyPairCollection(IEnumerable<ValuePropertyPair> propertyPairs) => this.propertyPairs = propertyPairs;

        public IEnumerator<ValuePropertyPair> GetEnumerator()
        {
            var resultPairs = this.propertyPairs
                .Where(p => !this.exclusions.Contains(p.Left.Name))
                .Where(pp => !this.overrides.Any(p => p.Left.Equals(pp.Left)) && !this.overrides.Any(p => p.Right.Equals(pp.Right)))
                .Concat(this.overrides);

            return resultPairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        #region IValuePropertyJoinConfiguration

        /// <summary>
        /// Exclude property pairs from the collection based on the name of a left side property.
        /// </summary>
        /// <param name="propertyNames"></param>
        public void ExcludeLeft(params string[] propertyNames) => this.exclusions.AddRange(propertyNames);

        /// <summary>
        /// Adds a manually configured paring of two properties having. In general these properties have different names
        /// but still must have indentical types.
        /// This excludes all other mappings which the given left side property or right side property participate
        /// </summary>
        /// <param name="leftProperty"></param>
        /// <param name="rightProperty"></param>
        public void OverridePair(PropertyInfo leftProperty, PropertyInfo rightProperty) => this.overrides.Add(new ValuePropertyPair(leftProperty, rightProperty));

        #endregion IValuePropertyJoinConfiguration
    }
}