using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public static class TicksGenerator
    {
        private static readonly TickGenerator recGenerator = new TickGenerator();

        public static IEnumerable<KeyValuePair<long, Tick>> GetFlow(long number, IGenerator keyGenerator)
        {
            for (long i = 0; i < number; i++)
            {
                long key = keyGenerator.NextInt64();
                Tick tick = recGenerator.Next();

                yield return new KeyValuePair<long, Tick>(key, tick);
            }
        }
    }
}
