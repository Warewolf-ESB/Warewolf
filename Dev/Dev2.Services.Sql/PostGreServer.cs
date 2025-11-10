#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Sql
{
    public class PostgreServer : IDbServer
    {
        readonly IDbFactory _factory;
        IDbCommand _command;
        IDbConnection _connection;
        IDbTransaction _transaction;

        public bool IsConnected => _connection != null && _connection.State == ConnectionState.Open;

        public int? CommandTimeout { get; set; }

        public string ConnectionString => _connection == null ? null : _connection.ConnectionString;

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
                        var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string fullProcedureName, IDbCommand command)
        {
            try
            {

                var parameters = GetProcedureParameters(command, fullProcedureName, out List<IDbDataParameter> outParameters);
                var helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

                procedureProcessor?.Invoke(command, parameters, outParameters, helpText, fullProcedureName);
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
            var command = _connection.CreateCommand();
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
            NpgsqlDataReader reader = null;

            var result = new List<string>();
            var cmd = new NpgsqlCommand("select datname from pg_database",  (NpgsqlConnection)_connection);

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
                
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return result;
        }

        #endregion FetchDatabases

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
		public int ExecuteNonQuery(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return _factory.ExecuteNonQuery(command);
		}

		public int ExecuteScalar(IDbCommand command)
		{
			VerifyArgument.IsNotNull("command", command);

			return _factory.ExecuteScalar(command);
		}
		public DataTable FetchDataTable(IDbDataParameter[] parameters, IEnumerable<IDbDataParameter> outparameters)
        {
            VerifyConnection();
            AddParameters(_command, parameters);
            foreach (var par in outparameters)
            {
                _command.Parameters.Add(par);
            }
            return FetchDataTable(_command);
        }

        #endregion FetchDataTable

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
                        var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName, CommandTimeout))
                    {
                        TryProcessProcedure(procedureProcessor, continueOnProcessorException, fullProcedureName, command);
                    }
                }
            }
        }

        private void TryProcessProcedure(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, bool continueOnProcessorException, string fullProcedureName, IDbCommand command)
        {
            try
            {
                var parameters = GetProcedureParameters(command, fullProcedureName, out List<IDbDataParameter> isOut);
                var helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

                procedureProcessor?.Invoke(command, parameters, helpText, fullProcedureName);
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

        #endregion FetchStoredProcedures

        #region VerifyConnection

        void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
        }

        #endregion VerifyConnection

        #region Connect

        public void Connect(string connectionString)
        {
            _connection = (NpgsqlConnection)_factory.CreateConnection(connectionString);
            _connection.Open();
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = (NpgsqlConnection)_factory.CreateConnection(connectionString);

            VerifyArgument.IsNotNull("commandText", commandText);
            if (commandText.ToLower().StartsWith("select "))
            {
                commandType = CommandType.Text;
            }

            _connection.Open();

            _command = _factory.CreateCommand(_connection, commandType, commandText, CommandTimeout);

            return true;
        }

        #endregion Connect

        static T ExecuteReader<T>(IDbCommand command, Func<IDataAdapter, T> handler)
        {
            try
            {
                var adapter = new NpgsqlDataAdapter(command as NpgsqlCommand);
                using (adapter)
                {
                    return handler(adapter);
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new NpgsqlDataAdapter());
                }
                throw;
            }
        }

        public static void AddParameters(IDbCommand command, ICollection<IDbDataParameter> parameters)
        {
            command.Parameters.Clear();
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
            }
        }

        DataTable GetSchema(IDbConnection connection)
        {
            var CommandText = GlobalConstants.SchemaQueryPostgreSql;
            using (var command = _factory.CreateCommand(connection, CommandType.Text, CommandText, CommandTimeout))
            {
                return FetchDataTable(command);
            }
        }

        string GetHelpText(IDbConnection connection, string objectName)
        {
            using (
                var command = _factory.CreateCommand(connection, CommandType.Text,

                    string.Format("SHOW CREATE PROCEDURE {0} ", objectName), CommandTimeout))
            {
                return ExecuteReader(command, delegate (IDataAdapter reader)
                    {
                        var sb = new StringBuilder();
                        var ds = new DataSet(); //conn is opened by dataadapter
                        reader.Fill(ds);
                        var t = ds.Tables[0];
                        var dataTableReader = t.CreateDataReader();
                        while (dataTableReader.Read())
                        {
                            var value = dataTableReader.GetValue(2);
                            if (value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        return sb.ToString();
                    });
            }
        }

        public void GetProcedureInOutParams(string fullProcedureName, out List<NpgsqlParameter> inParameters, out List<NpgsqlParameter> outParameters)
        {
            using (var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                var inPramas = GetProcedureParameters(command, fullProcedureName, out List<IDbDataParameter> isOut);
                inParameters = inPramas.Select(a => a as NpgsqlParameter).ToList();
                outParameters = isOut.Select(a => a as NpgsqlParameter).ToList();
            }
        }

        public List<NpgsqlParameter> GetProcedureOutParams(string fullProcedureName)
        {
            using (var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                GetProcedureParameters(command, fullProcedureName, out List<IDbDataParameter> isOut);
                return isOut.Select(a => a as NpgsqlParameter).ToList();
            }
        }

        public List<NpgsqlParameter> GetProcedureInParams(string fullProcedureName)
        {
            using (var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                var inPramas = GetProcedureParameters(command, fullProcedureName, out List<IDbDataParameter> isOut);
                return inPramas.Select(a => a as NpgsqlParameter).ToList();
            }
        }

        /// <summary>
        /// Maps PostgreSQL data types to NpgsqlDbType. Uses Enum.TryParse first, then fallback mapping.
        /// </summary>
        /// <param name="pgType">The PostgreSQL data type from information_schema.udt_name</param>
        /// <param name="npgsqlType">Represents a PostgreSQL data type that can be written or read to the database</param>
        /// <returns>True if mapping succeeded; otherwise false</returns>
        static bool TryMapPostgresType(string pgType, out NpgsqlDbType npgsqlType)
        {
            if (string.IsNullOrWhiteSpace(pgType))
            {
                npgsqlType = NpgsqlDbType.Unknown;
                return false;
            }

            var lower = pgType.ToLowerInvariant();

            // Handle PostgreSQL array types (prefixed with "_", e.g., "_int4")
            if (lower.StartsWith("_"))
            {
                var elementType = lower.Substring(1);
                if (TryMapPostgresType(elementType, out var baseType) && baseType != NpgsqlDbType.Unknown)
                {
                    // Npgsql 9.0.4 supports Array flag combination
                    npgsqlType = NpgsqlDbType.Array | baseType;
                    return true;
                }
            }

            // First attempt: Try direct enum parse (handles exact enum name matches)
            if (Enum.TryParse(lower, true, out npgsqlType))
            {
                return true;
            }

            // Second attempt: Manual mapping for PostgreSQL udt_name types that don't match enum names
            switch (lower)
            {
                // Numeric types - PostgreSQL udt_name -> NpgsqlDbType
                case "int2":
                    npgsqlType = NpgsqlDbType.Smallint; return true;
                case "int4":
                    npgsqlType = NpgsqlDbType.Integer; return true;
                case "int8":
                    npgsqlType = NpgsqlDbType.Bigint; return true;
                case "float4":
                    npgsqlType = NpgsqlDbType.Real; return true;
                case "float8":
                    npgsqlType = NpgsqlDbType.Double; return true;
                
                // Character types
                case "bpchar":
                    npgsqlType = NpgsqlDbType.Char; return true;
                case "character varying":
                    npgsqlType = NpgsqlDbType.Varchar; return true;
                
                // Boolean
                case "bool":
                    npgsqlType = NpgsqlDbType.Boolean; return true;
                
                // Date/Time types with timezone
                case "timestamptz":
                case "timestamp with time zone":
                    npgsqlType = NpgsqlDbType.TimestampTz; return true; 
                
                case "timetz":
                case "time with time zone":
                    npgsqlType = NpgsqlDbType.TimeTz; return true; 
                
                // Date/Time types without timezone
                case "timestamp without time zone":
                    npgsqlType = NpgsqlDbType.Timestamp; return true;
                
                case "time without time zone":
                    npgsqlType = NpgsqlDbType.Time; return true;
                
                // Geometric types
                case "lseg":
                    npgsqlType = NpgsqlDbType.LSeg; return true;
                
                // Network address types
                case "macaddr":
                    npgsqlType = NpgsqlDbType.MacAddr; return true;
                case "macaddr8":
                    npgsqlType = NpgsqlDbType.MacAddr8; return true;
                
                // Text search types
                case "tsvector":
                    npgsqlType = NpgsqlDbType.TsVector; return true;
                case "tsquery":
                    npgsqlType = NpgsqlDbType.TsQuery; return true;
                
                // Internal types
                case "int2vector":
                    npgsqlType = NpgsqlDbType.Int2Vector; return true;
                
                // JSON types
                case "jsonpath":
                    npgsqlType = NpgsqlDbType.JsonPath; return true;
                
                // PostgreSQL LSN (Log Sequence Number)
                case "pg_lsn":
                    npgsqlType = NpgsqlDbType.PgLsn; return true;
                
                // ltree extension types
                case "ltree":
                    npgsqlType = NpgsqlDbType.LTree; return true;
                case "lquery":
                    npgsqlType = NpgsqlDbType.LQuery; return true;
                case "ltxtquery":
                    npgsqlType = NpgsqlDbType.LTxtQuery; return true;
                
                // Range types
                case "int4range":
                    npgsqlType = NpgsqlDbType.IntegerRange; return true;
                case "int8range":
                    npgsqlType = NpgsqlDbType.BigIntRange; return true;
                case "numrange":
                    npgsqlType = NpgsqlDbType.NumericRange; return true;
                case "tsrange":
                    npgsqlType = NpgsqlDbType.TimestampRange; return true;
                case "tstzrange":
                    npgsqlType = NpgsqlDbType.TimestampTzRange; return true;
                case "daterange":
                    npgsqlType = NpgsqlDbType.DateRange; return true;
                
                // Multirange types (PostgreSQL 14+)
                case "int4multirange":
                    npgsqlType = NpgsqlDbType.IntegerMultirange; return true;
                case "int8multirange":
                    npgsqlType = NpgsqlDbType.BigIntMultirange; return true;
                case "nummultirange":
                    npgsqlType = NpgsqlDbType.NumericMultirange; return true;
                case "tsmultirange":
                    npgsqlType = NpgsqlDbType.TimestampMultirange; return true;
                case "tstzmultirange":
                    npgsqlType = NpgsqlDbType.TimestampTzMultirange; return true;
                case "datemultirange":
                    npgsqlType = NpgsqlDbType.DateMultirange; return true;
                
                default:
                    npgsqlType = NpgsqlDbType.Unknown;
                    return false;
            }
        }

        List<IDbDataParameter> GetProcedureParameters(IDbCommand command, string procedureName, out List<IDbDataParameter> outParams)
        {
            outParams = new List<IDbDataParameter>();
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();

            var proc = string.Format(@"select parameter_name as paramname, parameters.udt_name as datatype, parameters.parameter_mode as direction FROM information_schema.routines
                JOIN information_schema.parameters ON routines.specific_name=parameters.specific_name
                WHERE routines.specific_schema='public' and routine_name ='{0}' 
                ORDER BY routines.routine_name, parameters.ordinal_position;", procedureName);

            command.CommandType = CommandType.Text;
            command.CommandText = proc;

            var dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                if (row != null)
                {
                    var paramName = row[0].ToString();
                    var datatype = row[1].ToString();
                    var direction = row[2].ToString();

                    // Try direct enum parse first, then use type mapping for PostgreSQL-specific names
                    if (!Enum.TryParse(datatype, true, out NpgsqlDbType sqlType))
                    {
                        // Fallback to custom mapping for types that don't match enum names
                        if (!TryMapPostgresType(datatype, out sqlType))
                        {
                            // If mapping fails, leave as Unknown and let Npgsql infer from value
                            sqlType = NpgsqlDbType.Unknown;
                        }
                    }

                    var sqlParameter = new NpgsqlParameter(paramName, sqlType);
                    
                    // Only explicitly set type if we successfully mapped it
                    if (sqlType != NpgsqlDbType.Unknown)
                    {
                        sqlParameter.NpgsqlDbType = sqlType;
                    }

                    var isOutput = direction.ToUpper().Trim().Contains("OUT");
                    if (direction.ToUpper().Trim().Contains("IN"))
                    {
                        isOutput = false;
                    }

                    if (!isOutput)
                    {
                        command.Parameters.Add(sqlParameter);
                        parameters.Add(sqlParameter);
                    }
                    else
                    {
                        sqlParameter.Direction = ParameterDirection.Output;
                        outParams.Add(sqlParameter);
                        sqlParameter.Value = "@a";
                    }
                }
            }

            command.CommandText = originalCommandText;
            return parameters;
        }


        /// <summary>
        /// This method returns the type of the provided procedure/function
        /// Returns: "<procedure>" for procedures, "<void>" for functions returning void, or the actual return type name for functions
        /// </summary>
        /// <param name="fullProcedureName"></param>
        /// <returns>return type identifier</returns>
        public string GetProcedureReturnType(string fullProcedureName)
        {
            using (var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName, CommandTimeout))
            {
                var originalCommandText = command.CommandText;

                // Query to get both the routine type (procedure/function) and return type
                var proc = string.Format(@"
                    SELECT 
                        r.routine_type,
                        r.data_type AS return_type
                    FROM information_schema.routines r
                    WHERE r.specific_schema='public' 
                    AND r.routine_name ='{0}'", fullProcedureName);

                command.CommandType = CommandType.Text;
                command.CommandText = proc;

                var dataTable = FetchDataTable(command);
                command.CommandText = originalCommandText;

                if (dataTable.Rows.Count > 0)
                {
                    var row = dataTable.Rows[0];
                    var routineType = row["routine_type"]?.ToString()?.ToUpper() ?? "";
                    var returnType = row["return_type"]?.ToString() ?? "";

                    // Check if it's a procedure
                    if (routineType == "PROCEDURE")
                    {
                        return "<procedure>";
                    }

                    // It's a function - check the return type
                    if (string.IsNullOrEmpty(returnType) || returnType.Equals("void", StringComparison.OrdinalIgnoreCase))
                    {
                        return "<void>";
                    }

                    // Return the actual type name for functions
                    return returnType;
                }
               
                return "<void>";
            }
        }

        #region IDisposable

        bool _disposed;

        public PostgreServer()
        {
            _factory = new PostgreFactory();
        }

        public PostgreServer(IDbFactory dbFactory)
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

        ~PostgreServer()
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

                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }


                    if (_command != null)
                    {
                        _command.Dispose();
                    }

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

        #endregion IDisposable
    }
}