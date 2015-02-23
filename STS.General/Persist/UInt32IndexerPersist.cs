using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class UInt32IndexerPersist : IIndexerPersist<UInt32>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, uint> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, uint> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt32IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (uint)v); }, count);
        }
    }
}
