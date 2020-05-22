using System;
using System.Linq.Expressions;

namespace Elementary.Properties.Selectors
{
    public interface IValuePropertyPairCollectionConfiguration
    {
        public void ExcludeLeft(params string[] propertyNames);
    }

    public interface IValuePropertyPairCollectionConfiguration<L, R> : IValuePropertyPairCollectionConfiguration
    {
        /// <summary>
        /// Defines a custom pairing which is not included in teh collection by default.
        /// This might be cause the names ofthe left and rignt property differ or because the type differ.
        /// </summary>
        /// <param name="leftPropertyName"></param>
        /// <param name="rightPropertyName"></param>
        void IncludePair(string leftPropertyName, string rightPropertyName);

        /// <summary>
        /// Includes the value properties of the nested class of the are matching the  rights sides
        /// nested proertises
        /// </summary>
        /// <param name="propertyAccess">unchained access of a readable property</param>
        /// <param name="configure">allows to confgure the merged collection of the nested properties</param>
        /// <returns></returns>
        void IncludeNested(Expression<Func<L, object?>> propertyAccess, Action<IValuePropertyPairCollectionConfiguration<L, R>>? configure = null);
    }
}