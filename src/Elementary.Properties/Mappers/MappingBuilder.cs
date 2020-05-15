using Elementary.Properties.Selectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Test.Mappers
{
    public class MappingBuilder<S, D>
    {
        private const BindingFlags PropertyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly List<PropertyInfo> includedSourceProperties = new List<PropertyInfo>();

        private readonly List<PropertyInfo> excludedSourceProperties = new List<PropertyInfo>();

        #region Specify Properties to map

        /// <summary>
        /// Select properties for mapping using a property access expression
        /// </summary>
        /// <param name="propertyAccess"></param>
        /// <returns></returns>
        public MappingBuilder<S, D> Include(params Expression<Func<S, object>>[] propertyAccess) => this.Include(propertyAccess.Select(pa => PropertyInfo(pa)));

        /// <summary>
        /// Select properties for mapping from the property name
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public MappingBuilder<S, D> Include(params string[] propertyNames) => this.Include(propertyNames.Select(pn => PropertyInfo(typeof(S), pn)));

        /// <summary>
        /// Includes the properties which provide value type or string into the mapping
        /// </summary>
        /// <returns></returns>
        public MappingBuilder<S, D> IncludeValueProperties() => this.Include(ValueProperties.All<S>());

        private MappingBuilder<S, D> Include(IEnumerable<PropertyInfo> properties)
        {
            this.includedSourceProperties.AddRange(properties);
            return this;
        }

        public MappingBuilder<S, D> Exclude(params Expression<Func<S, object>>[] propertyAccess) => this.Exclude(propertyAccess.Select(pa => PropertyInfo(pa)));

        private MappingBuilder<S, D> Exclude(IEnumerable<PropertyInfo> properties)
        {
            this.excludedSourceProperties.AddRange(properties);
            return this;
        }

        #endregion Specify Properties to map

        #region Build Mapping

        public IEnumerable<Mapping> Build()
        {
            static Mapping ToMapping(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
                => new Mapping(sourceProperty.GetGetMethod(true), destinationProperty.GetSetMethod(true));

            return this.includedSourceProperties
                .Except(this.excludedSourceProperties)
                .Select(p => ToMapping(PropertyInfo(typeof(S), p.Name), PropertyInfo(typeof(D), p.Name)));
        }

        #endregion Build Mapping

        #region Get Properties from Source and Destination

        private static PropertyInfo PropertyInfo(Expression<Func<S, object>> propertyAccessExpression)
        {
            var depth = 0;
            Expression exp = propertyAccessExpression;
            do
            {
                switch (exp)
                {
                    case LambdaExpression lambda:
                        exp = lambda.Body;
                        break;

                    case UnaryExpression convert when convert.NodeType == ExpressionType.Convert:
                        exp = convert.Operand;
                        break;

                    case MemberExpression memberAccess when memberAccess.Member.MemberType == MemberTypes.Property:
                        return (PropertyInfo)memberAccess.Member;

                    case MemberExpression memberAccess when memberAccess.Member.MemberType != MemberTypes.Property:
                        throw new ArgumentException($"Expression doesn't access a property but a '{memberAccess.Member.MemberType}' named '{memberAccess.Member.Name}' ");

                    default:
                        throw new InvalidOperationException($"property access contained unexpected expression: {exp.NodeType}");
                }

                depth++;
            }
            while (depth < 3); // lambda -> convert -> member

            throw new InvalidOperationException("property access expression was too deep (>3)");
        }

        private static PropertyInfo PropertyInfo(Type t, string propertyName) => t.GetProperty(propertyName, PropertyBindingFlags);

        #endregion Get Properties from Source and Destination
    }
}