#pragma warning disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using MySql.Data.MySqlClient;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Sql
{
    public sealed class MySqlServer : IDbServer
    {
        readonly IDbFactory _factory;
        IDbCommand _command;
        MySqlConnection _connection;
        IDbTransaction _transaction;

        public bool IsConnected => _connection != null && _connection.State == ConnectionState.Open;

        public string ConnectionString => _connection?.ConnectionString;

        public int? CommandTimeout { get; set; }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, string dbName)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            var proceduresDataTable = GetSchema(_connection);

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var fullProcedureName = row["Name"].ToString();
                if (row["Db"].ToString() == dbName)
                {
                    using (
                        IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, dbName, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string dbName, string fullProcedureName, IDbCommand command)
        {
            try
            {

                var parameters = GetProcedureParameters(command, dbName, fullProcedureName, out List<IDbDataParameter> outParameters);
                var helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

                procedureProcessor(command, parameters, outParameters, helpText, fullProcedureName);


            }
            catch (Exception)
            {
                if (!continueOnProcessorException)
                {
                    throw;
                }
            }
        }

        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            IDbCommand command = _connection.CreateCommand();
            command.Connection = _connection;
            command.Transaction = _transaction;
            return command;
        }

        public void BeginTransaction()
        {
            if (IsConnected)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        #region FetchDatabases

        public List<string> FetchDatabases()
        {
            VerifyConnection();
            MySqlDataReader reader = null;
            var result = new List<string>();
            var cmd = new MySqlCommand("SHOW DATABASES", _connection);
            try
            {
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }

            finally
            {
                reader?.Close();
            }

            return result;
        }

        #endregion

        #region FetchDataTable

        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
        }
        public DataSet FetchDataSet(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return _factory.FetchDataSet(command);
        }
        public DataTable FetchDataTable(IDbDataParameter[] parameters, IEnumerable<IDbDataParameter> outparameters)
        {
            var command = _connection.CreateCommand();
            command.CommandText = _command.CommandText;
            command.CommandType = _command.CommandType;
            command.CommandTimeout = _command.CommandTimeout;

            VerifyConnection();
            AddParameters(command, parameters);
            foreach (var par in outparameters)
            {
                command.Parameters.Add(par);
            }
            return FetchDataTable(command);
        }
        public int ExecuteNonQuery(IDbCommand command)
        {
            if (!(command is MySqlCommand SqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = command.ExecuteNonQuery();
            return retValue;
        }

        public int ExecuteScalar(IDbCommand command)
        {
            if (!(command is MySqlCommand))
            {
                throw new Exception(string.Format(ErrorResource.InvalidCommand, "DBCommand"));
            }

            int retValue = 0;
            retValue = Convert.ToInt32(command.ExecuteScalar());
            return retValue;
        }
        #endregion

        #region FetchStoredProcedures

        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");

        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
            bool continueOnProcessorException, string dbName)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            var proceduresDataTable = GetSchema(_connection);

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var fullProcedureName = row["Name"].ToString();
                if (row["Db"].ToString() == dbName)
                {
                    using (
                        IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, dbName, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string dbName, string fullProcedureName, IDbCommand command)
        {
            try
            {
                var parameters = GetProcedureParameters(command, dbName, fullProcedureName, out List<IDbDataParameter> isOut);
                var helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);
                procedureProcessor(command, parameters, helpText, fullProcedureName);
            }
            catch (Exception)
            {
                if (!continueOnProcessorException)
                {
                    throw;
                }
            }
        }

        string FetchHelpTextContinueOnException(string fullProcedureName, IDbConnection con)
        {
            string helpText;

            try
            {
                helpText = GetHelpText(con, fullProcedureName);
            }
            catch (Exception e)
            {
                helpText = "Could not fetch because of : " + e.Message;
            }

            return helpText;
        }

        #endregion

        #region VerifyConnection

        void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
        }

        #endregion

        #region Connect

        public void Connect(string connectionString)
        {
            _connection = (MySqlConnection)_factory.CreateConnection(connectionString);
            _connection.Open();
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = (MySqlConnection)_factory.CreateConnection(connectionString);

            VerifyArgument.IsNotNull("commandText", commandText);
            if (commandText.ToLower().StartsWith("select "))
            {
                commandType = CommandType.Text;
            }

            _command = _factory.CreateCommand(_connection, commandType, commandText, CommandTimeout);

            _connection.Open();
            return true;
        }

        #endregion

        static T ExecuteReader<T>(IDbCommand command, Func<IDataAdapter, T> handler)
        {
            try
            {
                var da = new MySqlDataAdapter(command as MySqlCommand);
                using (da)
                {
                    return handler(da);
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new MySqlDataAdapter());
                }
                throw;
            }
        }


        public static void AddParameters(IDbCommand command, ICollection<IDbDataParameter> parameters)
        {
            command.Parameters.Clear();
            if (parameters != null && parameters.Count > 0)
            {
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }

        DataTable GetSchema(IDbConnection connection)
        {
            var CommandText = GlobalConstants.SchemaQueryMySql;
            using (IDbCommand command = _factory.CreateCommand(connection, CommandType.Text, CommandText, CommandTimeout))
            {
                return FetchDataTable(command);
            }
        }

        /// <summary>
        /// Gets the help text (definition) for a given stored procedure or function
        /// </summary>
        /// <param name="connection">Connection</param>
        /// <param name="objectName">ObjectName</param>
        /// <returns>Definition of the Database Object</returns>
        string GetHelpText(IDbConnection connection, string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return string.Empty;

            // Parse database and object name efficiently
            var (databaseName, procName) = ParseObjectName(objectName);

            // Try to get the routine definition from INFORMATION_SCHEMA
            const string query = @"
SELECT ROUTINE_DEFINITION
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_SCHEMA = @database 
  AND ROUTINE_NAME = @procname
LIMIT 1";

            using (var command = _factory.CreateCommand(connection, CommandType.Text, query, CommandTimeout))
            {
                AddParameter(command, "@database", databaseName);
                AddParameter(command, "@procname", procName);

                return ExecuteReader(command, adapter =>
                {
                    using (var ds = new DataSet())
                    {
                        adapter.Fill(ds);

                        // Check if we have valid results
                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            var value = ds.Tables[0].Rows[0]["ROUTINE_DEFINITION"];
                            return value != DBNull.Value ? value.ToString() : string.Empty;
                        }

                        return string.Empty;
                    }
                });
            }
        }

        private static (string database, string name) ParseObjectName(string objectName)
        {
            var dotIndex = objectName.IndexOf('.');

            return dotIndex > 0
                ? (objectName.Substring(0, dotIndex), objectName.Substring(dotIndex + 1))
                : (string.Empty, objectName);
        }

        /// <summary>
        /// Extracted helper method for creating and adding parameters to database commands.
        /// Reduces code redundancy when building parameterized queries.
        /// </summary>
        /// <param name="command">The database command to add the parameter to</param>
        /// <param name="name">The parameter name (e.g., "@database", "@procname")</param>
        /// <param name="value">The parameter value to be used in the query</param>
        /// <remarks>
        /// This method prevents SQL injection by properly parameterizing query values
        /// instead of using string concatenation or formatting.
        /// </remarks>
        private void AddParameter(IDbCommand command, string name, string value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        public List<MySqlParameter> GetProcedureOutParams(string fullProcedureName, string dbName)
        {
            using (IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                GetProcedureParameters(command, dbName, fullProcedureName, out List<IDbDataParameter> isOut);
                return isOut.Select(a => a as MySqlParameter).ToList();
            }
        }

        public List<IDbDataParameter> GetProcedureParameters(IDbCommand command, string dbName, string procedureName, out List<IDbDataParameter> outParams)
        {
            outParams = new List<IDbDataParameter>();
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();
            command.CommandType = CommandType.Text;
            command.CommandText =
                     string.Format(
     "SELECT PARAMETER_NAME, PARAMETER_MODE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_SCHEMA='{0}' AND SPECIFIC_NAME='{1}' ORDER BY ORDINAL_POSITION",
         dbName, procedureName);
            var dataTable = FetchDataTable(command);
           foreach (DataRow row in dataTable.Rows)
    {
            GetProcInputsFromRow(command, outParams, parameters, row);
     }
      command.CommandText = originalCommandText;
         return parameters;
         }

         private static void GetProcInputsFromRow(IDbCommand command, List<IDbDataParameter> outParams, List<IDbDataParameter> parameters, DataRow row)
         {
         var parameterName = row["PARAMETER_NAME"]?.ToString();
          var parameterMode = row["PARAMETER_MODE"]?.ToString();
 var dataType = row["DATA_TYPE"]?.ToString();

    if (string.IsNullOrEmpty(parameterName) || string.IsNullOrEmpty(dataType))
  {
       return;
        }

      var direction = ParameterDirection.Input;
             var isOut = false;

        if (parameterMode == "OUT")
        {
        direction = ParameterDirection.Output;
            isOut = true;
    }
    else if (parameterMode == "INOUT")
    {
           direction = ParameterDirection.InputOutput;
             isOut = false;
           }

   // Map MySQL data types to MySqlDbType
 if (!Enum.TryParse(dataType, true, out MySqlDbType sqlType))
           {
 // Handle common type mappings that don't match enum names exactly
            sqlType = MapDataTypeToMySqlDbType(dataType);
     }

   var sqlParameter = new MySqlParameter(parameterName, sqlType) { Direction = direction };

// Set size for varchar and other sized types
             if (row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value && row["CHARACTER_MAXIMUM_LENGTH"] != null)
     {
                 var maxLength = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]);
           if (maxLength > 0)
 {
                sqlParameter.Size = maxLength;
       }
         }

   command.Parameters.Add(sqlParameter);

 if (!isOut)
       {
      parameters.Add(sqlParameter);
      }
   else
       {
    sqlParameter.Value = "@a";
      outParams.Add(sqlParameter);
   }
       }

   private static MySqlDbType MapDataTypeToMySqlDbType(string dataType)
     {
           switch (dataType.ToLower())
      {
  case "varchar":
 case "char":
   return MySqlDbType.VarChar;
          case "text":
             return MySqlDbType.Text;
     case "int":
       case "integer":
   return MySqlDbType.Int32;
     case "bigint":
        return MySqlDbType.Int64;
    case "smallint":
      return MySqlDbType.Int16;
case "tinyint":
          return MySqlDbType.Byte;
   case "decimal":
      case "numeric":
          return MySqlDbType.Decimal;
 case "float":
   return MySqlDbType.Float;
             case "double":
          return MySqlDbType.Double;
    case "datetime":
     return MySqlDbType.DateTime;
   case "timestamp":
  return MySqlDbType.Timestamp;
       case "date":
   return MySqlDbType.Date;
  case "time":
       return MySqlDbType.Time;
 case "blob":
  return MySqlDbType.Blob;
         case "longblob":
         return MySqlDbType.LongBlob;
      case "mediumblob":
         return MySqlDbType.MediumBlob;
          case "tinyblob":
   return MySqlDbType.TinyBlob;
   case "longtext":
     return MySqlDbType.LongText;
            case "mediumtext":
   return MySqlDbType.MediumText;
    case "tinytext":
        return MySqlDbType.TinyText;
 case "bit":
   return MySqlDbType.Bit;
 case "binary":
      case "varbinary":
    return MySqlDbType.VarBinary;
       case "enum":
   return MySqlDbType.Enum;
 case "set":
         return MySqlDbType.Set;
     case "json":
         return MySqlDbType.JSON;
  default:
     return MySqlDbType.VarChar; // Default fallback
     }
    }

        #region IDisposable

        bool _disposed;

        public MySqlServer()
        {
            _factory = new MySqlDbFactory();
        }

        public MySqlServer(IDbFactory dbFactory)
        {
            _factory = dbFactory;
        }

        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        ~MySqlServer()
        {
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _transaction?.Dispose();

                    _command?.Dispose();
                    DisposeConnection();
                }

                // Call the appropriate methods to clean up 
                // unmanaged resources here. 
                // If disposing is false, 
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        private void DisposeConnection()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }

        #endregion
    }
}