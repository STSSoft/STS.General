using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace STS.General.SQL.Extensions
{
    public static class IDbConnectionExtensions
    {
        public static IDbCommand CreateCommand(this IDbConnection conn, string commandText)
        {
            IDbCommand command = conn.CreateCommand();
            command.CommandText = commandText;

            return command;
        }

        public static IDataReader ExecuteQuery(this IDbConnection conn, string query, CommandBehavior behavior)
        {
            IDbCommand command = conn.CreateCommand(query);

            return command.ExecuteReader(behavior);
        }

        public static IDataReader ExecuteQuery(this IDbConnection conn, string query)
        {
            return ExecuteQuery(conn, query, CommandBehavior.Default);
        }

        public static int ExecuteNonQuery(this IDbConnection conn, string query)
        {
            IDbCommand command = conn.CreateCommand(query);

            return command.ExecuteNonQuery();
        }

        public static IEnumerable<IDataRecord> Forward(this IDbConnection conn, string query)
        {
            return conn.ExecuteQuery(query).Forward();
        }

        public static IEnumerable<IDataRecord> Forward(this IDbConnection conn, string queryFormat, params object[] args)
        {
            return conn.Forward(String.Format(queryFormat, args));
        }
    }
}
