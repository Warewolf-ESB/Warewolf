/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Security.Encryption;

namespace Dev2.Services.Sql
{
    public class SqlServer : IDbServer
    {
        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        public SqlServer(ISqlConnection connection)
        {
            _connection = connection;
        }

        public SqlServer()
        {

        }

        public bool IsConnected { get; }
        public string ConnectionString { get; }
        private string _connectionString;
        private ISqlConnection _connection;
        private IDbTransaction _transaction;
        public bool Connect(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnectionWrapper(_connectionString);

            try
            {
                _connection.TryOpen();
                return true;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                return false;
            }

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

        void TrySetTransaction(IDbTransaction dbTransaction, IDbCommand command)
        {
            if (dbTransaction != null && command.Transaction == null)
            {
                command.Transaction = dbTransaction;
            }
        }
        public DataTable FetchDataTable(IDbCommand command)
        {
            _connection?.TryOpen();
            TrySetTransaction(_transaction, command);
            using (_connection)
            {
                if (_connection?.State != ConnectionState.Open)
                {
                    _connection = new SqlConnectionWrapper(_connectionString);
                    _connection.Open();
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
                    using (var executeReader = command.ExecuteReader())
                    {
                        var dataTable = new DataTable();
                        dataTable.Load(executeReader);
                        return dataTable;
                    }
                }
            }
        }

        public List<string> FetchDatabases()
        {
            const string databaseColumnName = "database_name";

            var dataTable = _connection?.GetSchema("Databases") ?? new DataTable("Databases");
            var orderedRows = dataTable.Select("", databaseColumnName);
            var result = orderedRows.Select(row => (row[databaseColumnName] ?? string.Empty).ToString()).Distinct().ToList();
            return result;
        }

        public void FetchStoredProcedures(Func<IDbCommand, List<IDbDataParameter>, string, string, bool> procedureProcessor, Func<IDbCommand, List<IDbDataParameter>, string, string, bool> functionProcessor, bool continueOnProcessorException = false,
            string dbName = "")
        {
            VerifyArgument.IsNotNull("procedureProcessor", procedureProcessor);
            VerifyArgument.IsNotNull("functionProcessor", functionProcessor);
            DataTable proceduresDataTable = GetSchema();
            DataColumn procedureDataColumn = GetDataColumn(proceduresDataTable, "ROUTINE_NAME");
            DataColumn procedureTypeColumn = GetDataColumn(proceduresDataTable, "ROUTINE_TYPE");
            DataColumn procedureSchemaColumn = GetDataColumn(proceduresDataTable, "SPECIFIC_SCHEMA");
            foreach (DataRow row in proceduresDataTable.Rows)
            {
                string fullProcedureName = GetFullProcedureName(row, procedureDataColumn, procedureSchemaColumn);
                _connection.TryOpen();
                var sqlCommand = _connection.CreateCommand();
                TrySetTransaction(_transaction, sqlCommand);
                sqlCommand.CommandText = fullProcedureName;
                sqlCommand.CommandType = CommandType.StoredProcedure;

                using (sqlCommand)
                {
                    try
                    {
                        var parameters = GetProcedureParameters(sqlCommand);
                        const string helpText = "";

                        if (IsStoredProcedure(row, procedureTypeColumn))
                        {
                            procedureProcessor(sqlCommand, parameters, helpText, fullProcedureName);
                        }
                        else if (IsFunction(row, procedureTypeColumn))
                        {
                            functionProcessor(sqlCommand, parameters, helpText, fullProcedureName);
                        }
                        else if (IsTableValueFunction(row, procedureTypeColumn))
                        {
                            functionProcessor(sqlCommand, parameters, helpText,
                                CreateTVFCommand(fullProcedureName, parameters));
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

            }
        }

        public void FetchStoredProcedures(
            Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> procedureProcessor
            , Func<IDbCommand, List<IDbDataParameter>, List<IDbDataParameter>, string, string, bool> functionProcessor
            , bool continueOnProcessorException = false,
            string dbName = "")
        {


        }
        private DataTable GetSchema()
        {
            const string commandText = GlobalConstants.SchemaQuery;
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
        private static DataColumn GetDataColumn(DataTable dataTable, string columnName)
        {
            DataColumn dataColumn = dataTable.Columns[columnName];
            if (dataColumn == null)
            {
                throw new Exception($"SQL Server - Unable to load '{columnName}' column of '{dataTable.TableName}'.");
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
                $"select * from information_schema.parameters where specific_name='{parts[1]}' and specific_schema='{parts[0]}'";
            DataTable dataTable = FetchDataTable(command);
            foreach (DataRow row in dataTable.Rows)
            {
                var parameterName = row["PARAMETER_NAME"] as string;
                if (String.IsNullOrEmpty(parameterName))
                {
                    continue;
                }
                Enum.TryParse(row["DATA_TYPE"] as string, true, out SqlDbType sqlType);
                int maxLength = row["CHARACTER_MAXIMUM_LENGTH"] as int? ?? -1;
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

        private string CreateTVFCommand(string fullProcedureName, List<IDbDataParameter> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return $"select * from {fullProcedureName}()";
            }
            var sql = new StringBuilder($"select * from {fullProcedureName}(");
            for (int i = 0; i < parameters.Count; i++)
            {
                sql.Append(parameters[i].ParameterName);
                sql.Append(i < parameters.Count - 1 ? "," : "");
            }
            sql.Append(")");
            return sql.ToString();
        }
        public IDbCommand CreateCommand()
        {
            var sqlCommand = _connection.CreateCommand();
            TrySetTransaction(_transaction, sqlCommand);
            sqlCommand.CommandTimeout = (int)GlobalConstants.TransactionTimeout.TotalSeconds;
            return sqlCommand;
        }

        private string _commantText;
        private CommandType _commandType;
        public bool Connect(string connectionString, CommandType commandType, string commandText)
        {
            VerifyArgument.IsNotNull("connectionString", connectionString);
            VerifyArgument.IsNotNull("commandText", commandText);
            if (connectionString.CanBeDecrypted())
            {
                connectionString = DpapiWrapper.Decrypt(connectionString);
            }
            connectionString = string.Concat(connectionString, "MultipleActiveResultSets=true;");
            _connection = new SqlConnectionWrapper(connectionString);

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

                    Dev2Logger.Error("SQL Error:" + errorMessages.ToString(), GlobalConstants.WarewolfError);
                }
            });

            _commantText = commandText;
            _commandType = commandType;


            return true;
        }

        public DataTable FetchDataTable(IDbDataParameter[] dbDataParameters, string conString)
        {
            _connection.TryOpen();
            using (_connection)
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection = new SqlConnectionWrapper(conString);
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
