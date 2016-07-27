/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
        MSSQL,
        MySQL,
        Oracle,
        PostgreSQL,
        ODBC
    }

    public interface IDBConnectionString
    {
        string Value { get; }
    }

    public interface IDBHelper
    {
        /// <summary>
        ///     What type of DB does the helper handle
        /// </summary>
        /// <returns></returns>
        enSupportedDBTypes HandlesType();
    }
}