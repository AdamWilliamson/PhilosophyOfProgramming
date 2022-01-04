using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CardPlay.Utilities
{
    public static class ThrowIf
    {
        public static class Arguments
        {
            //public static void AreNull(object argument, string argumentName)
            //{
            //    if (argument == null)
            //    {
            //        throw new ArgumentNullException(argumentName);
            //    }
            //}

            //public static void AreNull<T>(params Expression<Func<T>>[] expressions) where T : class
            //{
            //    foreach (var expression in expressions)
            //    {
            //        if (expression.Compile().Invoke() == null)
            //        {
            //            throw new ArgumentNullException(GetName(expression));
            //        }
            //    }
            //}

            //private static string GetName<T>(Expression<Func<T>> expression)
            //{
            //    return ((MemberExpression)expression.Body).Member.Name;
            //}

            public static void AreNull(params Expression<Func<object>>[] expressions)
            {
                foreach (var selector in expressions)
                {
                    var memberSelector = (MemberExpression)selector.Body;
                    if (memberSelector == null) throw new ArgumentException();
                    var constantSelector = memberSelector.Expression as ConstantExpression;
                    if (constantSelector == null) throw new ArgumentException();
                    object? value = (memberSelector.Member as FieldInfo)?.GetValue(constantSelector.Value);

                    if (value == null)
                    {
                        string name = ((MemberExpression)selector.Body).Member.Name;
                        throw new ArgumentNullException(name);
                    }
                }
            }
        }
    }
}

