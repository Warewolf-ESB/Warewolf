#pragma warning disable
﻿using Dev2.Common;
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

        public IDbCommand CreateCommand(IDbConnection connection, CommandType commandType, string commandText, int? commandTimeout)
        {
            var command = new NpgsqlCommand(commandText, connection as NpgsqlConnection);
            
            // PostgreSQL functions that return data must be called with SELECT, not CALL
            // Only use StoredProcedure CommandType for actual procedures (void return)
            if (commandType == CommandType.StoredProcedure)
            {
                // Check if this is a function by querying return type
                var returnType = GetFunctionReturnType(connection, commandText);
                
                if (!string.IsNullOrEmpty(returnType) && !returnType.Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    // It's a function that returns data - use SELECT syntax
                    command.CommandType = CommandType.Text;
                    // The actual SELECT statement will be built when parameters are added
                }
                else
                {
                    // It's a procedure or void function - use CALL syntax
                    command.CommandType = CommandType.StoredProcedure;
                }
            }
            else
            {
                command.CommandType = commandType;
            }
            
            if (commandTimeout != null)
            {
                command.CommandTimeout = commandTimeout.Value;
            }
            return command;
        }

        private string GetFunctionReturnType(IDbConnection connection, string functionName)
        {
            try
            {
                var query = $@"SELECT routines.data_type AS proc_return_type 
                               FROM information_schema.routines
                               WHERE routines.specific_schema='public' 
                               AND routine_name = '{functionName.ToLower()}';";
                
                using (var cmd = new NpgsqlCommand(query, connection as NpgsqlConnection))
                {
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "void";
                }
            }
            catch
            {
                return "void";
            }
        }

        public DataTable GetSchema(IDbConnection connection, string collectionName)
        {
            if (!(connection is NpgsqlConnection))
            {
                throw new Exception(string.Format(ErrorResource.InvalidSqlConnection, "Postgre"));
            }

            return ((NpgsqlConnection)connection).GetSchema(collectionName);
        }

        public DataTable CreateTable(IDataAdapter reader, LoadOption overwriteChanges)
        {
            var ds = new DataSet(); //conn is opened by dataadapter
            try
            {
                reader.Fill(ds);
            }
            catch (Exception ex)
            {
                return new DataTable();
            }
            return ds.Tables.Count > 0 ? ds.Tables[0] : new DataTable();
        }

        public DataSet FetchDataSet(IDbCommand command)
        {
            if (!(command is NpgsqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "PostgreCommand"));
            }

            var dataset = new DataSet();
            using (var adapter = new NpgsqlDataAdapter(command as NpgsqlCommand))
            {
                adapter.Fill(dataset);
            }

            return dataset;
        }

        public int ExecuteNonQuery(IDbCommand command)
        {
            if (!(command is NpgsqlCommand SqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = command.ExecuteNonQuery();
            return retValue;
        }

        public int ExecuteScalar(IDbCommand command)
        {
            if (!(command is NpgsqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = Convert.ToInt32(command.ExecuteScalar());
            return retValue;
        }
    }
}