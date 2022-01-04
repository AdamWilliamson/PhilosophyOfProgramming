using System;

namespace Utilities
{
    public static class CircularBuffer
    {
        public static T GetCircularBuffer<T>(T[] items, int index)
        {
            return items[index % items.Length];
        }
    }
}
