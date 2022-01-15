using System.Linq.Expressions;
using System.Text;

namespace Validations.Utilities
{
    public static class ExpressionUtilities
    {
        private static MemberExpression? GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is LambdaExpression)
            {
                var lambdaExpression = expression as LambdaExpression;
                if (lambdaExpression?.Body is MemberExpression expression1)
                {
                    return expression1;
                }
                else if (lambdaExpression?.Body is UnaryExpression expression3)
                {
                    return (MemberExpression)expression3.Operand;
                }
            }
            return null;
        }

        public static string GetPropertyPath(Expression expr)
        {
            var path = new StringBuilder();
            MemberExpression? memberExpression = GetMemberExpression(expr);
            if (memberExpression == null || memberExpression.Expression == null) return "";

            do
            {
                if (path.Length > 0)
                {
                    path.Insert(0, ".");
                }
                path.Insert(0, memberExpression.Member.Name);
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }
            while (memberExpression != null);
            return path.ToString();
        }
    }
}