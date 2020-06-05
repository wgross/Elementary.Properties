using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    /// <summary>
    /// Helper to fetch attributes of a C# property from the given type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Property<T>
    {
        /// <summary>
        /// Select a property info of the property specified by the given member access expression <paramref name="memberAccess"/> from type
        /// <typeparamref name="T"/>. If the property access is chained an <see cref="InvalidOperationException"/> is thrown
        /// </summary>
        public static PropertyInfo Info(Expression<Func<T, object?>> memberAccess) => InfoPath(memberAccess).Single();

        /// <summary>
        /// Returns a property chain from <paramref name="memberAccess"/> starting with the first property and ending with teh last propery in the chain.
        /// </summary>
        /// <param name="memberAccess"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> InfoPath(Expression<Func<T, object?>> memberAccess)
        {
            if (memberAccess is null)
                throw new ArgumentNullException(nameof(memberAccess));

            Expression currentExpression = memberAccess;
            bool reachedPropertyAccess = false;
            do
            {
                // parse the top part of the expepression ujntil member access is reached
                // lambda -> convert -> member access

                switch (currentExpression)
                {
                    case LambdaExpression lambda:
                        currentExpression = lambda.Body;
                        break;

                    case UnaryExpression convert when (convert.NodeType == ExpressionType.Convert):
                        currentExpression = convert.Operand;
                        break;

                    case MemberExpression propertyAccess when propertyAccess.Member.MemberType == MemberTypes.Property:
                        // first property access expression reached -> brek the look
                        reachedPropertyAccess = true;
                        break;

                    default:
                        throw new InvalidOperationException($"property access contained unexpected expression: {currentExpression.NodeType}");
                }
            }
            while (!reachedPropertyAccess);

            var pathToRoot = new Stack<PropertyInfo>();
            do
            {
                switch (currentExpression)
                {
                    case MemberExpression propertyAccess when propertyAccess.Member.MemberType == MemberTypes.Property:
                        pathToRoot.Push((PropertyInfo)propertyAccess.Member);
                        currentExpression = propertyAccess.Expression;
                        break;

                    default:
                        break;
                }
            }
            while (currentExpression is MemberExpression);

            return pathToRoot.OfType<PropertyInfo>();
        }

        /// <summary>
        /// Returns a property chain from the given property names in <paramref name="propertyNames"/> or throws
        /// if a path can't be built
        /// </summary>
        /// <param name="propertyNames"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> InfoPath(params string[] propertyNames)
        {
            var type = typeof(T);
            foreach (var name in propertyNames)
            {
                var currentProperty = Property.Info(type, name);
                yield return currentProperty;
                type = currentProperty.PropertyType;
            }
        }

        /// <summary>
        /// Select a property info of the property specified by the given name <paramref name="propertyName"/> from type
        /// <typeparamref name="T"/>
        /// </summary>
        public static PropertyInfo Info(string name) => Property.Info(typeof(T), name);

        /// <summary>
        /// Returns all properties iof <typeparamref name="T"/> which are accepted by all given <paramref name="filters"/>.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> Infos(params Func<PropertyInfo, bool>[] filters) => Property.Infos(typeof(T), filters);
    }

    internal class Property
    {
        internal const BindingFlags CommonBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        internal static PropertyInfo Info(Type type, string name) => type.GetProperty(name, CommonBindingFlags) ?? throw new InvalidOperationException($"Property(name='{name}') wasn't found in type(name='{type.Name}')");

        internal static IEnumerable<PropertyInfo> Infos(Type type, params Func<PropertyInfo, bool>[] filters)
            => filters.Aggregate(seed: type.GetProperties(CommonBindingFlags).AsEnumerable(), func: (accumulate, filter) => accumulate.Where(filter));
    }
}