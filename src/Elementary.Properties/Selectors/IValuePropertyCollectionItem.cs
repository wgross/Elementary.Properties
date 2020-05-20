using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyCollectionItem
    {
        /// <summary>
        /// The <see cref="PropertyInfo"/> which the value property collection item refers to.
        /// </summary>
        public PropertyInfo Info { get; }
    }
}