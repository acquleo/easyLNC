#if NETSTANDARD2_1
using System;
using System.Data;

namespace acquleo.Base.Data.Sql
{
    /// <summary>
    /// Implements the ITetDbConnection interface for MySql
    /// </summary>
    public class SqlServerTetDbConnection : ITetDbConnection
    {

        #region Public Member

        #region Cosntructor
        /// <summary>
        /// Costruttore di definizione
        /// </summary>
        /// <param name="mysqlConnectionString">Stringa di connessione al Db</param>
        public SqlServerTetDbConnection(string mysqlConnectionString)
        {
            // Per il momento viene definita così, appena avremo il codice che ritorna
            // il server in controllo il parametro in ingresso sarà quello
            this.ConnectionString = mysqlConnectionString;
        }
        #endregion

        #region Public Methods

        #endregion

        #region IEntitiesConfig Member
        /// <summary>
        /// Returns the current connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Returns the connection to the database
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            return new System.Data.SqlClient.SqlConnection(this.ConnectionString);
        }
        /// <summary>
        /// Returns an escaped text 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string GetEscapedKeyword(string text)
        {
            return "\"" + text + "\"";
        }
        /// <summary>
        /// Returns the query to obtain the last inserted identity
        /// </summary>
        /// <returns></returns>
        public string GetLastIdentityQuery()
        {
            return "SELECT SCOPE_IDENTITY();";
        }

#endregion
#endregion
    }
}

#endif