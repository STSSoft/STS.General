using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace STS.General.IO
{
    /// <summary>
    /// An optimized FileStram - optimizes calls to Seek & Size methods
    /// The requirement is if the file is opened for writing, it is an exclusive.
    /// </summary>
    public class OptimizedFileStream : FileStream
    {
        protected long position = 0;
        protected long length;

        public OptimizedFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions fileOption, long length = long.MinValue)
            : base(path, mode, access, share, bufferSize, fileOption)
        {
            this.length = length;
        }

        public OptimizedFileStream(string fileName, FileMode mode, FileAccess access, long length = long.MinValue)
            : base(fileName, mode, access)
        {
            this.length = length;
        }

        public OptimizedFileStream(string fileName, FileMode mode, long length = long.MinValue)
            : base(fileName, mode)
        {
            this.length = length;
        }

        public override long Position
        {
            get { return position; }
            set
            {
                if (position != value)
                {
                    base.Position = value;
                    position = value;
                }
            }
        }

        public override void Write(byte[] array, int offset, int count)
        {
            try
            {
                base.Write(array, offset, count);

                position += count;

                if (position > Length)
                    length = position;
            }
            catch (Exception exc)
            {
                length = long.MinValue;
                throw exc;
            }
        }

        public override void WriteByte(byte value)
        {
            try
            {
                base.WriteByte(value);

                position++;

                if (position > Length)
                    length = position;
            }
            catch (Exception exc)
            {
                length = long.MinValue;
                throw exc;
            }
        }

        public override int Read(byte[] array, int offset, int count)
        {
            int readed = base.Read(array, offset, count);

            position += readed;

            return readed;
        }

        public override int ReadByte()
        {
            int readed = base.ReadByte();

            if (readed >= 0)
                position++;

            return readed;
        }

        public override long Length
        {
            get
            {
                if (length == long.MinValue)
                    length = base.Length;

                return length;
            }
        }

        public override void SetLength(long value)
        {
            try
            {
                base.SetLength(value);

                length = value;
            }
            catch (Exception exc)
            {
                length = long.MinValue;

                throw exc;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        if (offset != Position)
                            position = base.Seek(offset, SeekOrigin.Begin);
                    }
                    break;
                case SeekOrigin.Current:
                    {
                        if (offset != 0)
                            position = base.Seek(offset, SeekOrigin.Current);
                    }
                    break;
                case SeekOrigin.End:
                    {
                        if (offset != Length - Position)
                            position = base.Seek(offset, SeekOrigin.End);
                    }
                    break;
            }

            return position;
        }
    }
}
