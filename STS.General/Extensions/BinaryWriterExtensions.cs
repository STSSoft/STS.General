using STS.General.Buffers;
using STS.General.Comparers;
using STS.General.IO;
using STS.General.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Text;

namespace STS.General.Extensions
{
    public static class BinaryWriterExtensions
    {
        private static MemberReflector<BinaryWriter, byte[]> largeByteBufferAccess = new MemberReflector<BinaryWriter, byte[]>("_largeByteBuffer");
        private static CommonArray common;

        //private const int LargeByteBufferSize = 0x100;
        private static readonly int LARGE_BYTE_BUFFER_SIZE = new MemberReflector<BinaryWriter, int>("LargeByteBufferSize").Get(null);

        private static byte[] ObtainLargeByteBuffer(this BinaryWriter writer)
        {
            byte[] buffer = largeByteBufferAccess.Get(writer);

            if (buffer == null)
            {
                buffer = new byte[LARGE_BYTE_BUFFER_SIZE];
                largeByteBufferAccess.Set(writer, buffer);
            }

            return buffer;
        }

        public static void WriteLine(this BinaryWriter writer, string line)
        {
            int len = line.Length;
            char[] array = new char[len + 2];

            line.CopyTo(0, array, 0, len);
            array[len] = '\r';
            array[len + 1] = '\n';

            writer.Write(array);
        }

        #region Array Extensions

        private static void WriteArray<T>(BinaryWriter writer, T[] array, int index, int count, int ITEM_SIZE)
        {
            //if(writer == null)
            //throw new ArgumentNullException("writer");

            //if (array == null)
            //throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (index + count > array.Length)
                throw new ArgumentException("index + count > array.Length");

            byte[] buffer = ObtainLargeByteBuffer(writer);

            int chunk = LARGE_BYTE_BUFFER_SIZE;
            count = count * ITEM_SIZE;

            while (count > 0)
            {
                if (chunk > count)
                    chunk = count;

                Buffer.BlockCopy(array, index, buffer, 0, chunk);
                writer.Write(buffer, 0, chunk);

                index += chunk;
                count -= chunk;
            }
        }

        /// <summary>
        /// up to 14x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, long[] array, int index, int count)
        {
            WriteArray<long>(writer, array, index, count, sizeof(long));
        }

        /// <summary>
        /// up to 14x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, ulong[] array, int index, int count)
        {
            WriteArray<ulong>(writer, array, index, count, sizeof(ulong));
        }

        /// <summary>
        /// up to 18x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, int[] array, int index, int count)
        {
            WriteArray<int>(writer, array, index, count, sizeof(int));
        }

        /// <summary>
        /// up to 18x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, uint[] array, int index, int count)
        {
            WriteArray<uint>(writer, array, index, count, sizeof(uint));
        }

        /// <summary>
        /// up to 19x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, short[] array, int index, int count)
        {
            WriteArray<short>(writer, array, index, count, sizeof(short));
        }

        /// <summary>
        /// up to 18x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, ushort[] array, int index, int count)
        {
            WriteArray<ushort>(writer, array, index, count, sizeof(ushort));
        }

        /// <summary>
        /// up to 14x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, double[] array, int index, int count)
        {
            WriteArray<double>(writer, array, index, count, sizeof(double));
        }

        /// <summary>
        /// up to 19x
        /// </summary>
        public static void WriteArray(this BinaryWriter writer, float[] array, int index, int count)
        {
            WriteArray<float>(writer, array, index, count, sizeof(float));
        }

        public static void WriteArray(this BinaryWriter writer, decimal[] array, int index, int count)
        {
            //if (writer == null)
            //    throw new ArgumentNullException("writer");

            //if (array == null)
            //    throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (index + count > array.Length)
                throw new ArgumentException("index + count > array.Length");

            for (int i = 0; i < count; i++)
                writer.Write(array[index + i]);
        }

        #endregion

        #region List Extensions

        private static void WriteList<T>(BinaryWriter writer, List<T> list, int ITEM_SIZE)
        {
            //if(writer == null)
            //  throw new ArgumentNullException("writer");

            //if (list == null)
            //  throw new ArgumentNullException("list");

            T[] internalArray = list.GetArray();
            byte[] buffer = ObtainLargeByteBuffer(writer);

            int chunk = LARGE_BYTE_BUFFER_SIZE;
            int count = list.Count * ITEM_SIZE;
            int index = 0;

            while (count > 0)
            {
                if (chunk > count)
                    chunk = count;

                Buffer.BlockCopy(internalArray, index, buffer, 0, chunk);
                writer.Write(buffer, 0, chunk);

                index += chunk;
                count -= chunk;
            }
        }

        public static void WriteList(this BinaryWriter writer, List<long> list)
        {
            WriteList<long>(writer, list, sizeof(long));
        }

        public static void WriteList(this BinaryWriter writer, List<ulong> list)
        {
            WriteList<ulong>(writer, list,sizeof(ulong));
        }

        public static void WriteList(this BinaryWriter writer, List<int> list)
        {
            WriteList<int>(writer, list, sizeof(int));
        }

        public static void WriteList(this BinaryWriter writer, List<uint> list)
        {
            WriteList<uint>(writer, list, sizeof(uint));
        }

        public static void WriteList(this BinaryWriter writer, List<short> list)
        {
            WriteList<short>(writer, list, sizeof(short));
        }

        public static void WriteList(this BinaryWriter writer, List<ushort> list)
        {
            WriteList<ushort>(writer, list, sizeof(ushort));
        }

        public static void WriteList(this BinaryWriter writer, List<double> list)
        {
            WriteList<double>(writer, list, sizeof(double));
        }

        public static void WriteList(this BinaryWriter writer, List<float> list)
        {
            WriteList<float>(writer, list, sizeof(float));
        }

        public static void WriteList(this BinaryWriter writer, List<decimal> list)
        {
            //if (writer == null)
            //    throw new ArgumentNullException("writer");

            //if (list == null)
            //    throw new ArgumentNullException("array");

            foreach(var item in list)
                writer.Write(item);
        }

        #endregion
    }
}
