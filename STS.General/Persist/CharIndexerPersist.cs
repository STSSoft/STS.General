using System;
using System.IO;
using System.Collections.Generic;
using STS.General.Compression;

namespace STS.General.Persist
{
    public class CharIndexerPersist : IIndexerPersist<Char>
    {
        public const byte VERSION = 40;

        private readonly Int64IndexerPersist persist = new Int64IndexerPersist();

        public void Store(BinaryWriter writer, Func<int, char> values, int count)
        {
            writer.Write(VERSION);

            persist.Store(writer, (i) => { return values(i); }, count);
        }

        public void Load(BinaryReader reader, Action<int, char> values, int count)
        {
            if (reader.ReadByte() != VERSION)
                throw new Exception("Invalid CharIndexerPersist version.");

            persist.Load(reader, (i, v) => { values(i, (char)v); }, count);
        }
    }
}
