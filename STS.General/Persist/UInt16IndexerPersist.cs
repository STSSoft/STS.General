using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class UInt16IndexerPersist : IIndexerPersist<UInt16>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, ushort> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, ushort> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt16IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (ushort)v); }, count);
        }
    }
}
