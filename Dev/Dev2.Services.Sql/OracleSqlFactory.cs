#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using System;
using System.Data;
using Dev2.Common.Interfaces.Services.Sql;
using Oracle.ManagedDataAccess.Client;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    class OracleSqlFactory : IDbFactory
    {
        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            if (connectionString.CanBeDecrypted())
            {
                connectionString = SecurityEncryption.Decrypt(connectionString);
            }
            return new OracleConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText, int? commandTimeout)
        {
            var command = new OracleCommand(commandText, connection as OracleConnection)
            {
                CommandType = commandType,                
            };
            if (commandTimeout != null)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName) => GetOracleServerSchema(connection);

        DataTable GetOracleServerSchema(IDbConnection connection)
        {
            if (!(connection is OracleConnection))
            {
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "Oracle"));
            }

            return ((OracleConnection)connection).GetSchema();
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            var ds = new DataSet(); //conn is opened by dataadapter
            reader.Fill(ds);
            var t = ds.Tables[0];
            return t;
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is OracleCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "OracleCommand"));
            }

            var dataSet = new DataSet();
            using (var adapter = new OracleDataAdapter(command as OracleCommand))
            {
                adapter.Fill(dataSet);
            }
            return dataSet;
        }
		public int ExecuteNonQuery(IDbCommand command)
		{
			if (!(command is OracleCommand SqlCommand))
			{
				throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
			}

			int retValue = 0;
			retValue = command.ExecuteNonQuery();
			return retValue;
		}

		public int ExecuteScalar(IDbCommand command)
		{
			if (!(command is OracleCommand))
			{
				throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
			}

			int retValue = 0;
			retValue = Convert.ToInt32(command.ExecuteScalar());
			return retValue;
		}
		#endregion
	}
}
