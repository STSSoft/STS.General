using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace STS.General.Buffers
{
    public static class BitUtils
    {
        private const int CACHE_SIZE = 2048; //2^11

        private static int[] cache = new int[CACHE_SIZE];

        static BitUtils()
        {
            for (int i = 0; i < CACHE_SIZE; i++)
                cache[i] = GetBitBoundsClassic((ulong)i);
        }

        private static int GetBitBoundsClassic(ulong value)
        {
            return (value > 0) ? (int)Math.Ceiling(Math.Log(value + 1.0, 2)) : 1;
        }

        public static int GetBitBounds(ulong value)
        {
            int bits = 0;

            while (value >= CACHE_SIZE) //2^11
            {
                value = value >> 11;
                bits += 11;
            }

            return bits + cache[value];
        }
    }
}