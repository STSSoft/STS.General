using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.Storage;
using Windows.Storage.Streams;
using System.IO;
#endif

namespace STS.General.IO
{
#if NETFX_CORE

    public class WindowsUniversalFileStream : Stream
    {
        private IRandomAccessStream RandomAccessStream;
        private Stream WriteStream;
        private Stream ReadStream;

        public StorageFolder Directory { get; private set; }
        public string FileName { get; private set; }

        public FileAccessMode Mode { get; private set; }
        public CreationCollisionOption CollisionOptions { get; private set; }
        public StorageOpenOptions OpenOptions { get; private set; }

        public int WriteBufferSize { get; private set; }
        public int ReadBufferSize { get; private set; }

        public WindowsUniversalFileStream(string fileName, StorageFolder directory, FileAccessMode mode, CreationCollisionOption collisionOptions, StorageOpenOptions openOptions, int length = 1024 * 80, int writeBufferSize = 1024 * 80, int readBufferSize = 1024 * 80)
        {
            if (fileName == null || fileName == string.Empty)
                throw new ArgumentNullException("path");

            Directory = directory;
            FileName = fileName;
            Mode = mode;
            CollisionOptions = collisionOptions;
            OpenOptions = openOptions;

            WriteBufferSize = writeBufferSize;
            ReadBufferSize = readBufferSize;

            RandomAccessStream = Directory.CreateFileAsync(FileName, collisionOptions).AsTask().Result.OpenAsync(mode, openOptions).AsTask().Result;
            ReadStream = RandomAccessStream.AsStreamForRead(ReadBufferSize);
            ReadStream.SetLength(length);

            if (mode == FileAccessMode.ReadWrite)
                WriteStream = RandomAccessStream.AsStreamForWrite(WriteBufferSize);
        }

        public WindowsUniversalFileStream(string fileName, StorageFolder directory)
            : this(fileName, directory, FileAccessMode.ReadWrite, CreationCollisionOption.ReplaceExisting, StorageOpenOptions.None)
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            return ReadStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos = ReadStream.Seek(offset, origin);

            if (Mode == FileAccessMode.Read)
                return pos;

            return WriteStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (Mode == FileAccessMode.ReadWrite)
                WriteStream.SetLength(value);

            ReadStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Mode == FileAccessMode.Read)
                throw new InvalidOperationException("Stream is only for read. Create new stream with FileMode.Read/Write.");

            WriteStream.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            if (Mode == FileAccessMode.Read)
                throw new InvalidOperationException("Stream is only for read. Create new stream with FileMode.Read/Write.");

            WriteStream.WriteByte(value);
        }

        public override void Flush()
        {
            if (Mode == FileAccessMode.ReadWrite)
                WriteStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Mode == FileAccessMode.ReadWrite)
                {
                    WriteStream.Flush();
                    WriteStream.Dispose();
                }

                ReadStream.Dispose();
                RandomAccessStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Properties

        public override bool CanRead
        {
            get
            {
                return ReadStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (Mode == FileAccessMode.ReadWrite)
                    return ReadStream.CanSeek && WriteStream.CanSeek;

                return ReadStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (Mode == FileAccessMode.Read)
                    return false;

                return WriteStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (Mode == FileAccessMode.Read)
                    return ReadStream.Length;

                return WriteStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return (long)RandomAccessStream.Position;
            }

            set
            {
                if (Mode == FileAccessMode.ReadWrite)
                    WriteStream.Position = value;

                ReadStream.Position = value;
            }
        }
        #endregion
    }
#endif
}
