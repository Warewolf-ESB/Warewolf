using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using MySql.Data.MySqlClient;
using Npgsql;
using NpgsqlTypes;
using Oracle.ManagedDataAccess.Client;

namespace Dev2.Services.Sql
{
    public class PostgreServer : IDbServer
    {
        private readonly IDbFactory _factory;
        private IDbCommand _command;
        private NpgsqlConnection _connection;
        private IDbTransaction _transaction;

        public bool IsConnected
        {
            get { return _connection != null && _connection.State == ConnectionState.Open; }
        }

        public string ConnectionString
        {
            get { return _connection == null ? null : _connection.ConnectionString; }
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException = false, string dbName = "")
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            DataTable proceduresDataTable = GetSchema(_connection);


            // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                string fullProcedureName = row["Name"].ToString();
                if (row["Db"].ToString() == dbName)
                {
                    using (
                        IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName))
                    {
                        try
                        {
                            List<IDbDataParameter> outParameters;

                            List<IDbDataParameter> parameters = GetProcedureParameters(command, dbName, fullProcedureName, out outParameters);
                            string helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

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
            List<string> result = new List<string>();
            NpgsqlCommand cmd = new NpgsqlCommand("select datname from pg_database", _connection);
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
                if (reader != null) reader.Close();
            }

            return result;
        }

        #endregion

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

        #endregion

        #region FetchDataSet

        public DataSet FetchDataSet(params SqlParameter[] parameters)
        {
            VerifyConnection();
            return FetchDataSet(_command, parameters);
        }

        public DataSet FetchDataSet(IDbCommand command, params SqlParameter[] parameters)
        {
            VerifyArgument.IsNotNull("command", command);
            AddParameters(command, parameters);
            return _factory.FetchDataSet(command);
        }

        #endregion

        #region FetchStoredProcedures

        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor,
            bool continueOnProcessorException = false, string dbName = "")
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            DataTable proceduresDataTable = GetSchema(_connection);

            // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                string fullProcedureName = row["Name"].ToString();
                if (row["Db"].ToString() == dbName)
                {
                    using (
                        IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                            fullProcedureName))
                    {
                        try
                        {
                            List<IDbDataParameter> isOut;
                            List<IDbDataParameter> parameters = GetProcedureParameters(command, dbName, fullProcedureName, out isOut);
                            string helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

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

        // ReSharper disable InconsistentNaming

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

        #endregion

        #region VerifyConnection

        private void VerifyConnection()
        {
            if (!IsConnected)
            {
                throw new Exception("Please connect first.");
            }
        }

        #endregion

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

        #endregion

        private static T ExecuteReader<T>(IDbCommand command, CommandBehavior commandBehavior,
            Func<IDataReader, T> handler)
        {
            try
            {
                using (IDataReader reader = command.ExecuteReader(commandBehavior))
                {
                    return handler(reader);
                }
            }
            catch (DbException e)
            {
                if (e.Message.Contains("There is no text for object "))
                {
                    var exceptionDataTable = new DataTable("Error");
                    exceptionDataTable.Columns.Add("ErrorText");
                    exceptionDataTable.LoadDataRow(new object[] { e.Message }, true);
                    return handler(new DataTableReader(exceptionDataTable));
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

        private DataTable GetSchema(IDbConnection connection)
        {
            const string CommandText = GlobalConstants.SchemaQueryPostgreSql;
            using (IDbCommand command = _factory.CreateCommand(connection, CommandType.Text, CommandText))
            {
                return FetchDataTable(command);
            }
        }

        private string GetHelpText(IDbConnection connection, string objectName)
        {
            using (
                IDbCommand command = _factory.CreateCommand(connection, CommandType.Text,
                    string.Format("SHOW CREATE PROCEDURE {0} ", objectName)))
            {
                return ExecuteReader(command, CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo,
                    delegate (IDataReader reader)
                    {
                        var sb = new StringBuilder();
                        while (reader.Read())
                        {
                            object value = reader.GetValue(2);
                            if (value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        return sb.ToString();
                    });
            }
        }

        public List<NpgsqlParameter> GetProcedureOutParams(string fullProcedureName, string dbName)
        {
            using (IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure, fullProcedureName))
            {

                List<IDbDataParameter> isOut;
                GetProcedureParameters(command, dbName, fullProcedureName, out isOut);
                return isOut.Select(a => a as NpgsqlParameter).ToList();

            }
        }

        public List<IDbDataParameter> GetProcedureParameters(IDbCommand command, string dbName, string procedureName, out List<IDbDataParameter> outParams)
        {
            outParams = new List<IDbDataParameter>();
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            string originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();

            var proc = string.Format(@"select parameter_name as paramname, parameters.udt_name as datatype, parameters.parameter_mode as direction FROM information_schema.routines
                JOIN information_schema.parameters ON routines.specific_name=parameters.specific_name
                WHERE routines.specific_schema='public' and routine_name ='{0}' and parameters.parameter_mode = 'IN'
                ORDER BY routines.routine_name, parameters.ordinal_position;", procedureName);

            command.CommandType = CommandType.Text;
            command.CommandText = proc;

            DataTable dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                if (row != null)
                {
                    var value = row[0].ToString();
                    var datatype = row[1].ToString();
                    var direction = row[2].ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        NpgsqlDbType sqlType;

                        Enum.TryParse(datatype, true, out sqlType);

                        var sqlParameter = new NpgsqlParameter(value, sqlType);

                        bool isout = false;

                        if (direction.Contains("OUT "))
                            isout = true;
                        if (direction.Contains("IN"))
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
                            command.Parameters.Add(sqlParameter);
                        }
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

        [ExcludeFromCodeCoverage]
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

        #endregion
    }
}
