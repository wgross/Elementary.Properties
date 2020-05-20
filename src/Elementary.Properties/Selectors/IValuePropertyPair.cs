using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyPair
    {
        /// <summary>
        /// Retrieves the left propperties info
        /// </summary>
        PropertyInfo Left { get; }
    }
}