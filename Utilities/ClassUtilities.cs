using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public static class ClassUtilities
    {
        public static Dictionary<string, string> ToDictionary<T>(T obj)
        {
            return ToRecursiveDictionary(obj).ToDictionary(prop => prop.Item1, prop => prop.Item2);
        }

        private static (string, string)[] ToRecursiveDictionary<T>(T obj, string append = "")
        {
            var propAppend = (string.IsNullOrEmpty(append)) ? "" : $"{append}.";
            if (obj == null) return new (string, string)[] { };

            return obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .SelectMany(prop =>
                {
                    if (prop.PropertyType.IsAssignableFrom(typeof(string)))
                    {
                        return new (string, string)[] {
                            (propAppend + prop.Name, prop.GetValue(obj, null)?.ToString() ?? "")
                        };
                    }
                    else if (prop.PropertyType.IsClass)
                    {
                        return ToRecursiveDictionary(prop.GetValue(obj, null), prop.Name);
                    }
                    else
                    {
                        return new (string, string)[] {
                            (propAppend + prop.Name, prop.GetValue(obj, null)?.ToString() ?? "")
                        };
                    }
                })
                .ToArray();
        }
    }
}
