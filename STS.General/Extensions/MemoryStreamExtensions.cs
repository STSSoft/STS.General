using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;

namespace STS.General.Extensions
{
    public class MemoryStreamHelper
    {
        public static readonly MemoryStreamHelper Instance = new MemoryStreamHelper();

        public readonly Func<MemoryStream, int> ReadInt32;

        public MemoryStreamHelper()
        {
            ReadInt32 = CreateReadInt32Method().Compile();
        }

        private Expression<Func<MemoryStream, int>> CreateReadInt32Method()
        {
            var stream = Expression.Parameter(typeof(MemoryStream), "stream");

            MethodInfo internalReadInt32Method;

#if NETFX_CORE
            internalReadInt32Method = stream.Type.GetMethod("InternalReadInt32");
#else
            internalReadInt32Method = stream.Type.GetMethod("InternalReadInt32", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

            var method = Expression.Call(stream, internalReadInt32Method);

            return Expression.Lambda<Func<MemoryStream, int>>(Expression.Label(Expression.Label(typeof(int)), method), stream);
        }
    }

    public static class MemoryStreamExtensions
    {
        public static int ReadInt32(this MemoryStream stream)
        {
            return MemoryStreamHelper.Instance.ReadInt32(stream);
        }
    }
}
