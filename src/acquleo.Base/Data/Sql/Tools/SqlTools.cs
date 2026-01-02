#if NETSTANDARD2_1

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace acquleo.Base.Data.Sql.Tools
{
    /// <summary>
    /// Sql tools
    /// </summary>
    public class SqlTools
    {
        /// <summary>
        /// Returns a Datatable from a data reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>

        public static DataTable GetDataTableFromReader(IDataReader reader)
        {
            DataTable schemaTable = reader.GetSchemaTable();
            DataTable data = new DataTable();
            foreach (DataRow row in schemaTable.Rows)
            {                
                string colName = row.Field<string>("ColumnName");
                Type t = row.Field<Type>("DataType");
                data.Columns.Add(colName, t);
            }
            while (reader.Read())
            {
                var newRow = data.Rows.Add();
                foreach (DataColumn col in data.Columns)
                {
                    newRow[col.ColumnName] = reader[col.ColumnName];
                }
            }
            return data;
        }

        /// <summary>
        /// Add parameter to a generic dbcommand
        /// </summary>
        /// <param name="cmd">db command</param>
        /// <param name="type">parameter type</param>
        /// <param name="name">parameter name</param>
        /// <param name="value">parameter value</param>
        public static void AddParameter(IDbCommand cmd, DbType type, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.DbType = type; p.Value = value; p.ParameterName = name;
            cmd.Parameters.Add(p);
        }
    }
}

#endif