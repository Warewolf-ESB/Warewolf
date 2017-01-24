using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Npgsql;
using System;
using System.Data;
using Warewolf.Resource.Errors;
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
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection , "Postgre"));
            }

            return ((NpgsqlConnection)connection).GetSchema(collectionName);
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            DataSet ds = new DataSet(); //conn is opened by dataadapter
            reader.Fill(ds);
            return ds.Tables[0];
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is NpgsqlCommand))
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "PostgreCommand"));

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