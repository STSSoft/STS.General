﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS.General.Data
{
    public interface IToString<T> : ITransformer<T, string>
    {
    }
}
