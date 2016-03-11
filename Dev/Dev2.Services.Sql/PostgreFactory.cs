using System;
using System.Data;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Npgsql;
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    public class PostgreFactory : IDbFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);

            if (connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }

            return new NpgsqlConnection(connectionString);
        }

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText)
        {
            return new NpgsqlCommand(commandText, connection as NpgsqlConnection)
            {
                CommandType = commandType,
                CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds
            };
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName)
        {
            if (!(connection is NpgsqlConnection))
            {
                throw new Exception("Invalid Postgre connection");
            }

            return ((NpgsqlConnection)connection).GetSchema(collectionName);
        }

        public DataTable CreateTable(IDataReader reader, LoadOption overwriteChanges)
        {
            var table = new DataTable();
            table.Load(reader, LoadOption.OverwriteChanges);

            return table;
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is NpgsqlCommand))
                throw new Exception("Not a valid PostgreCommand");

            using (var dataset = new DataSet())
            {
                using (var adapter = new NpgsqlDataAdapter(command as NpgsqlCommand))
                {
                    adapter.Fill(dataset);
                }

                return dataset;
            }
        }
    }
}
