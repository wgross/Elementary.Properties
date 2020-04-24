using System;
using System.Linq.Expressions;

namespace Elementary.Properties
{
    internal static class FrameworkExtensions
    {
        public static string MemberName<T, V>(this Expression<Func<T, V>> propertyAccessExpression)
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

                    case UnaryExpression convert when (convert.NodeType == ExpressionType.Convert):
                        exp = convert.Operand;
                        break;

                    case MemberExpression memberAccess:
                        return memberAccess.Member.Name;

                    default:
                        throw new InvalidOperationException($"property access contained unexpected expression: {exp.NodeType}");
                }

                depth++;
            }
            while (depth < 3); // lambda -> convert -> member

            throw new InvalidOperationException("property access expression was too deep (>3)");
        }

       
    }
}