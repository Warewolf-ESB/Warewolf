/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Services.Sql;
using Warewolf.Resource.Errors;
using Warewolf.Security.Encryption;
using System.Globalization;

namespace Dev2.Services.Sql
{
    public class SqlServer : IDbServer
    {
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();            
            _connection = null;
        }

        readonly IConnectionBuilder _connectionBuilder;
        public SqlServer(IConnectionBuilder connectionBuilder)
        {
            _connectionBuilder = connectionBuilder;
        }

        public SqlServer() : this(new ConnectionBuilder())
        {

        }

        public bool IsConnected { get; }
        public string ConnectionString { get => _connectionString; }
        string _connectionString;
        ISqlConnection _connection;
        IDbTransaction _transaction;


        public void Connect(string connectionString)
        {
            _connectionString = connectionString;
            _connection = _connectionBuilder.BuildConnection(_connectionString);

            try
            {
                _connection.TryOpen();
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw new WarewolfDbException(e.Message);
            }
        }

        public void Connect()
        {
            Connect(_connectionString);            
        }

        public void BeginTransaction()
        {
            if (_connection.State == ConnectionState.Open)
            {
                _transaction = _connection.BeginTransaction();
            }
        }

        public void RollbackTransaction()
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                }
                catch (Exception e)
                {
                    _transaction.Dispose();
                    _transaction = null;
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }

            }
        }

        static void TrySetTransaction(IDbTransaction dbTransaction, IDbCommand command)
        {
            if (dbTransaction != null && command.Transaction == null)
            {
                command.Transaction = dbTransaction;
            }
        }
        public DataTable FetchDataTable(IDbCommand command)
        {
            VerifyArgument.IsNotNull(nameof(command), command);
            _connection?.TryOpen();
            TrySetTransaction(_transaction, command);
            using (_connection)
            {
                if (_connection?.State != ConnectionState.Open)
                {
                    _connection = _connectionBuilder.BuildConnection(_connectionString);
                    _connection.EnsureOpen();
                    var dbCommand = _connection.CreateCommand();
                    TrySetTransaction(_transaction, dbCommand);
                    dbCommand.CommandText = command.CommandText;
                    dbCommand.CommandType = command.CommandType;
                    using (dbCommand)
                    {
                        using (var executeReader = dbCommand.ExecuteReader())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(executeReader);
                            return dataTable;
                        }
                    }
                }
                using (command)
                {
                    try
                    {
                        using (var executeReader = command.ExecuteReader())
                        {
                            var dataTable = new DataTable();
                            dataTable.Load(executeReader);
                            return dataTable;
                        }
                    }
                    catch (Exception e)
                    {
                        return new DataTable(e.Message);
                    }
                }
            }
        }

        public List<string> FetchDatabases()
        {
            if (_connection == null)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
            const string databaseColumnName = "database_name";
            var dataTable = _connection?.GetSchema("Databases") ?? new DataTable("Databases");
            var orderedRows = dataTable.Select("", databaseColumnName);
            var result = orderedRows.Select(row => (row[databaseColumnName] ?? string.Empty).ToString()).Distinct().ToList();
            return result;
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException,
            string dbName)
        {
            VerifyArgument.IsNotNull(nameof(procedureProcessor), procedureProcessor);
            VerifyArgument.IsNotNull(nameof(functionProcessor), functionProcessor);
            var proceduresDataTable = GetSchema();
            var procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            var procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            var procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA");
            foreach (DataRow row in proceduresDataTable.Rows)
            {
                var fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);
                Connect();
                var sqlCommand = _connection.CreateCommand();
                TrySetTransaction(_transaction, sqlCommand);
                sqlCommand.CommandText = fullProcedureName;
                sqlCommand.CommandType = CommandType.StoredProcedure;

                using (sqlCommand)
                {
                    TryFetch(procedureProcessor, functionProcessor, continueOnProcessorException, procedureTypeColumn, row, fullProcedureName, sqlCommand);
                }
            }
        }

        private void TryFetch(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException, DataColumn procedureTypeColumn, DataRow row, string fullProcedureName, IDbCommand sqlCommand)
        {
            try
            {
                var parameters = GetProcedureParameters(sqlCommand);
                const string helpText = "";

                if (IsStoredProcedure(row, procedureTypeColumn))
                {
                    procedureProcessor?.Invoke(sqlCommand, parameters, helpText, fullProcedureName);
                }
                else if (IsFunction(row, procedureTypeColumn))
                {
                    functionProcessor?.Invoke(sqlCommand, parameters, helpText, fullProcedureName);
                }
                else
                {
                    if (IsTableValueFunction(row, procedureTypeColumn))
                    {
                        functionProcessor?.Invoke(sqlCommand, parameters, helpText,
                            CreateTVFCommand(fullProcedureName, parameters));
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                if (!continueOnProcessorException)
                {
                    throw;
                }
            }
        }

       
        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor,
            Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor) => FetchStoredProcedures(procedureProcessor, functionProcessor, false, "");


        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor, 
            Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor, 
            bool continueOnProcessorException, string dbName)
        {
        }

        DataTable GetSchema()
        {
            var commandText = GlobalConstants.SchemaQuery;
            _connection.TryOpen();
            using (_connection)
            {
                using (var sqlCommand = _connection.CreateCommand())
                {
                    TrySetTransaction(_transaction, sqlCommand);
                    sqlCommand.CommandText = commandText;
                    sqlCommand.CommandType = CommandType.Text;
                    return FetchDataTable(sqlCommand);
                }
            }
        }
        static DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            var dataColumn = dataTable.Columns[columnName];
            if (dataColumn == null)
            {
                throw new Exception($"SQL Server - Unable to load '{columnName}' column of '{dataTable.TableName}'.");
            }
            return dataColumn;
        }

        static string GetFullProcedureName(DataRow row, DataColumn procedureDataColumn,
            DataColumn procedureSchemaColumn)
        {
            var procedureName = row[procedureDataColumn].ToString();
            var schemaName = row[procedureSchemaColumn].ToString();
            return schemaName + "." + procedureName;
        }

        List<IDbDataParameter> GetProcedureParameters(IDbCommand command)
        {
            //Please do not use SqlCommandBuilder.DeriveParameters(command); as it does not handle CLR procedures correctly.
            var originalCommandText = command.CommandText;
            var parameters = new List<IDbDataParameter>();
            var parts = command.CommandText.Split('.');
            command.CommandType = CommandType.Text;
            command.CommandText =
                $"select * from information_schema.parameters where specific_name='{parts[1]}' and specific_schema='{parts[0]}'";
            var dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                var parameterName = row["PARAMETER_NAME"] as string;
                if (String.IsNullOrEmpty(parameterName))
                {
                    continue;
                }
                Enum.TryParse(row["DATA_TYPE"] as string, true, out SqlDbType sqlType);
                var maxLength = row["CHARACTER_MAXIMUM_LENGTH"] as int? ?? -1;
                var sqlParameter = new SqlParameter(parameterName, sqlType, maxLength);
                command.Parameters.Add(sqlParameter);
                if (parameterName.ToLower(CultureInfo.InvariantCulture) == "@return_value")
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

        static string CreateTVFCommand(string fullProcedureName, List<IDbDataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return $"select * from {fullProcedureName}()";
            }
            var sql = new StringBuilder($"select * from {fullProcedureName}(");
            for (var i = 0; i < parameters.Count; i++)
            {
                sql.Append(parameters[i].ParameterName);
                sql.Append(i < parameters.Count - 1 ? "," : "");
            }
            sql.Append(")");
            return sql.ToString();
        }
        public IDbCommand CreateCommand()
        {
            if (_connection == null)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
            var sqlCommand = _connection.CreateCommand();
            TrySetTransaction(_transaction, sqlCommand);
            sqlCommand.CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds;
            return sqlCommand;
        }

        string _commantText;
        CommandType _commandType;


        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            VerifyArgument.IsNotNull(nameof(connectionString), connectionString);
            VerifyArgument.IsNotNull(nameof(commandText), commandText);
            var connString = connectionString;
            if (connString.CanBeDecrypted())
            {
                connString = DpapiWrapper.Decrypt(connectionString);
            }
            connString = string.Concat(connString, "MultipleActiveResultSets=true;");
            _connection = _connectionBuilder.BuildConnection(connString);

            _connection.TryOpen();
            _connection.FireInfoMessageEventOnUserErrors = true;
            _connection.StatisticsEnabled = true;
            _connection.SetInfoMessage((sender, args) =>
            {
                Dev2Logger.Debug("SQL Server:" + args.Message + " Source:" + args.Source,
                    GlobalConstants.WarewolfDebug);
                foreach (SqlError error in args.Errors)
                {
                    var errorMessages = new StringBuilder();
                    errorMessages.Append("Index #" + error.Number + Environment.NewLine +
                                         "Message: " + error.Message + Environment.NewLine +
                                         "LineNumber: " + error.LineNumber + Environment.NewLine +
                                         "Source: " + error.Source + Environment.NewLine +
                                         "Procedure: " + error.Procedure + Environment.NewLine);

                    Dev2Logger.Error("SQL Error:" + errorMessages, GlobalConstants.WarewolfError);
                }
            });
            var typeOfCommand = commandType;
            if (commandText.ToLower(CultureInfo.InvariantCulture).StartsWith("select ", StringComparison.Ordinal))
            {
                typeOfCommand = CommandType.Text;
            }

            _commantText = commandText;
            _commandType = typeOfCommand;


            return true;
        }

        public DataTable FetchDataTable(params IDbDataParameter[] dbDataParameters)
        {

            if (_connection == null)
            {
                throw new Exception(ErrorResource.PleaseConnectFirst);
            }
            _connection.TryOpen();

            using (_connection)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection = _connectionBuilder.BuildConnection(_connectionString);
                }
                using (var sqlCommand = _connection.CreateCommand())
                {
                    TrySetTransaction(_transaction, sqlCommand);
                    sqlCommand.CommandText = _commantText;
                    sqlCommand.CommandType = _commandType;
                    sqlCommand.CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds;
                    foreach (var dbDataParameter in dbDataParameters)
                    {
                        sqlCommand.Parameters.Add(dbDataParameter);
                    }
                    return FetchDataTable(sqlCommand);
                }
            }

        }
    }

    public class WarewolfDbException : DbException
    {
        public WarewolfDbException(string message) : base(message)
        {
        }
    }
}
