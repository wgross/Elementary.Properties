using System;
using System.Linq.Expressions;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyPairCollectionConfiguration<L>
    {
        /// <summary>
        /// Excludes a property pair from the selection if the left property matches the given propery access expression
        /// </summary>
        /// <param name="propertyAccess"></param>
        public void ExcludeLeftValue(Expression<Func<L, object?>> propertyAccess);
    }

    public interface IValuePropertyPairCollectionConfiguration<L, R> : IValuePropertyPairCollectionConfiguration<L>
    {
        /// <summary>
        /// Includes the value properties of the nested class of the are matching the  rights sides
        /// nested proertises
        /// </summary>
        /// <param name="propertyAccess">unchained access of a readable property</param>
        /// <param name="configure">allows to confgure the merged collection of the nested properties</param>
        /// <returns></returns>
        void IncludeNested(Expression<Func<L, object?>> propertyAccess);
    }
}