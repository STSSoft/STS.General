using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace STS.General.Extensions
{
    public static class IListExtensions
    {
        public static T RemoveLast<T>(this IList<T> list)
        {
            var item = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);

            return item;
        }

        public static int BinarySearch<T>(this IList<T> array, int index, int length, T value, IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            int low = index;
            int high = index + length - 1;

            while (low <= high)
            {
                int mid = (low + high) >> 1;
                int cmp = comparer.Compare(array[mid], value);

                if (cmp == 0)
                    return mid;
                if (cmp < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return ~low;
        }

        public static int BinarySearch<T>(this IList<T> array, int index, int length, T value)
        {
            return BinarySearch<T>(array, index, length, value, Comparer<T>.Default);
        }

        public static int BinarySearch(this IList array, int index, int length, object value, IComparer comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            int low = index;
            int high = index + length - 1;

            while (low <= high)
            {
                int mid = (low + high) >> 1;
                int cmp = comparer.Compare(array[mid], value);

                if (cmp == 0)
                    return mid;
                if (cmp < 0)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return ~low;
        }

        public static int BinarySearch(this IList array, int index, int length, object value)
        {
#if NETFX_CORE
            return BinarySearch(array, index, length, value, Comparer<object>.Default);
#else
            return BinarySearch(array, index, length, value, Comparer.Default);
#endif
        }
    }
}
