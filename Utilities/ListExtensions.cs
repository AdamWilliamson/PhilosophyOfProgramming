using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class ListExtensions
    {
        public static IList<T> ChangeIndex<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }

        public static void RemoveWhere<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i--);
                }
            }
        }

        public static void RemoveWhere<T, U>(this IDictionary<T, U> list, Func<T, U, bool> predicate)
        {
            var keys = list.Keys.ToList();
            foreach (var key in keys)
            {
                if (predicate(key, list[key]))
                {
                    list.Remove(key);
                }
            }
        }
    }
}
