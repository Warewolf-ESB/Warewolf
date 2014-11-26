/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;

namespace Dev2.Services.Sql
{
    [ExcludeFromCodeCoverage]
    internal class DbFactory : IDbFactory
    {
        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);

            return new SqlConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText)
        {
            return new SqlCommand(commandText, connection as SqlConnection)
            {
                CommandType = commandType,
                CommandTimeout = (int) GlobalConstants.TransactionTimeout.TotalSeconds,
            };
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName)
        {
            var sqlConnection = connection as SqlConnection;
            if (sqlConnection != null)
            {
                return sqlConnection.GetSchema(collectionName);
            }
                // ReSharper disable RedundantIfElseBlock
            else
                // ReSharper restore RedundantIfElseBlock
            {
                throw new Exception("Invalid SqlConnection");
            }
        }

        public DataTable CreateTable(IDataReader reader, LoadOption overwriteChanges)
        {
            var table = new DataTable();
            table.Load(reader, LoadOption.OverwriteChanges);
            return table;
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is SqlCommand))
                throw new Exception("Invalid DBCommand expected.");
            using (var dataSet = new DataSet())
            {
                using (var adapter = new SqlDataAdapter(command as SqlCommand))
                {
                    adapter.Fill(dataSet);
                }
                return dataSet;
            }
        }

        #endregion
    }
}