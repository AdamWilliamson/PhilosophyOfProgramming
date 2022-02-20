using System.Linq.Expressions;
using System.Text;

namespace Validations.Utilities
{
    public static class ExpressionUtilities
    {
        private static MemberExpression? GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression;
            }
            else if (expression is LambdaExpression lambdaExpression)
            {
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

        private static string GetIndexPath(Expression expression)
        {
            string path = "";
            if (expression is LambdaExpression lambdaExpression)
            {
                if (lambdaExpression?.Body is IndexExpression indexExpression)
                {
                    if (indexExpression.Arguments.Count == 1 && indexExpression.Arguments[0] is ConstantExpression constantExpression)
                    {
                        path = $"[{constantExpression.Value}]";
                        if (indexExpression.Object is MemberExpression memberExpression)
                        {
                            var temp = GetPropertyPath(memberExpression);
                            return temp + path;
                        }
                    }
                }
            }

            return "";
        }

        public static string GetPropertyPath(Expression expr)
        {
            var path = new StringBuilder();
            MemberExpression? memberExpression = GetMemberExpression(expr);
            if (memberExpression != null && memberExpression.Expression != null)
            {

                do
                {
                    if (path.Length > 0)
                    {
                        path.Insert(0, ".");
                    }
                    path.Insert(0, memberExpression.Member.Name);
#pragma warning disable CS8604 // Possible null reference argument.
                    memberExpression = GetMemberExpression(memberExpression.Expression);
#pragma warning restore CS8604 // Possible null reference argument.
                }
                while (memberExpression != null);
                return path.ToString();
            }
            else if (GetIndexPath(expr) is string name)
            {
                return name;
            }
            return "";
        }
    }
}