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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System.Data.Odbc;
using System.Data.SQLite;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Npgsql;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Dev2.Common.DateAndTime;
using TSQL;

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution : ServiceExecutionAbstract<DbService, DbSource>, IDisposable
    {
        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true)
        {
        }

        public string ProcedureName { private get; set; }
        public string SqlQuery { private get; set; }
        public int ConnectionTimeout { private get; set; }
        public int? CommandTimeout { private get; set; }

        MySqlServer SetupMySqlServer(ErrorResultTO errors)
        {
            var server = new MySqlServer();
            if (CommandTimeout != null)
            {
                server.CommandTimeout = CommandTimeout.Value;
            }

            try
            {
                var connected = server.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString), GlobalConstants.WarewolfError);
                }

                return server;
            }
            catch (MySqlException sex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(sex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return server;
        }

        public bool SourceIsNull() => Source == null;

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected override object ExecuteService(int update, out ErrorResultTO errors, IOutputFormatter formater)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();

            switch (Source.ServerType)
            {
                case enSourceType.SqlDatabase:
                {
                    try
                    {
                        MssqlSqlExecution(ConnectionTimeout, CommandTimeout, invokeErrors, update);
                        _errorResult.MergeErrors(invokeErrors);
                        return Guid.NewGuid();
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e.StackTrace, DataObj.ExecutionID.ToString());
                        return Guid.NewGuid();
                    }
                }
                case enSourceType.MySqlDatabase:
                {
                    object result = MySqlExecution(invokeErrors, update);
                    _errorResult.MergeErrors(invokeErrors);
                    return result;
                }

                case enSourceType.Oracle:
                {
                    object result = OracleExecution(invokeErrors, update);
                    _errorResult.MergeErrors(invokeErrors);
                    return result;
                }

                case enSourceType.ODBC:
                {
                    object result = OdbcExecution(invokeErrors, update);
                    _errorResult.MergeErrors(invokeErrors);
                    return result;
                }
                case enSourceType.PostgreSQL:
                {
                    object result = PostgreSqlExecution(invokeErrors, update);
                    _errorResult.MergeErrors(invokeErrors);
                    return result;
                }
                case enSourceType.SQLiteDatabase:
                {
                    object result = SqliteExecution(invokeErrors, update);
                    _errorResult.MergeErrors(invokeErrors);
                    return result;
                }
                default:
                    return null;
            }
        }

        void TranslateDataTableToEnvironment(DataTable executeService, IExecutionEnvironment environment, int update)
        {
            var started = true;
            if (executeService != null && Outputs != null && Outputs.Count != 0 && executeService.Rows != null)
            {
                try
                {
                    var rowIdx = 1;
                    MapDataRowsToEnvironment(executeService, environment, update, ref started, ref rowIdx);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
            }
        }

        void MapDataRowsToEnvironment(DataTable executeService, IExecutionEnvironment environment, int update, ref bool started, ref int rowIdx)
        {
            foreach (DataRow row in executeService.Rows)
            {
                ProcessDataRow(executeService, environment, update, ref started, ref rowIdx, row);
            }
        }

        void ProcessDataRow(DataTable executeService, IExecutionEnvironment environment, int update, ref bool started, ref int rowIdx, DataRow row)
        {
            foreach (var serviceOutputMapping in Outputs)
            {
                if (!string.IsNullOrEmpty(serviceOutputMapping?.MappedTo))
                {
                    ProcessOutputMapping(executeService, environment, update, ref started, ref rowIdx, row, serviceOutputMapping);
                }
            }

            rowIdx++;
        }

        void TranslateDataSetToEnvironment(DataSet executeService, IExecutionEnvironment environment, int update)
        {
            var started = true;
            foreach (DataTable table in executeService.Tables)
            {
                if (executeService != null && Outputs != null && Outputs.Count != 0 && table.Rows != null)
                {
                    try
                    {
                        var rowIdx = 1;
                        MapDataRowsToEnvironment(table, environment, update, ref started, ref rowIdx);
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }
                }
            }
        }

        static void ProcessOutputMapping(DataTable executeService, IExecutionEnvironment environment, int update, ref bool started, ref int rowIdx, DataRow row, IServiceOutputMapping serviceOutputMapping)
        {
            var rsType = DataListUtil.GetRecordsetIndexType(serviceOutputMapping.MappedTo);
            var rowIndex = DataListUtil.ExtractIndexRegionFromRecordset(serviceOutputMapping.MappedTo);
            var rs = serviceOutputMapping.RecordSetName;

            if (!string.IsNullOrEmpty(rs) && environment.HasRecordSet(rs))
            {
                if (started)
                {
                    rowIdx = environment.GetLength(rs) + 1;
                    started = false;
                }
            }
            else
            {
                try
                {
                    environment.AssignDataShape(serviceOutputMapping.MappedTo);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
            }

            GetRowIndex(ref started, ref rowIdx, rsType, rowIndex);
            if (!executeService.Columns.Contains(serviceOutputMapping.MappedFrom) && !executeService.Columns.Contains("ReadForXml"))
            {
                return;
            }

            var value = GetColumnValue(executeService, row, serviceOutputMapping);
            if (update != 0)
            {
                rowIdx = update;
            }

            var displayExpression = DataListUtil.ReplaceRecordsetBlankWithIndex(DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo), rowIdx);
            if (rsType == enRecordsetIndexType.Star)
            {
                displayExpression = DataListUtil.ReplaceStarWithFixedIndex(displayExpression, rowIdx);
            }

            if (value is DateTime)
            {
                var error = "";
                var format = DateTimeConverterFactory.CreateFormatter();
                if (format.TryFormat(((DateTime) value), out var result, out error))
                {
                    environment.Assign(displayExpression, result, update);
                }
                else
                {
                    Dev2Logger.Error("Error:", error);
                }
            }
            else
            {
                environment.Assign(displayExpression, value.ToString(), update);
            }
        }

        static object GetColumnValue(DataTable executeService, DataRow row, IServiceOutputMapping serviceOutputMapping) => executeService.Columns.Contains("ReadForXml") ? row["ReadForXml"] : row[serviceOutputMapping.MappedFrom];

        static void GetRowIndex(ref bool started, ref int rowIdx, enRecordsetIndexType rsType, string rowIndex)
        {
            if (rsType == enRecordsetIndexType.Star && started)
            {
                rowIdx = 1;
                started = false;
            }

            if (rsType == enRecordsetIndexType.Numeric)
            {
                rowIdx = int.Parse(rowIndex);
            }
        }


        static string MssqlGetSqlForProcedure(SqlConnection connection, string procedureName)
        {
            var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = string.Format("sp_helptext '{0}'", procedureName);
            using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly & CommandBehavior.KeyInfo))
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

                if (sb.Length == 0)
                {
                    throw new WarewolfDbException(string.Format("There is no text for object '{0}'.", procedureName));
                }

                return sb.ToString();
            }
        }

        private bool MssqlIsStoredProcForXmlResult(SqlConnection connection, string procedureName)
        {
            bool result = false;
            var procSqlScript = MssqlGetSqlForProcedure(connection, procedureName);
            var statements = TSQLStatementReader.ParseStatements(procSqlScript);
            foreach (var statement in statements)
            {
                var tokens = statement.Tokens;
                var cnt = tokens.Count;

                if (cnt < 3)
                {
                    continue;
                }

                var i = 0;
                for (; i < tokens.Count; i++)
                {
                    if (tokens[i].Type == TSQL.Tokens.TSQLTokenType.Keyword && tokens[i].Text.ToUpper() == "FOR")
                    {
                        i++;
                        result = (i < tokens.Count && tokens[i].Type == TSQL.Tokens.TSQLTokenType.Identifier && tokens[i].Text == "XML");
                    }
                }
            }

            return result;
        }

        void MssqlSqlExecution(int connectionTimeout, int? commandTimeout, ErrorResultTO errors, int update)
        {
            DataObj.StateNotifier?.LogAdditionalDetail(new
                {
                    this.Source,
                    this.ProcedureName,
                    this.SqlQuery,
                    this.ConnectionTimeout,
                    this.CommandTimeout,
                },
                nameof(MssqlSqlExecution)
            );

            var connectionBuilder = new ConnectionBuilder();
            var connection = new SqlConnection(connectionBuilder.ConnectionString(Source.GetConnectionStringWithTimeout(connectionTimeout)));
            var startTime = Stopwatch.StartNew();
            try
            {
                connection.Open();
                if (MssqlIsStoredProcForXmlResult(connection, ProcedureName))
                {
                    MssqlReadDataForXml(update, startTime, connection, commandTimeout);
                }
                else
                {
                    MssqlReadData(update, startTime, connection, commandTimeout);
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SQL Error:", ex, GlobalConstants.WarewolfError);
                Dev2Logger.Error("SQL Error:", ex.StackTrace);
                errors.AddError($"SQL Error: {ex.Message}");
            }
            finally
            {
                connection.Dispose();
            }
        }

        private void MssqlReadData(int update, Stopwatch startTime, SqlConnection connection, int? commandTimeout)
        {
            using (SqlTransaction dbTransaction = connection.BeginTransaction())
            {
                using (var cmd = MssqlCreateCommand(connection, commandTimeout, GetSqlParameters()))
                {
                    cmd.Transaction = dbTransaction;
                    using (var reader = cmd.ExecuteReader())
                    {
                        var table = new DataTable();
                        table.Load(reader);
                        reader.Close();
                        dbTransaction.Commit();
                        Dev2Logger.Info("Time taken to process proc " + ProcedureName + ":" + startTime.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                        var startTime1 = Stopwatch.StartNew();
                        TranslateDataTableToEnvironment(table, DataObj.Environment, update);
                        Dev2Logger.Info("Time taken to TranslateDataTableToEnvironment " + ProcedureName + ":" + startTime1.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                    }
                }
            }
        }

        bool MssqlReadDataForXml(int update, Stopwatch startTime, SqlConnection connection, int? commandTimeout)
        {
            var xmlResults = false;
            var dbTransaction = connection.BeginTransaction();
            try
            {
                using (var cmd = MssqlCreateCommand(connection, commandTimeout, GetSqlParameters()))
                {
                    cmd.Transaction = dbTransaction;
                    using (var reader = cmd.ExecuteXmlReader())
                    {
                        var hasXmlElement = reader.Read();
                        var table = new DataTable("x");
                        table.Columns.Add("ReadForXml");
                        while (hasXmlElement)
                        {
                            var outerXml = reader.ReadOuterXml();
                            if (outerXml == string.Empty)
                            {
                                break;
                            }

                            table.LoadDataRow(new object[] {outerXml}, true);
                            xmlResults = true;
                        }

                        Dev2Logger.Info("Time taken to process proc " + ProcedureName + ":" + startTime.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                        var startTime1 = Stopwatch.StartNew();
                        TranslateDataTableToEnvironment(table, DataObj.Environment, update);
                        Dev2Logger.Info("Time taken to TranslateDataTableToEnvironment " + ProcedureName + ":" + startTime1.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                        dbTransaction.Commit();
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    // Attempt to roll back the transaction.
                    dbTransaction.Rollback();
                }
                catch (Exception exRollback)
                {
                    //Ignore the rollback exception
                    Dev2Logger.Error("Error Rolling Back Transaction", exRollback, GlobalConstants.WarewolfError);
                }

                if (!e.Message.Equals(ErrorResource.NotXmlResults))
                {
                    throw;
                }

                return xmlResults;
            }
            finally
            {
                dbTransaction.Dispose();
            }

            return xmlResults;
        }

        SqlCommand MssqlCreateCommand(SqlConnection connection, int? commandTimeout, List<SqlParameter> parameters)
        {
            var cmd = connection.CreateCommand();
            foreach (var item in parameters)
            {
                cmd.Parameters.Add(item);
            }

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = ProcedureName;
            if (commandTimeout != null)
            {
                if (commandTimeout <= 0)
                {
                    if (commandTimeout == 0)
                    {
                        Dev2Logger.Warn("mssql command timeout is set to wait indefinitely", GlobalConstants.WarewolfWarn);
                    }
                    else
                    {
                        Dev2Logger.Warn("mssql command timeout is set below 0", GlobalConstants.WarewolfWarn);
                    }
                }

                cmd.CommandTimeout = commandTimeout.Value;
            }

            return cmd;
        }

        bool MySqlExecution(ErrorResultTO errors, int update)
        {
            try
            {
                var parameters = GetMySqlParameters(Inputs);
                using (var server = SetupMySqlServer(errors))
                {
                    if (parameters != null)
                    {
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName, Source.DatabaseName)))
                        {
                            TranslateDataTableToEnvironment(dataSet, DataObj.Environment, update);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                DataObj.StateNotifier?.LogAdditionalDetail(new
                    {
                        Exception = ex,
                    },
                    nameof(MySqlExecution)
                );
            }

            return false;
        }

        List<SqlParameter> GetSqlParameters()
        {
            var sqlParameters = new List<SqlParameter>();

            if (Inputs.Count > 0)
            {
                foreach (var parameter in Inputs)
                {
                    sqlParameters.Add(parameter.EmptyIsNull &&
                                      (parameter.Value == null ||
                                       string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                        ? new SqlParameter($"@{parameter.Name}", DBNull.Value)
                        : new SqlParameter($"@{parameter.Name}", parameter.Value));
                }
            }

            return sqlParameters;
        }

        static List<MySqlParameter> GetMySqlParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<MySqlParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    var parameterName = parameter.Name.Replace("`", "");
                    sqlParameters.Add(parameter.EmptyIsNull &&
                                      (parameter.Value == null ||
                                       string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                        ? new MySqlParameter($"@{parameterName}", DBNull.Value)
                        : new MySqlParameter($"@{parameterName}", parameter.Value));
                }
            }

            return sqlParameters;
        }

        OracleServer SetupOracleServer(ErrorResultTO errors)
        {
            var server = new OracleServer();
            if (CommandTimeout != null)
            {
                server.CommandTimeout = CommandTimeout.Value;
            }

            try
            {
                var connected = server.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString), GlobalConstants.WarewolfError);
                }

                return server;
            }
            catch (OracleException oex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(oex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return server;
        }

        bool OracleExecution(ErrorResultTO errors, int update)
        {
            try
            {
                var parameters = GetOracleParameters(Inputs);
                using (var server = SetupOracleServer(errors))
                {
                    if (parameters != null)
                    {
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName, Source.DatabaseName)))

                        {
                            TranslateDataTableToEnvironment(dataSet, DataObj.Environment, update);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("Oracle Error:", ex, GlobalConstants.WarewolfError);
                Dev2Logger.Error("Oracle Error:", ex.StackTrace);
                errors.AddError($"Oracle Error: {ex.Message}");
            }

            return false;
        }

        static List<OracleParameter> GetOracleParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<OracleParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    var dbDataParameter = new OracleParameter($"@{parameter.Name}", parameter.Value);
                    if (parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        dbDataParameter.Value = DBNull.Value;
                    }

                    sqlParameters.Add(dbDataParameter);
                }
            }

            return sqlParameters;
        }

        ODBCServer SetupOdbcServer(ErrorResultTO errors)
        {
            var server = new ODBCServer();
            if (CommandTimeout != null)
            {
                server.CommandTimeout = CommandTimeout.Value;
            }

            try
            {
                var connected = server.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString), GlobalConstants.WarewolfError);
                }

                return server;
            }
            catch (OdbcException oex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(oex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return server;
        }

        bool OdbcExecution(ErrorResultTO errors, int update)
        {
            try
            {
                var parameters = GetOdbcParameters(Inputs);
                using (var server = SetupOdbcServer(errors))
                {
                    if (parameters != null)
                    {
                        var xmlData = server.FetchXmlData();
                        if (xmlData.Rows[0]["ReadForXml"].ToString() != "Error")
                        {
                            TranslateDataTableToEnvironment(xmlData, DataObj.Environment, update);
                            return true;
                        }
                        else
                        {
                            using (var dataSet = server.FetchDataTable())
                            {
                                TranslateDataTableToEnvironment(dataSet, DataObj.Environment, update);
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("ODBC Error:", ex, GlobalConstants.WarewolfError);
                Dev2Logger.Error("ODBC Error:", ex.StackTrace);
                errors.AddError($"ODBC Error: {ex.Message}");
            }

            return false;
        }

        public string OdbcMethod(string command) => ODBCParameterIterators(0, command);

        static List<OdbcParameter> GetOdbcParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<OdbcParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    sqlParameters.Add(parameter.EmptyIsNull &&
                                      (parameter.Value == null ||
                                       string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0)
                        ? new OdbcParameter($"@{parameter.Name}", DBNull.Value)
                        : new OdbcParameter($"@{parameter.Name}", parameter.Value));
                }
            }

            return sqlParameters;
        }

        PostgreServer SetupPostgreServer(ErrorResultTO errors)
        {
            var server = new PostgreServer();
            if (CommandTimeout != null)
            {
                server.CommandTimeout = CommandTimeout.Value;
            }

            try
            {
                var connected = server.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString), GlobalConstants.WarewolfError);
                }

                return server;
            }
            catch (NpgsqlException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(ex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return server;
        }

        bool PostgreSqlExecution(ErrorResultTO errors, int update)
        {
            try
            {
                var parameters = GetPostgreSqlParameters(Inputs);
                using (var server = SetupPostgreServer(errors))
                {
                    if (parameters != null)
                    {
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName)))

                        {
                            TranslateDataTableToEnvironment(dataSet, DataObj.Environment, update);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("PostgreSql Error:", ex, GlobalConstants.WarewolfError);
                Dev2Logger.Error("PostgreSql Error:", ex.StackTrace);
                errors.AddError($"PostgreSql Error: {ex.Message}");
            }

            return false;
        }

        static List<NpgsqlParameter> GetPostgreSqlParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<NpgsqlParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    if (!string.IsNullOrEmpty(parameter.Name))
                    {
                        sqlParameters.Add(parameter.EmptyIsNull &&
                                          (parameter.Value == null ||
                                           string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) ==
                                           0)
                            ? new NpgsqlParameter($"@{parameter.Name}", DBNull.Value)
                            : new NpgsqlParameter($"@{parameter.Name}", parameter.Value));
                    }
                }
            }

            return sqlParameters;
        }

        public void Dispose()
        {
        }

        SqliteServer SetupSqlite(ErrorResultTO errors)
        {
            var server = new SqliteServer();
            if (CommandTimeout != null)
            {
                server.CommandTimeout = CommandTimeout.Value;
            }

            try
            {
                server.Connect(Source.ConnectionString);
                return server;
            }
            catch (SQLiteException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(ex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            return server;
        }

        bool SqliteExecution(ErrorResultTO errors, int update)
        {
            var startTime = Stopwatch.StartNew();
            try
            {
                using (var server = SetupSqlite(errors))
                {
                    var cmd = server.CreateCommand();
                    cmd.CommandText = SqlQuery;
                    cmd.ExecuteScalar();
                    using (var dataSet = server.FetchDataSet(cmd))
                    {
                        Dev2Logger.Info("Time taken to create DB " + SqlQuery + ":" + startTime.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                        var startTime1 = Stopwatch.StartNew();
                        long size = 0;
                        using (Stream s = new MemoryStream())
                        {
                            var formatter = new BinaryFormatter();
                            formatter.Serialize(s, dataSet);
                            size = s.Length;
                        }

                        TranslateDataSetToEnvironment(dataSet, DataObj.Environment, update);
                        Dev2Logger.Info("Time taken to TranslateDataSetToEnvironment ( Size: " + size + " ) :" + startTime1.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                        return true;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(ex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString(), GlobalConstants.WarewolfError);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SQLite Error:", ex, GlobalConstants.WarewolfError);
                Dev2Logger.Error("SQLite Error:", ex.StackTrace);
                errors.AddError($"SQLite Error: {ex.Message}");
            }

            return false;
        }
    }
}