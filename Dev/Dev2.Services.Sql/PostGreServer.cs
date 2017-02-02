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
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace Dev2.Services.Sql
{
    public class PostgreServer : IDbServer
    {
        // ReSharper disable once SuggestVarOrType_Elsewhere

        private readonly IDbFactory _factory;
        private IDbCommand _command;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public bool IsConnected
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _connection != null && _connection.State == ConnectionState.Open; }
        }

        public string ConnectionString
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            // ReSharper disable once MergeConditionalExpression
            get { return _connection == null ? null : _connection.ConnectionString; }
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException = false, string dbName = "")
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            var proceduresDataTable = GetSchema(_connection);

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var type = row["proretset"];
                if (type.ToString().ToUpperInvariant() == "FALSE")
                {
                    continue;
                }
                var fullProcedureName = row["Name"].ToString();

                if (row["Db"].ToString() == dbName)
                {
                    using (
                        var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName))
                    {
                        try
                        {
                            List<IDbDataParameter> outParameters;

                            var parameters = GetProcedureParameters(command, fullProcedureName, out outParameters);
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
                }
            }
        }

        public IDbCommand CreateCommand()
        {
            VerifyConnection();
            IDbCommand command = _connection.CreateCommand();
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
                // ReSharper disable once UseNullPropagation
                if (reader != null) reader.Close();
            }

            return result;
        }

        #endregion FetchDatabases

        #region FetchDataTable

        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo,
                reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
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
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
            bool continueOnProcessorException = false, string dbName = "")
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            var proceduresDataTable = GetSchema(_connection);

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var fullProcedureName = row["Name"].ToString();
                if (row["Db"].ToString() == dbName)
                {
                    using (
                        var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName))
                    {
                        try
                        {
                            List<IDbDataParameter> isOut;
                            var parameters = GetProcedureParameters(command, fullProcedureName, out isOut);
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
                }
            }
        }

        private string FetchHelpTextContinueOnException(string fullProcedureName, IDbConnection con)
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

        private void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
        }

        #endregion VerifyConnection

        #region Connect

        public bool Connect(string connectionString)
        {
            _connection = (NpgsqlConnection)_factory.CreateConnection(connectionString);
            _connection.Open();
            return true;
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = (NpgsqlConnection)_factory.CreateConnection(connectionString);

            VerifyArgument.IsNotNull("commandText", commandText);
            if (commandText.ToLower().StartsWith("select "))
            {
                commandType = CommandType.Text;
            }

            _command = _factory.CreateCommand(_connection, commandType, commandText);

            _connection.Open();
            return true;
        }

        #endregion Connect

        private static T ExecuteReader<T>(IDbCommand command, CommandBehavior commandBehavior,
            Func<IDataAdapter, T> handler)
        {
            try
            {
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command as NpgsqlCommand);
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

        private static void AddParameters(IDbCommand command, ICollection<IDbDataParameter> parameters)
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

        private DataTable GetSchema(IDbConnection connection)
        {
            const string CommandText = GlobalConstants.SchemaQueryPostgreSql;
            using (var command = _factory.CreateCommand(connection, CommandType.Text, CommandText))
            {
                return FetchDataTable(command);
            }
        }

        private string GetHelpText(IDbConnection connection, string objectName)
        {
            using (
                var command = _factory.CreateCommand(connection, CommandType.Text,
                    // ReSharper disable once UseStringInterpolation
                    string.Format("SHOW CREATE PROCEDURE {0} ", objectName)))
            {
                return ExecuteReader(command, CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo,
                    delegate (IDataAdapter reader)
                    {
                        var sb = new StringBuilder();
                        DataSet ds = new DataSet(); //conn is opened by dataadapter
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

        public List<NpgsqlParameter> GetProcedureOutParams(string fullProcedureName)
        {
            using (var command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName))
            {
                List<IDbDataParameter> isOut;
                GetProcedureParameters(command, fullProcedureName, out isOut);
                return isOut.Select(a => a as NpgsqlParameter).ToList();
            }
        }

        private List<IDbDataParameter> GetProcedureParameters(IDbCommand command, string procedureName, out List<IDbDataParameter> outParams)
        {
            outParams = new List<IDbDataParameter>();
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();

            // ReSharper disable once UseStringInterpolation
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
                    var value = row[0].ToString();
                    var datatype = row[1].ToString();
                    var direction = row[2].ToString();

                    NpgsqlDbType sqlType;

                    Enum.TryParse(datatype, true, out sqlType);

                    var sqlParameter = new NpgsqlParameter(value, sqlType);

                    var isout = direction.ToUpper().Trim().Contains("OUT".Trim());
                    if (direction.ToUpper().Trim().Contains("IN".Trim()))
                        isout = false;

                    if (!isout)
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

        #region IDisposable

        private bool _disposed;

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
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    // ReSharper disable once UseNullPropagation
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                    }

                    // ReSharper disable once UseNullPropagation
                    if (_command != null)
                    {
                        _command.Dispose();
                    }

                    if (_connection != null)
                    {
                        if (_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                        _connection.Dispose();
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }

        #endregion IDisposable
    }
}