using STS.General.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public class SemiRandomGenerator : IGenerator
    {
        private readonly Random Random1;
        private readonly Random Random2;

        private readonly IGenerator RandomnessGenerator;

        ulong LastValue;

        public SemiRandomGenerator(int seed1,int seed2, IGenerator randomnessGenerator)
        {
            RandomnessGenerator = randomnessGenerator;

            Random1 = new Random(seed1);
            Random2 = new Random(seed2);

            LastValue = (ulong)Random1.Next();
        }

        /// <summary> 
        /// Random generator with sequental sub series.</summary>
        /// <param name="randomness">
        ///     [0.0]    -Full Sequental,
        ///     [1.0]    -Full Random,
        ///     (0.0,1.0)-combination of both</param>
        public SemiRandomGenerator(int seed1,int seed2, double randomness)
            : this(seed1, seed2, new CoinTossGenerator(randomness))
        {
        }

        /// <summary> 
        /// Random generator with sequental sub series.</summary> 
        /// <param name="randomness">
        ///     [0.0]    -Full Sequental,
        ///     [1.0]    -Full Random,
        ///     (0.0,1.0)-combination of both</param>
        public SemiRandomGenerator(double randomness)
            : this(DateTime.Now.Ticks.GetHashCode(), DateTime.Now.Ticks.GetHashCode(), randomness)
        {
        }

        public long NextInt64()
        {
            if (RandomnessGenerator.NextInt64() == 0)
            {
                LastValue = (ulong)Random1.Next();
                LastValue <<= 32;
                LastValue |= (ulong)Random2.Next();
            }
            else
                LastValue++;

            return (long)LastValue;
        }
    }
}
