using STS.General.IO;
using STS.General.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace STS.General.Extensions
{
    public static class BinaryReaderExtensions
    {
        private static MemberReflector<BinaryReader, byte[]> largeByteBufferAccess = new MemberReflector<BinaryReader, byte[]>("m_charBytes");

        //private const int LargeByteBufferSize = 0x80;
        private static readonly int LARGE_BYTE_BUFFER_SIZE = new MemberReflector<BinaryReader, int>("MaxCharBytesSize").Get(null);

        private static byte[] ObtainLargeByteBuffer(this BinaryReader reader)
        {
            byte[] buffer = largeByteBufferAccess.Get(reader);

            if (buffer == null)
            {
                buffer = new byte[LARGE_BYTE_BUFFER_SIZE];
                largeByteBufferAccess.Set(reader, buffer);
            }

            return buffer;
        }

        public static string ReadLine(this BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();

            while (true)
            {
                int ch = reader.Read();

                switch (ch)
                {
                    case -1:
                        {
                            if (builder.Length > 0)
                                return builder.ToString();

                            return null;
                        }

                    case 13:
                    case 10:
                        {
                            if ((ch == 13) && (reader.PeekChar() == 10))
                                reader.Read();

                            return builder.ToString();
                        }
                }

                builder.Append((char)ch);
            }
        }

        #region Array Extensions

        private static void ReadArray<T>(BinaryReader reader, T[] array, int index, int count, int ITEM_SIZE)
        {
            //if(reader == null)
            //throw new ArgumentNullException("reader");

            //if (array == null)
            //throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (index + count > array.Length)
                throw new ArgumentException("index + count > array.Length");

            Stream stream = reader.BaseStream;

            if (stream == null)
                IOUtils.ThrowFileNotOpenError();

            byte[] buffer = ObtainLargeByteBuffer(reader);
            int chunk = LARGE_BYTE_BUFFER_SIZE;
            count = count * ITEM_SIZE;

            while (count > 0)
            {
                if (chunk > count)
                    chunk = count;

                if (stream.Read(buffer, 0, chunk) != chunk)
                    throw new ArgumentException("Cant be read");

                Buffer.BlockCopy(buffer, 0, array, index, chunk);

                index += chunk;
                count -= chunk;
            }
        }

        /// <summary>
        /// up to 9.6x
        /// </summary>
        public static void ReadInt64Array(this BinaryReader reader, long[] array, int index, int count)
        {
            ReadArray<long>(reader, array, index, count, sizeof(long));
        }

        /// <summary>
        /// up to 9.6x
        /// </summary>
        public static void ReadUInt64Array(this BinaryReader reader, ulong[] array, int index, int count)
        {
            ReadArray<ulong>(reader, array, index, count, sizeof(ulong));
        }

        /// <summary>
        /// up to 6.2x
        /// </summary>
        public static void ReadInt32Array(this BinaryReader reader, int[] array, int index, int count)
        {
            ReadArray<int>(reader, array, index, count, sizeof(int));
        }

        /// <summary>
        /// up to 12x
        /// </summary>
        public static void ReadUInt32Array(this BinaryReader reader, uint[] array, int index, int count)
        {
            ReadArray<uint>(reader, array, index, count, sizeof(uint));
        }

        /// <summary>
        /// up to 17x
        /// </summary>
        public static void ReadInt16Array(this BinaryReader reader, short[] array, int index, int count)
        {
            ReadArray<short>(reader, array, index, count, sizeof(short));
        }

        /// <summary>
        /// up to 19x
        /// </summary>
        public static void ReadUInt16Array(this BinaryReader reader, ushort[] array, int index, int count)
        {
            ReadArray<ushort>(reader, array, index, count, sizeof(ushort));
        }

        /// <summary>
        /// up to 10x
        /// </summary>
        public static void ReadDoubleArray(this BinaryReader reader, double[] array, int index, int count)
        {
            ReadArray<double>(reader, array, index, count, sizeof(double));
        }

        /// <summary>
        /// up to 13x
        /// </summary>
        public static void ReadSingleArray(this BinaryReader reader, float[] array, int index, int count)
        {
            ReadArray<float>(reader, array, index, count, sizeof(float));
        }

        public static void ReadDecimalArray(this BinaryReader reader, decimal[] array, int index, int count)
        {
            //if (reader == null)
            //    throw new ArgumentNullException("reader");

            //if (array == null)
            //    throw new ArgumentNullException("array");

            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (index + count > array.Length)
                throw new ArgumentException("index + count > array.Length");

            Stream stream = reader.BaseStream;

            if (stream == null)
                IOUtils.ThrowFileNotOpenError();

            for (int i = 0; i < count; i++)
                array[index + i] = reader.ReadDecimal();
        }

        #endregion

        #region List Extensions

        private static List<T> ReadList<T>(BinaryReader reader, int count, int ITEM_SIZE)
        {
            //if(reader == null)
            //throw new ArgumentNullException("reader");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            Stream stream = reader.BaseStream;

            if (stream == null)
                IOUtils.ThrowFileNotOpenError();

            List<T> list = new List<T>();
            list.SetCount(count);
            T[] array = new T[count];
            byte[] buffer = ObtainLargeByteBuffer(reader);
            int chunk = LARGE_BYTE_BUFFER_SIZE;
            count = count * ITEM_SIZE;
            int index = 0;

            while (count > 0)
            {
                if (chunk > count)
                    chunk = count;

                if (stream.Read(buffer, 0, chunk) != chunk)
                    throw new ArgumentException("Cant be read");

                Buffer.BlockCopy(buffer, 0, array, index, chunk);

                index += chunk;
                count -= chunk;
            }

           
            list.SetArray(array);

            return list;
        }

        public static List<long> ReadInt64AList(this BinaryReader reader, int count)
        {
            return ReadList<long>(reader, count, sizeof(long));
        }

        public static List<ulong> ReadUInt64List(this BinaryReader reader, int count)
        {
            return ReadList<ulong>(reader, count, sizeof(ulong));
        }

        public static List<int> ReadInt32List(this BinaryReader reader, int count)
        {
            return ReadList<int>(reader, count, sizeof(int));
        }

        public static List<uint> ReadUInt32List(this BinaryReader reader, int count)
        {
            return ReadList<uint>(reader, count, sizeof(uint));
        }

        public static List<short> ReadInt16List(this BinaryReader reader, int count)
        {
            return ReadList<short>(reader, count, sizeof(short));
        }

        public static List<ushort> ReadUInt16List(this BinaryReader reader, int count)
        {
            return ReadList<ushort>(reader, count, sizeof(ushort));
        }

        public static List<double> ReadDoubleList(this BinaryReader reader, int count)
        {
            return ReadList<double>(reader, count, sizeof(double));
        }

        public static List<float> ReadSingleList(this BinaryReader reader, int count)
        {
            return ReadList<float>(reader, count, sizeof(double));
        }

        public static List<decimal> ReadDecimalList(this BinaryReader reader, int count)
        {
            //if (reader == null)
            //    throw new ArgumentNullException("reader");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            Stream stream = reader.BaseStream;

            if (stream == null)
                IOUtils.ThrowFileNotOpenError();

            List<decimal> list = new List<decimal>();

            for (int i = 0; i < count; i++)
                list.Add(reader.ReadDecimal());

            return list;
        }

        #endregion
    }
}
