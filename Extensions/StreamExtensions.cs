using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Extensions
{
    public static class StreamExtensions
    {
        public static void FillZero(this Stream stream)
        {
            stream.Position = 0;

            byte[] buffer = new byte[8 * 1024];

            long count = stream.Length / buffer.Length;
            for (int i = 0; i < count; i++)
                stream.Write(buffer, 0, buffer.Length);

            long restLength = stream.Length % buffer.Length;
            if (restLength > 0)
                stream.Write(buffer, 0, (int)restLength);

            stream.Flush();

            stream.Position = 0;
        }
    }
}
