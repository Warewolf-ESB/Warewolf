/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    [ExcludeFromCodeCoverage]
    internal class DbFactory : IDbFactory
    {
        SqlConnection _sqlConnection;

        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            if(connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }
            _sqlConnection = new SqlConnection(connectionString);
            _sqlConnection.FireInfoMessageEventOnUserErrors = true;
            _sqlConnection.StatisticsEnabled = true;
            _sqlConnection.InfoMessage += (sender, args) =>
            {
                Dev2Logger.Debug("Sql Server:" + args.Message + " Source:" + args.Source);
                foreach (SqlError error in args.Errors)
                {
                    var errorMessages = new StringBuilder();
                    errorMessages.Append("Index #" + error.Number + Environment.NewLine +
                                        "Message: " + error.Message + Environment.NewLine +
                                        "LineNumber: " + error.LineNumber + Environment.NewLine +
                                        "Source: " + error.Source + Environment.NewLine +
                                        "Procedure: " + error.Procedure + Environment.NewLine);

                    Dev2Logger.Error("Sql Error:" + errorMessages.ToString());
                }
                
            };
            return _sqlConnection;
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText)
        {
            return new SqlCommand(commandText, connection as SqlConnection)
            {
                CommandType = commandType,
                CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds,
            };
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName)
        {
            return GetSqlServerSchema(connection, collectionName);
        }

        static DataTable GetSqlServerSchema(IDbConnection connection, string collectionName)
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
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "Sql"));
            }
        }

        public DataTable CreateTable(IDataReader reader, LoadOption overwriteChanges)
        {
            var table = new DataTable();
            table.Load(reader, LoadOption.OverwriteChanges);
            var retrieveStatistics = _sqlConnection.RetrieveStatistics();
            foreach (DictionaryEntry retrieveStatistic in retrieveStatistics)
            {
                Dev2Logger.Debug("Sql Stat:"+retrieveStatistic.Key+": "+retrieveStatistic.Value);
            }
            return table;
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is SqlCommand))
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
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
