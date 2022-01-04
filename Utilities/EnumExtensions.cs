using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public static class EnumHelper
    {
        public static List<T> Values<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToList();
        }
    }
}
