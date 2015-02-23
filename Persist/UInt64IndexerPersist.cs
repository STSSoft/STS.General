using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class UInt64IndexerPersist : IIndexerPersist<UInt64>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, ulong> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return (long)values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, ulong> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid UInt64IndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (ulong)v); }, count);
        }
    }
}
