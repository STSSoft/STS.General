using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS.General.Extensions
{
    public static class TypeCodeExtensions
    {
        private static Type[] types = new Type[19];

        static TypeCodeExtensions()
        {
            types[(int)TypeCode.Boolean] = typeof(Boolean);
            types[(int)TypeCode.Byte] = typeof(Byte);
            types[(int)TypeCode.Char] = typeof(Char);
            types[(int)TypeCode.DateTime] = typeof(DateTime);
            types[(int)TypeCode.DBNull] = typeof(DBNull);
            types[(int)TypeCode.Decimal] = typeof(Decimal);
            types[(int)TypeCode.Double] = typeof(Double);
            types[(int)TypeCode.Empty] = null;
            types[(int)TypeCode.Int16] = typeof(Int16);
            types[(int)TypeCode.Int32] = typeof(Int32);
            types[(int)TypeCode.Int64] = typeof(Int64);
            types[(int)TypeCode.Object] = typeof(Object);
            types[(int)TypeCode.SByte] = typeof(SByte);
            types[(int)TypeCode.String] = typeof(String);
            types[(int)TypeCode.UInt16] = typeof(UInt16);
            types[(int)TypeCode.UInt32] = typeof(UInt32);
            types[(int)TypeCode.UInt64] = typeof(UInt64);
        }
        
        public static Type ToType(this TypeCode self)
        {
            return types[(int)self];
        }
    }
}
