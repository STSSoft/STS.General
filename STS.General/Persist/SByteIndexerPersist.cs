using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class SByteIndexerPersist : IIndexerPersist<SByte>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, sbyte> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, sbyte> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid SByteIndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (sbyte)v); }, count);
        }
    }
}
