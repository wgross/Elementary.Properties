using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Elementary.Properties.Selectors
{
    public static class PropertyInfos
    {
        public static PropertyInfo PropertyFromMemberAccess<T>(Expression<Func<T, object?>> memberAccess)
        {
            var depth = 0;
            Expression exp = memberAccess;
            do
            {
                switch (exp)
                {
                    case LambdaExpression lambda:
                        exp = lambda.Body;
                        break;

                    case UnaryExpression convert when (convert.NodeType == ExpressionType.Convert):
                        exp = convert.Operand;
                        break;

                    case MemberExpression propertyAccess when propertyAccess.Member.MemberType == MemberTypes.Property:
                        return (PropertyInfo)propertyAccess.Member;

                    case MemberExpression otherAccess when otherAccess.Member.MemberType != MemberTypes.Property:
                        throw new ArgumentException($"Expression doesn't access a property but a '{otherAccess.Member.MemberType}' named '{otherAccess.Member.Name}' ");

                    default:
                        throw new InvalidOperationException($"property access contained unexpected expression: {exp.NodeType}");
                }

                depth++;
            }
            while (depth < 3); // lambda -> convert -> member

            throw new InvalidOperationException("property access expression was too deep (>3)");
        }

        public static string PropertyNameFromMemberAccess<T>(Expression<Func<T, object?>> memberAccess) => PropertyFromMemberAccess<T>(memberAccess).Name;
    }
}