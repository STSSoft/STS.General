using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using STS.General.Extensions;

namespace STS.General.SQL
{
    /// <summary>
    /// <para>Executes multi row insert queries with prepare statements, matches SQL-92 standard:</para>
    /// <para>INSERT|REPLACE INTO TableName (colA, colB,...)    </para>
    /// <para>                VALUES (value1a, value1b, ...),   </para>
    /// <para>                       (value2a, value2b, ...),   </para>
    /// <para>                       (value3a, value3b, ...),   </para>
    /// <para>                          ...                     </para>
    /// <para>                          ...                     </para>
    /// <para>                       (valueNa, valueNb, ...);   </para>
    /// </summary>
    public class SQLMultiInsert
    {
        public IDbConnection Connection { get; private set; }
        private List<Field> list = new List<Field>();
        private IDbCommand cmd;

        /// <summary>
        /// insert | insert ignore | replace | update or insert | ... - depends from the SQL dialect. The default value is 'insert'. 
        /// </summary>
        public string InsertCommand { get; set; }
        public string TableName { get; private set; }
        public int InsertsPerQuery { get; private set; }
        public int RowCount { get; private set; }

        /// <param name="connection">The connection object. Must be valid and opened connection.</param>
        /// <param name="tableName">Non-empty valid table name string.</param>
        /// <param name="insertsPerQuery">Number of inserts that will executed with one query. If your database does not support multi insert, use 1 for value.</param>
        public SQLMultiInsert(IDbConnection connection, string tableName, int insertsPerQuery)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");
            if (tableName == null)
                throw new ArgumentNullException("tableName");
            if (insertsPerQuery <= 0)
                throw new ArgumentOutOfRangeException("insertsPerQuery");

            Connection = connection;
            TableName = tableName;
            InsertsPerQuery = insertsPerQuery;
            InsertCommand = "insert";
        }

        private IDbCommand Prepare(string tableName, int rowCount)
        {
            IDbCommand cmd = Connection.CreateCommand();

            string[] fields = list.Select(x => x.Name).ToArray();
            string[] rows = new string[rowCount];
            string[] values = new string[list.Count];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    string paramName = String.Format("@{0}{1}", fields[j], i);
                    values[j] = paramName;

                    IDbDataParameter param = list[j].GetParameter(cmd, paramName);
                    cmd.Parameters.Add(param);
                }

                rows[i] = "(" + String.Join(",", values) + ")";
            }

            string allFields = String.Join(",", fields);
            string allRows = String.Join(",", rows);

            string command = String.Format("{0} into {1} ({2}) values {3};", InsertCommand, tableName, allFields, allRows);

            cmd.CommandText = command;
            cmd.Prepare();

            return cmd;
        }

        public void AddField(string name, DbType type, int size)
        {
            if (Connection is OleDbConnection)
            {
                switch (type)
                {
                    //workaround: http://www.pcreview.co.uk/forums/thread-3789409.php
                    case DbType.DateTime: list.Add(new Field(true, name, (int)OleDbType.Date, size)); return;
                }
            }

            list.Add(new Field(false, name, (int)type, size));
        }

        public void Prepare()
        {
            cmd = Prepare(TableName, InsertsPerQuery);
            RowCount = 0;
        }

        public void Close()
        {
            if (cmd != null)
                cmd.Dispose();
        }

        public void Insert(params object[] fieldValues)
        {
            if (fieldValues.Length != list.Count)
                throw new ArgumentOutOfRangeException("fieldValues.Length");

            if (RowCount == InsertsPerQuery) //is full
                Flush();

            int idx = fieldValues.Length * RowCount;

            for (int i = 0; i < fieldValues.Length; i++)
            {
                IDbDataParameter param = (IDbDataParameter)cmd.Parameters[idx++];
                param.Value = fieldValues[i];
            }

            RowCount++;
        }

        public int Flush()
        {
            if (RowCount == 0)
                return -1;

            if (RowCount < InsertsPerQuery) //new prepare for the rest values
            {
                IDbCommand cmd2 = Prepare(TableName, RowCount);

                for (int i = 0; i < cmd2.Parameters.Count; i++)
                {
                    IDbDataParameter param = (IDbDataParameter)cmd.Parameters[i];
                    IDbDataParameter param2 = (IDbDataParameter)cmd2.Parameters[i];
                    param2.Value = param.Value;
                }

                RowCount = 0;
                return cmd2.ExecuteNonQuery();
            }

            RowCount = 0;
            return cmd.ExecuteNonQuery();
        }

        private class Field
        {
            public bool IsOleDb { get; private set; }
            public string Name { get; private set; }
            public int Type { get; private set; }
            public int Size { get; private set; }

            public Field(bool isOleDb, string name, int type, int size)
            {
                this.IsOleDb = isOleDb;
                this.Name = name;
                this.Type = type;
                this.Size = size;
            }

            public IDbDataParameter GetParameter(IDbCommand command, string parameterName)
            {
                if (!IsOleDb)
                    return command.CreateParameter(parameterName, (DbType)Type, Size);
                else
                    return new OleDbParameter(parameterName, (OleDbType)Type, Size);
            }

            public override string ToString()
            {
                return String.Format("{0} {1} ({2})", Name, IsOleDb ? ((OleDbType)Type).ToString() : ((DbType)Type).ToString(), Size);
            }
        }
    }
}
