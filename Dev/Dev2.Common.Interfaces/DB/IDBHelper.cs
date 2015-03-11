/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Common.Interfaces.DB
{
    public enum enSupportedDBTypes
    {
        MSSQL
    }

    public interface IDBConnectionString
    {
        string Value { get; }
    }

    public interface IDBHelper
    {
        /// <summary>
        ///     Generic wrapper for Connection strings
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        IDBConnectionString CreateConnectionString(IDBConnectionString properties);

        /// <summary>
        ///     Extract Stored Proces and Fuctions
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string ExtractCodedEntities(IDBConnectionString str);

        /// <summary>
        ///     Tickle a stored proc / function
        /// </summary>
        /// <param name="str"></param>
        /// <param name="proc"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string TickleDBLogic(IDBConnectionString str, string proc, string args);

        /// <summary>
        ///     List the databases in the system
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string ListDatabases(IDBConnectionString str);

        /// <summary>
        ///     What type of DB does the helper handle
        /// </summary>
        /// <returns></returns>
        enSupportedDBTypes HandlesType();
    }
}