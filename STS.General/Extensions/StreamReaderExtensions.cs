using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using STS.General.Buffers;
using System.Linq.Expressions;

namespace STS.General.Extensions
{
    public static class StreamReaderExtensions
    {
        private static Func<StreamReader, int> GetInternalPosition;

        static StreamReaderExtensions()
        {
            GetInternalPosition = CreateInternalPosition();
        }

        private static Func<StreamReader, int> CreateInternalPosition()
        {
            var reader = Expression.Parameter(typeof(StreamReader), "reader");

            var charPos = Expression.PropertyOrField(reader, "charPos");
            var charLen = Expression.PropertyOrField(reader, "charLen");

            var sum = Expression.Add(charPos, charLen);
            var ret = Expression.Label(Expression.Label(typeof(int)), sum);

            // return charPos + charLen;
            Expression<Func<StreamReader, int>> lambda = Expression.Lambda<Func<StreamReader, int>>(ret, reader);

            return lambda.Compile();
        }

        /// <summary>
        /// Returns the true position in the underlying stream.
        /// </summary>
        public static long GetPosition(this StreamReader self)
        {
            int pos = GetInternalPosition(self);

            return self.BaseStream.Position - pos;
        }
    }
}
