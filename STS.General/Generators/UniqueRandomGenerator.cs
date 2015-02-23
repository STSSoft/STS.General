using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public class UniqueRandomGenerator : IGenerator
    {
        private Random Random;
        private long Last;

        public UniqueRandomGenerator(int seed)
        {
            Random = new Random(seed);
            Last = Random.Next();
        }

        public UniqueRandomGenerator()
            :this(DateTime.Now.Ticks.GetHashCode())
        {
        }

        public long NextInt64()
        {
            long rand = Random.Next();
            long randomPart = 0L;
            randomPart = randomPart | (rand & 0x000000ff);
            randomPart = randomPart | (rand & 0x0000ff00) << 8;
            randomPart = randomPart | (rand & 0x00ff0000) << 16;
            randomPart = randomPart | (rand & 0xff000000) << 24;

            Last++;
            long sequentalPart = 0L;
            sequentalPart = sequentalPart | (Last & 0x000000ff) << 8;
            sequentalPart = sequentalPart | (Last & 0x0000ff00) << 16;
            sequentalPart = sequentalPart | (Last & 0x00ff0000) << 24;
            sequentalPart = sequentalPart | (Last & 0xff000000) << 32;

            return sequentalPart | randomPart;
        }
    }
}
