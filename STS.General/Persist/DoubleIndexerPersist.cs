using STS.General.Compression;
using STS.General.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Persist
{
    public class DoubleIndexerPersist : IIndexerPersist<Double>
    {
        public const byte VERSION = 40;

        private int GetMaxDigits(Func<int, double> values, int count)
        {
            int maxDigits = 0;
            for (int i = 0; i < count; i++)
            {
                double value = values(i);
                int digits = MathUtils.GetDigits(value);
                if (digits < 0)
                    return -1;

                if (digits > maxDigits)
                    maxDigits = digits;
            }

            return maxDigits;
        }

        public void Store(BinaryWriter writer, Func<int, double> values, int count)
        {
            writer.Write(VERSION);

            DeltaCompression.Helper helper = null;
            long[] array = null;
            int digits;

            try
            {
                digits = GetMaxDigits(values, count);
                if (digits >= 0)
                {
                    helper = new DeltaCompression.Helper();
                    array = new long[count];

                    double koef = Math.Pow(10, digits);
                    for (int i = 0; i < count; i++)
                    {
                        double value = values(i);
                        long v = checked((long)Math.Round(value * koef));

                        array[i] = v;
                        helper.Add(v);
                    }
                }
            }
            catch (OverflowException)
            {
                digits = -1;
            }

            writer.Write((sbyte)digits);
            if (digits >= 0)
                DeltaCompression.Compress(writer, array, 0, count, helper);
            else
            {
                for (int i = 0; i < count; i++)
                    writer.Write(values(i));
            }
        }

        public void Load(BinaryReader reader, Action<int, double> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid DoubleIndexerPersist version.");

            int digits = reader.ReadSByte();
            if (digits >= 0)
            {
                double koef = Math.Pow(10, digits);
                DeltaCompression.Decompress(reader, (idx, val) => values(idx, (double)Math.Round(val / koef, digits)), count);
            }
            else //native read
            {
                for (int i = 0; i < count; i++)
                    values(i, reader.ReadDouble());
            }
        }
    }
}
