#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Data;
using System.Data.SQLite;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    public class SqliteServerFactory : IDbFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            var connectionStr = connectionString;
            VerifyArgument.IsNotNull("connectionString", connectionStr);
            if (connectionStr.CanBeDecrypted())
            {
                connectionStr = DpapiWrapper.Decrypt(connectionStr);
            }
            return new SQLiteConnection(connectionStr);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText, int? commandTimeout)
        {
            var command = new SQLiteCommand(commandText, connection as SQLiteConnection)
            {
                CommandType = commandType,
            };
            if (commandTimeout != null)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName) => GetSqliteServerSchema(connection);

        static DataTable GetSqliteServerSchema(IDbConnection connection)
        {
            if (!(connection is SQLiteConnection))
            {
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "SQLite"));
            }

            return ((SQLiteConnection)connection).GetSchema();
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            var ds = new DataSet();
            reader.Fill(ds);
            return ds.Tables[0];
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is SQLiteCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            var dataSet = new DataSet();
            using (var adapter = new SQLiteDataAdapter(command as SQLiteCommand))
            {
                adapter.Fill(dataSet);
            }
            return dataSet;
        }
        public int ExecuteNonQuery(IDbCommand command)
        {
            if (!(command is SQLiteCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = command.ExecuteNonQuery();
            return retValue;
        }

        public int ExecuteScalar(IDbCommand command)
        {
            if (!(command is SQLiteCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = Convert.ToInt32(command.ExecuteScalar());
            return retValue;
        }
    }
}
