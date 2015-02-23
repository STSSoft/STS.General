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
    public class DecimalIndexerPersist : IIndexerPersist<Decimal>
    {
        public const byte VERSION = 40;

        private int GetMaxDigits(Func<int, decimal> values, int count)
        {
            int maxDigits = 0;
            for (int i = 0; i < count; i++)
            {
                decimal value = values(i);
                int digits = MathUtils.GetDigits(value);
                if (digits > maxDigits)
                    maxDigits = digits;
            }

            return maxDigits;
        }

        #region IIndexerPersist<decimal> Members

        public void Store(BinaryWriter writer, Func<int, decimal> values, int count)
        {
            writer.Write(VERSION);

            DeltaCompression.Helper helper = null;
            long[] array = null;
            int digits;

            try
            {
                digits = GetMaxDigits(values, count);
                if (digits <= 15)
                {
                    helper = new DeltaCompression.Helper();
                    array = new long[count];

                    decimal koef = (decimal)Math.Pow(10, digits);
                    for (int i = 0; i < count; i++)
                    {
                        decimal value = values(i);
                        long v = checked((long)Math.Round(value * koef));

                        array[i] = v;
                        helper.Add(v);
                    }
                }
                else
                    digits = -1;
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

        public void Load(BinaryReader reader, Action<int, decimal> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid DecimalIndexerPersist version.");

            int digits = reader.ReadSByte();

            if (digits >= 0)
            {
                double koef = Math.Pow(10, digits);
                DeltaCompression.Decompress(reader, (idx, val) => values(idx, (decimal)Math.Round(val / koef, digits)), count);
            }
            else //native read
            {
                for (int i = 0; i < count; i++)
                    values(i, reader.ReadDecimal());
            }
        }

        #endregion
    }
}
