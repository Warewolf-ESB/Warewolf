using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Oracle.ManagedDataAccess.Client;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    [ExcludeFromCodeCoverage]
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

        public DataTable CreateTable(IDataReader reader, LoadOption overwriteChanges)
        {
            var table = new DataTable();
            table.Load(reader, LoadOption.OverwriteChanges);
            return table;
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
