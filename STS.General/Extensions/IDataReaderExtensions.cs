using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace STS.General.Extensions
{
    public static class IDataReaderExtensions
    {
        public static IEnumerable<IDataRecord> Forward(this IDataReader reader)
        {
            try
            {
                while (reader.Read())
                    yield return reader;
            }
            finally
            {
                reader.Close();
            }
        }
    }
}
