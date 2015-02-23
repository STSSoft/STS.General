using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public class CoinTossGenerator : IGenerator
    {
        private readonly double Randomness;

        private readonly Random Random;

        public CoinTossGenerator(int seed, double randomness)
        {
            Randomness = randomness;

            Random = new Random(seed);
        }

        public CoinTossGenerator(double randomness)
            : this(DateTime.Now.Ticks.GetHashCode(), randomness)
        {
        }

        public long NextInt64()
        {
            if (Random.NextDouble() <= Randomness)
                return 0;
            else
                return 1;
        }
    }
}
