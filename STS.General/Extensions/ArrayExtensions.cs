using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STS.General.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Copy<T>(this T[] array)
        {
            T[] result = new T[array.Length];
            Array.Copy(array, result, array.Length);

            return result;
        }

        public static T[] Middle<T>(this T[] buffer, int index, int length)
        {
            T[] middle = new T[length];
            Array.Copy(buffer, index, middle, 0, length);
            return middle;
        }

        public static T[] WithoutMiddle<T>(this T[] array, int index, int count)
        {
            T[] result = new T[array.Length - count];

            if (index > 0)
            {
                //there is left part to copy
                Array.Copy(array, 0, result, 0, index);
            }

            if (index + count < array.Length)
            {
                //there is right part to copy
                Array.Copy(array, index + count, result, index, array.Length - (index + count));
            }

            return result;
        }

        public static T[] Left<T>(this T[] buffer, int length)
        {
            return buffer.Middle(0, length);
        }

        public static T[] Right<T>(this T[] buffer, int length)
        {
            return buffer.Middle(buffer.Length - length, length);
        }

        public static string ToString<T>(this T[] array, string separator)
        {
            return "{" + String.Join<T>(separator, array) + "}";
        }

        public static List<T> CreateList<T>(this T[] array, int count)
        {
            List<T> list = new List<T>();

            list.SetArray(array);
            list.SetCount(count);
            //list.IncrementVersion();

            return list;
        }

        public static int RemoveAll<T>(this T[] array, Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentException("match");

            int index = 0;
            int count = array.Length;

            while (index < count && !match(array[index]))
                index++;

            if (index >= count)
                return 0;

            int idx = index + 1;

            while (idx < count)
            {
                while (idx < count && match(array[idx]))
                    idx++;

                if (idx < count)
                    array[index++] = array[idx++];
            }

            int removed = count - index;

            Array.Clear(array, index, removed);

            return removed;
        }

        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            T tmp = array[index1];
            array[index1] = array[index2];
            array[index2] = tmp;
        }

        public static void InsertionSort<T>(this T[] array, int index, int count, Comparison<T> comparison)
        {
            int limit = index + count;
            for (int i = index + 1; i < limit; i++)
            {
                var item = array[i];

                int j = i - 1;
                while (comparison(array[j], item) > 0)
                {
                    array[j + 1] = array[j];
                    j--;
                    if (j < index)
                        break;
                }

                array[j + 1] = item;
            }
        }

        public static void InsertionSort<T>(this T[] array, Comparison<T> comparison)
        {
            InsertionSort<T>(array, 0, array.Length, comparison);
        }

        public static void InsertionSort<T>(this T[] array, int index, int count, IComparer<T> comparer)
        {
            InsertionSort<T>(array, index, count, comparer.Compare);
        }

        public static void InsertionSort<T>(this T[] array, IComparer<T> comparer)
        {
            InsertionSort<T>(array, 0, array.Length, comparer);
        }

        private static void QuickSort<T>(T[] array, int low, int high, IComparer<T> comparer, int maxDepth, int depth)
        {
            if (high - low <= 16)
            {
                array.InsertionSort(low, high - low + 1, comparer);

                return;
            }

            int i = low;
            int j = high;
            T pivot = array[(low + high) / 2];

            do
            {
                while (comparer.Compare(array[i], pivot) < 0)
                    i++;

                while (comparer.Compare(array[j], pivot) > 0)
                    j--;

                if (i <= j)
                {
                    array.Swap(i, j);
                    i++; j--;
                }

            } while (i <= j);

            if (maxDepth <= depth)
            {
                if (i < high)
                    QuickSort(array, i, high, comparer, maxDepth, depth + 1);
                if (low < j)
                    QuickSort(array, low, j, comparer, maxDepth, depth + 1);
            }
            else
            {
                Action leftAction = () => QuickSort(array, i, high, comparer, maxDepth, depth + 1);
                Action rightAction = () => QuickSort(array, low, j, comparer, maxDepth, depth + 1);

                if (i < high && low < j)
                    Parallel.Invoke(leftAction, rightAction);
                else if (i < high)
                    Parallel.Invoke(leftAction);
                else if (low < j)
                    Parallel.Invoke(rightAction);
            }
        }

        public static void QuickSort<T>(this T[] array, IComparer<T> comparer)
        {
            QuickSort(array, 0, array.Length - 1, comparer, System.Environment.ProcessorCount * 2, 0);
        }

        public static bool IsOrdered<T>(this T[] array, int index, int count, IComparer<T> comparer, bool strictMonotone = false)
        {
            if (count == 0)
                return true;

            int limit = strictMonotone ? -1 : 0;
            int toExclusive = index + count;

            for (int i = index + 1; i < toExclusive; i++)
            {
                if (comparer.Compare(array[i - 1], array[i]) > limit)
                    return false;
            }

            return true;
        }

        public static bool IsOrdered<T>(this T[] array, bool strictMonotone = false)
        {
            return array.IsOrdered(0, array.Length, Comparer<T>.Default, strictMonotone);
        }
    }
}
