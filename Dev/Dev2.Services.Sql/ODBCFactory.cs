#pragma warning disable
﻿using System;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using System.Data.Odbc;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    class ODBCFactory : IDbFactory
    {
        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            if (connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }
            return new OdbcConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText, int? commandTimeout)
        {
            var command = new OdbcCommand(commandText, connection as OdbcConnection)
            {
                CommandType = commandType,
            };
            if (commandTimeout != null)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }
        public DataTable GetSchema(IDbConnection connection, string collectionName) => GetOdbcServerSchema(connection);

        DataTable GetOdbcServerSchema(IDbConnection connection)
        {
            if (!(connection is OdbcConnection))
            {
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "Oracle"));
            }

            return ((OdbcConnection)connection).GetSchema();
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            var ds = new DataSet(); //conn is opened by dataadapter
            reader.Fill(ds);
            return ds.Tables[0];
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is OdbcCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "OracleCommand"));
            }

            var dataSet = new DataSet();
            using (var adapter = new OdbcDataAdapter(command as OdbcCommand))
            {
                adapter.Fill(dataSet);
            }
            return dataSet;
        }
        public int ExecuteNonQuery(IDbCommand command)
        {
            if (!(command is OdbcCommand SqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = command.ExecuteNonQuery();
            return retValue;
        }

        public int ExecuteScalar(IDbCommand command)
        {
            if (!(command is OdbcCommand))
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
