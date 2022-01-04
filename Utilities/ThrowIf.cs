using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Utilities
{
    public static class ThrowIf
    {
        public static class Value
        {
            public static void IsNull(object value, string propName)
            {
                if (value == null) throw new NullReferenceException($"Value ({propName}) is Null.");
            }
        }

        public static class Arguments
        {
            public static void AreNull(params Expression<Func<object>>[] expressions)
            {
                foreach (var selector in expressions)
                {
                    var memberSelector = selector.Body as MemberExpression;
                    if (memberSelector == null) throw new ArgumentException();
                    var constantSelector = memberSelector.Expression as ConstantExpression;
                    if (constantSelector == null) throw new ArgumentException();
                    object? value = (memberSelector.Member as FieldInfo)?
                        .GetValue(constantSelector.Value);

                    Debug.Assert(value != null);

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
