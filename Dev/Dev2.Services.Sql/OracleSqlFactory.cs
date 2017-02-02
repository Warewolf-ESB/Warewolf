using System;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Oracle.ManagedDataAccess.Client;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    internal class OracleSqlFactory : IDbFactory
    {
        #region Implementation of IDbFactory

        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            if (connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }
            return new OracleConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText)
        {
            return new OracleCommand(commandText, connection as OracleConnection)
            {
                CommandType = commandType,
                CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds
            };
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName)
        {
            return GetOracleServerSchema(connection);
        }

        DataTable GetOracleServerSchema(IDbConnection connection)
        {
            if (!(connection is OracleConnection))
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "Oracle"));

            return ((OracleConnection)connection).GetSchema();
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            DataSet ds = new DataSet(); //conn is opened by dataadapter
            reader.Fill(ds);
            var t = ds.Tables[0];
            return t;
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is OracleCommand))
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "OracleCommand"));
            using (var dataSet = new DataSet())
            {
                using (var adapter = new OracleDataAdapter(command as OracleCommand))
                {
                    adapter.Fill(dataSet);
                }
                return dataSet;
            }
        }

        #endregion
    }
}
