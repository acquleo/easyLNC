#if NETSTANDARD2_1

using System.Data;

namespace acquleo.Base.Data.Sql
{
    /// <summary>
    /// Generid database connection interface
    /// </summary>
    public interface ITetDbConnection
    {
        /// <summary>
        /// Returns or sets the current connection string
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Returns a new connection to the database
        /// </summary>
        /// <returns></returns>
        IDbConnection GetConnection();

        /// <summary>
        /// Returns an escaped text 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string GetEscapedKeyword(string text);

        /// <summary>
        /// Returns the query to obtain the last inserted identity
        /// </summary>
        /// <returns></returns>
        string GetLastIdentityQuery();
    }
}

#endif