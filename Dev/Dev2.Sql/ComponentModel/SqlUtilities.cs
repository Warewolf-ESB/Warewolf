/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Data;
using System.Data.SqlClient;

namespace Warewolf.ComponentModel
{
    /// <summary>
    ///     Groups some SQL Server utility methods.
    /// </summary>
    public static class SqlUtilities
    {
        /// <summary>
        ///     The CLR connection string: <code>context connection=true</code>
        /// </summary>
        public const string ClrConnectionString = "context connection=true";

        #region FetchDataTable

        /// <summary>
        ///     Executes the given <code>SqlCommand</code> and returns a
        ///     <code>DataTable</code> for the result set returned by the
        ///     <code>SqlCommand</code>.
        /// </summary>
        /// <returns>The result of the command in a <code>DataTable</code>.</returns>
        public static DataTable FetchDataTable(this SqlCommand command)
        {
            using (SqlDataReader reader = command.ExecuteReader((CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo)))
            {
                var table = new DataTable();
                table.Load(reader, LoadOption.OverwriteChanges);
                return table;
            }
        }

        #endregion
    }
}
