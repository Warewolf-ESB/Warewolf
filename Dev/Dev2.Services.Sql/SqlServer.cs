/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;

namespace Dev2.Services.Sql
{
    public class SqlServer : IDbServer
    {
        private readonly IDbFactory _factory;
        private IDbCommand _command;
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public bool IsConnected
        {
            get { return _connection != null && _connection.State == ConnectionState.Open; }
        }

        public string ConnectionString
        {
            get { return _connection == null ? null : _connection.ConnectionString; }
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

            const string DatabaseColumnName = "database_name";

            DataTable databases = GetSchemaFromConnection(_connection, "Databases");

            // 2013.07.10 - BUG 9933 - AL - sort database list
            DataRow[] orderedRows = databases.Select("", DatabaseColumnName);

            List<string> result =
                orderedRows.Select(row => (row[DatabaseColumnName] ?? string.Empty).ToString()).Distinct().ToList();

            return result;
        }

        #endregion

        #region FetchDataTable

        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull("command", command);

            return ExecuteReader(command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                reader => _factory.CreateTable(reader, LoadOption.OverwriteChanges));
        }

        public DataTable FetchDataTable(params IDbDataParameter[] parameters)
        {
            VerifyConnection();
            AddParameters(_command, parameters);
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
            bool continueOnProcessorException = false)
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            VerifyConnection();

            DataTable proceduresDataTable = GetSchema(_connection);
            DataColumn procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            DataColumn procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            DataColumn procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA");
                // ROUTINE_CATALOG - ROUTINE_SCHEMA ,SPECIFIC_SCHEMA

            foreach (DataRow row in proceduresDataTable.Rows)
            {
                string fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);

                using (
                    IDbCommand command = _factory.CreateCommand(_connection, CommandType.StoredProcedure,
                        fullProcedureName))
                {
                    try
                    {
                        List<IDbDataParameter> parameters = GetProcedureParameters(command);

                        string helpText = FetchHelpTextContinueOnException(fullProcedureName, _connection);

                        if (IsStoredProcedure(row, procedureTypeColumn))
                        {
                            procedureProcessor(command, parameters, helpText, fullProcedureName);
                        }
                        else if (IsFunction(row, procedureTypeColumn))
                        {
                            functionProcessor(command, parameters, helpText, fullProcedureName);
                        }
                        else if (IsTableValueFunction(row, procedureTypeColumn))
                        {
                            functionProcessor(command, parameters, helpText,
                                CreateTVFCommand(fullProcedureName, parameters));
                        }
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

        // ReSharper disable InconsistentNaming
        private string CreateTVFCommand(string fullProcedureName, List<IDbDataParameter> parameters)
            // ReSharper restore InconsistentNaming
        {
            if (parameters == null || parameters.Count == 0)
            {
                return string.Format("select * from {0}()", fullProcedureName);
            }
            var sql = new StringBuilder(string.Format("select * from {0}(", fullProcedureName));
            for (int i = 0; i < parameters.Count; i++)
            {
                sql.Append(parameters[i].ParameterName);
                sql.Append(i < parameters.Count - 1 ? "," : "");
            }
            sql.Append(")");
            return sql.ToString();
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
            _connection = _factory.CreateConnection(connectionString);
            _connection.Open();
            return true;
        }

        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            _connection = _factory.CreateConnection(connectionString);

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
                    exceptionDataTable.LoadDataRow(new object[] {e.Message}, true);
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
            const string CommandText = GlobalConstants.SchemaQuery;
            using (IDbCommand command = _factory.CreateCommand(connection, CommandType.Text, CommandText))
            {
                return FetchDataTable(command);
            }
        }

        private DataTable GetSchemaFromConnection(IDbConnection connection, string collectionName)
        {
            return _factory.GetSchema(connection, collectionName); //todo: fix this
        }

        private string GetHelpText(IDbConnection connection, string objectName)
        {
            using (
                IDbCommand command = _factory.CreateCommand(connection, CommandType.Text,
                    string.Format("sp_helptext '{0}'", objectName)))
            {
                return ExecuteReader(command, (CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo),
                    delegate(IDataReader reader)
                    {
                        var sb = new StringBuilder();
                        while (reader.Read())
                        {
                            object value = reader.GetValue(0);
                            if (value != null)
                            {
                                sb.Append(value);
                            }
                        }
                        return sb.ToString();
                    });
            }
        }

        private static DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            DataColumn dataColumn = dataTable.Columns[columnName];
            if (dataColumn == null)
            {
                throw new Exception(string.Format("SQL Server - Unable to load '{0}' column of '{1}'.", columnName,
                    dataTable.TableName));
            }
            return dataColumn;
        }

        private static string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn,
            DataColumn procedureSchemaColumn)
        {
            string procedureName = row[procedureDataColumn].ToString();
            string schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        private List<IDbDataParameter> GetProcedureParameters(IDbCommand command)
        {
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            string originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();
            string[] parts = command.CommandText.Split('.');
            command.CommandType = CommandType.Text;
            command.CommandText =
                string.Format(
                    "select * from information_schema.parameters where specific_name='{0}' and specific_schema='{1}'",
                    parts[1], parts[0]);
            DataTable dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                var parameterName = row["PARAMETER_NAME"] as string;
                if (String.IsNullOrEmpty(parameterName))
                {
                    continue;
                }
                SqlDbType sqlType;
                Enum.TryParse(row["DATA_TYPE"] as string, true, out sqlType);
                int maxLength = row["CHARACTER_MAXIMUM_LENGTH"] is int ? (int) row["CHARACTER_MAXIMUM_LENGTH"] : -1;
                var sqlParameter = new SqlParameter(parameterName, sqlType, maxLength);
                command.Parameters.Add(sqlParameter);
                if (parameterName.ToLower() == "@return_value")
                {
                    continue;
                }
                parameters.Add(sqlParameter);
            }
            command.CommandText = originalCommandText;
            return parameters;
        }

        public static bool IsStoredProcedure(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }
            return row[procedureTypeColumn].ToString().Equals("SQL_STORED_PROCEDURE") ||
                   row[procedureTypeColumn].ToString().Equals("CLR_STORED_PROCEDURE");
        }

        public static bool IsFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return row[procedureTypeColumn].ToString().Equals("SQL_SCALAR_FUNCTION");
        }

        public static bool IsTableValueFunction(DataRow row, DataColumn procedureTypeColumn)
        {
            if (row == null || procedureTypeColumn == null)
            {
                return false;
            }

            return row[procedureTypeColumn].ToString().Equals("SQL_TABLE_VALUED_FUNCTION");
        }

        #region IDisposable

        private bool _disposed;

        public SqlServer()
        {
            _factory = new DbFactory();
        }

        public SqlServer(IDbFactory dbFactory)
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
        ~SqlServer()
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
        protected virtual void Dispose(bool disposing)
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
