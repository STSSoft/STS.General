using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class Int16IndexerPersist : IIndexerPersist<Int16>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, short> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, short> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int16IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (short)v); }, count);
        }
    }
}
