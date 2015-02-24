using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class Int64IndexerPersist : IIndexerPersist<Int64>
    {
        public const byte VERSION = 40;

        private long[] factors;

        /// <summary>
        /// This contructor gets the factors in ascending order
        /// </summary>
        public Int64IndexerPersist(long[] factors)
        {
            this.factors = factors;
        }

        public Int64IndexerPersist()
            : this(new long[0])
        {
        }

        public void Store(BinaryWriter writer, Func<int, long> values, int count)
        {
            writer.Write(VERSION);

            long[] array = new long[count];

            int index = factors.Length - 1;
            for (int i = 0; i < count; i++)
            {
                long value = values(i);
                array[i] = value;

                while (index >= 0)
                {
                    if (value % factors[index] == 0)
                        break;
                    else
                        index--;
                }
            }

            long factor = index >= 0 ? factors[index] : 1;

            DeltaCompression.Helper helper = new DeltaCompression.Helper();
            for (int i = 0; i < count; i++)
            {
                array[i] /= factor;
                helper.Add(array[i]);
            }

            CountCompression.Serialize(writer, checked((ulong)factor));
            DeltaCompression.Compress(writer, array, 0, count, helper);
        }

        public void Load(BinaryReader reader, Action<int, long> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int64IndexerPersist version.");

            long factor = (long)CountCompression.Deserialize(reader);

            DeltaCompression.Decompress(reader, (idx, val) => values(idx, factor * val), count);
        }
    }
}
