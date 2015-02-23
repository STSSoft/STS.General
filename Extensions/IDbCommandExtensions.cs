using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace STS.General.Extensions
{
    public static class IDbCommandExtensions
    {
        public static IDbDataParameter CreateParameter(this IDbCommand command, string name, DbType type, int size)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Size = size;

            return parameter;
        }

        public static IDbDataParameter AddParameter(this IDbCommand command, string name, DbType type, int size)
        {
            IDbDataParameter parameter = command.CreateParameter(name, type, size);
            command.Parameters.Add(parameter);

            return parameter;
        }
    }
}
