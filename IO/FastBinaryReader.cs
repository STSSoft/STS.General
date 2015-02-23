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
    public class FastBinaryReader : BinaryReader
    {
        private static readonly Func<BinaryReader, byte[]> bufferGet = MemberReflector<BinaryReader, byte[]>.MemberGet("m_buffer");

        private MemoryStream memoryStream;
        private CommonArray common;
        private byte[] m_buffer;

        public FastBinaryReader(Stream output, Encoding encoding)
            : base(output, encoding)
        {
            Init();
        }

        public FastBinaryReader(Stream output)
            : base(output)
        {
            Init();
        }

        private void Init()
        {
            m_buffer = bufferGet(this);

            common = new CommonArray();
            common.ByteArray = m_buffer;

            memoryStream = BaseStream as MemoryStream;
        }

        /// <summary>
        /// +23%
        /// </summary>
        public override long ReadInt64()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(long));

            if (readByteCount != sizeof(long))
                IOUtils.ThrowEndOfFileError();

            return common.Int64Array[0];
        }

        /// <summary>
        /// +23%
        /// </summary>
        public override ulong ReadUInt64()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(ulong));

            if (readByteCount != sizeof(ulong))
                IOUtils.ThrowEndOfFileError();

            return common.UInt64Array[0];
        }

        /// <summary>
        /// +13%
        /// </summary>
        public override int ReadInt32()
        {
            if (memoryStream != null)
                return MemoryStreamHelper.Instance.ReadInt32(memoryStream);

            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(int));

            if (readByteCount != sizeof(int))
                IOUtils.ThrowEndOfFileError();

            return common.Int32Array[0];
        }

        /// <summary>
        /// +23%
        /// </summary>
        public override uint ReadUInt32()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(uint));

            if (readByteCount != sizeof(uint))
                IOUtils.ThrowEndOfFileError();

            return common.UInt32Array[0];
        }

        /// <summary>
        /// +22%
        /// </summary>
        public override double ReadDouble()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(double));

            if (readByteCount != sizeof(double))
                IOUtils.ThrowEndOfFileError();

            return common.DoubleArray[0];
        }

        /// <summary>
        /// +24%
        /// </summary>
        public override float ReadSingle()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(float));

            if (readByteCount != sizeof(float))
                IOUtils.ThrowEndOfFileError();

            return common.SingleArray[0];
        }

        /// <summary>
        /// +32%
        /// </summary>
        public override decimal ReadDecimal()
        {
            int readByteCount = this.BaseStream.Read(m_buffer, 0, sizeof(decimal));

            if (readByteCount != sizeof(decimal))
                IOUtils.ThrowEndOfFileError();

            return new decimal(common.Int32Array[0], common.Int32Array[1], common.Int32Array[2], (common.Int32Array[3] & (1 << 31)) == (1 << 31), (byte)(common.Int32Array[3] >> 16));
        }
    }
}
