using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class Int32IndexerPersist : IIndexerPersist<Int32>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, int> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, int> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid Int32IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (int)v); }, count);
        }
    }
}
