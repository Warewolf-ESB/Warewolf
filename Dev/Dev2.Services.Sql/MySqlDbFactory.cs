#pragma warning disable
using System;
using System.Data;
using System.Data.SqlClient;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using MySql.Data.MySqlClient;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    class MySqlDbFactory : IDbFactory
    {
        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            if (connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }
            return new MySqlConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText, int? commandTimeout)
        {
            var command = new MySqlCommand(commandText, connection as MySqlConnection)
            {
                CommandType = commandType,
            };
            if (commandTimeout != null)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName) => GetMySqlServerSchema(connection);

        DataTable GetMySqlServerSchema(IDbConnection connection)
        {
            if (!(connection is MySqlConnection))
            {
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "MySql"));
            }
            return ((MySqlConnection)connection).GetSchema();
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            var ds = new DataSet();
            reader.Fill(ds);
            return ds.Tables[0];
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is MySqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            var dataSet = new DataSet();
            using (var adapter = new MySqlDataAdapter(command as MySqlCommand))
            {
                adapter.Fill(dataSet);
            }
            return dataSet;
        }
        public int ExecuteNonQuery(IDbCommand command)
        {
            if (!(command is SqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = command.ExecuteNonQuery();
            return retValue;
        }

        public int ExecuteScalar(IDbCommand command)
        {
            if (!(command is SqlCommand))
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