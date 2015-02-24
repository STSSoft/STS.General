﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace STS.General.Persist
{
    public interface IPersist
    {
    }

    public interface IPersist<T> : IPersist
    {
        void Write(BinaryWriter writer, T item);
        T Read(BinaryReader reader);
    }
}
