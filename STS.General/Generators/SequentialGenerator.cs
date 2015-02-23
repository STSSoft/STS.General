using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public class SequentialGenerator : IGenerator
    {
        private long last;

        public SequentialGenerator(int seed)
        {
            last = seed;
        }

        public long NextInt64()
        {
            return last++;
        }
    }
}
