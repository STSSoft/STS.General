using STS.General.Buffers;
using STS.General.Comparers;
using STS.General.Extensions;
using STS.General.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.IO
{
    public class FastBinaryWriter : BinaryWriter
    {
        private static readonly Func<BinaryWriter, byte[]> bufferGet = MemberReflector<BinaryWriter, byte[]>.MemberGet(!Environment.RunningOnMono ? "_buffer" : "buffer");

        private CommonArray common;
        private byte[] _buffer;

        public FastBinaryWriter(Stream output, Encoding encoding)
            : base(output, encoding)
        {
            Init();
        }

        public FastBinaryWriter(Stream output)
            : base(output)
        {
            Init();
        }

        private void Init()
        {
            _buffer = bufferGet(this);

            common = new CommonArray();
            common.ByteArray = _buffer;
        }

        /// <summary>
        /// +13%
        /// </summary>
        public override void Write(long value)
        {
            common.Int64Array[0] = value;

            this.OutStream.Write(_buffer, 0, sizeof(long));
        }

        /// <summary>
        /// +11%
        /// </summary>
        public override void Write(ulong value)
        {
            common.UInt64Array[0] = value;

            OutStream.Write(_buffer, 0, sizeof(ulong));
        }

        /// <summary>
        /// +7%
        /// </summary>
        public override void Write(int value)
        {
            common.Int32Array[0] = value;

            OutStream.Write(_buffer, 0, sizeof(int));
        }

        /// <summary>
        /// +7%
        /// </summary>
        public override void Write(uint value)
        {
            common.UInt32Array[0] = value;

            OutStream.Write(_buffer, 0, sizeof(uint));
        }

        /// <summary>
        /// +15%
        /// </summary>
        public override void Write(double value)
        {
            common.DoubleArray[0] = value;

            OutStream.Write(_buffer, 0, sizeof(double));
        }

        /// <summary>
        /// +10%
        /// </summary>
        public override void Write(float value)
        {
            common.SingleArray[0] = value;

            OutStream.Write(_buffer, 0, sizeof(float));
        }

        /// <summary>
        /// +17%
        /// </summary>
        public override void Write(decimal value)
        {
            DecimalHelper.Instance.Write(ref value, common.Int32Array, 0);

            OutStream.Write(_buffer, 0, 16);
        }
    }
}
