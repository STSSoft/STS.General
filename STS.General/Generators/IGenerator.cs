using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Generators
{
    public interface IGenerator
    {
        long NextInt64();
    }
}
