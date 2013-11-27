namespace Dev2.Common.DB
{
    public interface IDBHelper
    {
        /// <summary>
        /// Generic wrapper for Connection strings
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        DBConnectionString CreateConnectionString(DBConnectionProperties properties);

        /// <summary>
        /// Extract Stored Proces and Fuctions
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string ExtractCodedEntities(DBConnectionString str);

        /// <summary>
        /// Tickle a stored proc / function
        /// </summary>
        /// <param name="str"></param>
        /// <param name="proc"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string TickleDBLogic(DBConnectionString str, string proc, string args);

        /// <summary>
        /// List the databases in the system
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string ListDatabases(DBConnectionString str);

        /// <summary>
        /// What type of DB does the helper handle
        /// </summary>
        /// <returns></returns>
        enSupportedDBTypes HandlesType(); 
    }
}
