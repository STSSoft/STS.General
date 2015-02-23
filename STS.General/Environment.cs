using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General
{
    public static class Environment
    {
        /// <summary>
        /// Check if the runtime is Mono.
        /// </summary>
        public static readonly bool RunningOnMono = Type.GetType("Mono.Runtime") != null;
    }
}
