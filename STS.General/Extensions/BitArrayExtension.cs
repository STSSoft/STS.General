using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Extensions
{
    public class BitArrayHelper
    {
        public Func<BitArray, int[]> GetArray;

        public static readonly BitArrayHelper Instance = new BitArrayHelper();

        public BitArrayHelper()
        {
            GetArray = CreateGetArrayMethod().Compile();
        }

        public Expression<Func<BitArray, int[]>> CreateGetArrayMethod()
        {
            var array = Expression.Parameter(typeof(BitArray), "m_array");
            var items = Expression.PropertyOrField(array, "m_array");

            return Expression.Lambda<Func<BitArray, int[]>>(Expression.Label(Expression.Label(typeof(int[])), items), array);
        }
    }

    public static class BitArrayExtension
    {
        /// <summary>
        /// Return ints representation of array
        /// </summary> 
        public static int[] GetArray(this BitArray array)
        {
            return BitArrayHelper.Instance.GetArray(array);
        }

        private static bool Get(int[] array, int index)
        {
            return (array[index / 32] & (((int)1) << (index % 32))) != 0;
        }

        /// <summary>
        /// Return next index from position(include position) with value, if value dont present in array return -1.
        /// </summary> 
        public static int FindNext(this BitArray array, int index, bool value)
        {
            if (0 > index || index >= array.Length)
                throw new ArgumentException("0 > index || position >= array.Length");

            if (array.Length == 0)
                return -1;

            int length = array.Length;
            int localIndex = 0;
            int[] internalArray = array.GetArray();

            int idx = index / 32;

            if (value)
            {
                if (index % 32 > 0 && (internalArray[idx] >> (index % 32)) != 0)
                {
                    localIndex = index % 32;
                    int intValue = internalArray[idx];

                    while (((intValue >> localIndex) & 1) == 0)
                    {
                        if (++localIndex == 32)
                            return -1;
                    }

                    if ((idx * 32) + localIndex >= length)
                        return -1;
                }
                else
                {
                    while (idx < internalArray.Length && internalArray[idx] == 0)
                        idx++;

                    if (idx == internalArray.Length)
                        return -1;

                    int intValue = internalArray[idx];

                    while (((intValue >> localIndex) & 1) == 0)
                    {
                        if (++localIndex == 32)
                            return -1;
                    }

                    if ((idx * 32) + localIndex >= length)
                        return -1;
                }
            }
            else
            {
                if (index % 32 > 0 && (internalArray[idx] >> (index % 32)) != 0)
                {
                    localIndex = index % 32;
                    int intValue = internalArray[idx];

                    while (((intValue >> localIndex) & 1) == 1)
                    {
                        if (++localIndex == 32)
                            return -1;
                    }

                    if ((index * 32) + localIndex >= length)
                        return -1;
                }
                else
                {
                    while (idx < internalArray.Length && internalArray[idx] == -1)
                        idx++;

                    if (idx == internalArray.Length)
                        return -1;

                    int intValue = internalArray[idx];

                    while (((intValue >> localIndex) & 1) == 1)
                    {
                        if (++localIndex == 32)
                            return -1;
                    }

                    if ((idx * 32) + localIndex >= length)
                        return -1;
                }
            }

            return localIndex + idx * 32;
        }

        /// <summary>
        /// Return prev index from position(include position) with value, if value dont present in array return -1.
        /// </summary> 
        public static int FindPrev(this BitArray array, int index, bool value)
        {
            if (0 > index || index >= array.Length)
                throw new ArgumentException("0 > index || position >= array.Length");

            if (array.Length == 0)
                return -1;

            int length = array.Length;
            int localIndex = 0;
            int[] internalArray = array.GetArray();

            int idx = index / 32;

            if (value)
            {
                int mask = (1 << ((index + 1) % 32)) - 1;

                if (index % 32 != 31 && (internalArray[index / 32] & mask) != 0)
                {
                    localIndex = index % 32;
                    int intValue = internalArray[idx];
                    int localValue = (intValue >> localIndex) & 1;

                    while (localValue == 0)
                    {
                        if (--localIndex == -1)
                            return -1;

                        localValue = (intValue >> localIndex) & 1;
                    }
                }
                else
                {
                    while (idx > -1 && internalArray[idx] == -1)
                        idx--;

                    if (idx == -1)
                        return -1;

                    localIndex = (idx + 1) * 32 >= length ? (length - 1) % 32 : 31;

                    int intValue = internalArray[idx];
                    int localValue = (intValue >> localIndex) & 1;

                    while (localValue == 0)
                    {
                        if (--localIndex == -1)
                            return -1;

                        localValue = (intValue >> localIndex) & 1;
                    }
                }
            }
            else
            {
                int mask = (1 << ((index + 1) % 32)) - 1;

                if (index % 32 != 31 && (internalArray[idx] & mask) != mask)
                {
                    localIndex = index % 32;
                    int intValue = internalArray[idx];
                    int localValue = (intValue >> localIndex) & 1;

                    while (localValue == 1)
                    {
                        if (--localIndex == -1)
                            return -1;

                        localValue = (intValue >> localIndex) & 1;
                    }
                }
                else
                {
                    while (idx > -1 && internalArray[idx] == -1)
                        idx--;

                    if (idx == -1)
                        return -1;

                    localIndex = (idx + 1) * 32 >= length ? (length - 1) % 32 : 31;

                    int intValue = internalArray[idx];
                    int localValue = (intValue >> localIndex) & 1;

                    while (localValue == 1)
                    {
                        if (--localIndex == -1)
                            return -1;

                        localValue = (intValue >> localIndex) & 1;
                    }
                }
            }

            return localIndex + idx * 32;
        }


        /// <summary>
        /// Return first index with value, if value dont present in array return -1.
        /// </summary> 
        public static int FindFirst(this BitArray array, bool value)
        {
            if (array.Length == 0)
                return -1;

            return FindNext(array, 0, value);
        }

        /// <summary>
        /// Return last index with value, if value dont present in array return -1.
        /// </summary> 
        public static int FindLast(this BitArray array, bool value)
        {
            if (array.Length == 0)
                return -1;

            return FindPrev(array, array.Length - 1, value);
        }

        public static IEnumerable<int> Where(this BitArray array, bool value)
        {
            for (int idx = 0; idx < array.Length; idx++)
            {
                if (array[idx] == value)
                    yield return idx;
            }

            //TODO: da se opravi
            //for (int idx = 0; idx < array.Length; idx++)
            //{
            //    idx = array.FindNext(idx, value);
            //    if (idx >= 0)
            //        yield return idx;
            //}

            //find next breaks when index is out of the array 
            //int idx = -1;

            //while ((idx = array.FindNext(idx + 1, value)) >= 0)
            //    yield return idx;
        }

        public static void SetAll(this BitArray array, bool value, int index, int count)
        {
            //TODO: Must be optimized

            for (int i = 0; i < count; i++)
                array[index + i] = value;
        }
    }
}
