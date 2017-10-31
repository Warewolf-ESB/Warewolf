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
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Npgsql;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using System.Transactions;
using System.Diagnostics;

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution : ServiceExecutionAbstract<DbService, DbSource>, IDisposable
    {
        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true)
        {
        }

        public string ProcedureName { private get; set; }



        private MySqlServer SetupMySqlServer(ErrorResultTO errors)
        {
            var server = new MySqlServer();
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
                            SqlExecution(invokeErrors, update);
                            ErrorResult.MergeErrors(invokeErrors);
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
                        ErrorResult.MergeErrors(invokeErrors);
                        return result;
                    }

                case enSourceType.Oracle:
                    {
                        object result = OracleExecution(invokeErrors, update);
                        ErrorResult.MergeErrors(invokeErrors);
                        return result;
                    }

                case enSourceType.ODBC:
                    {
                        object result = ODBCExecution(invokeErrors, update);
                        ErrorResult.MergeErrors(invokeErrors);
                        return result;
                    }
                case enSourceType.PostgreSQL:
                    {
                        object result = PostgreSqlExecution(invokeErrors, update);
                        ErrorResult.MergeErrors(invokeErrors);
                        return result;
                    }

                case enSourceType.WebService:
                    break;
                case enSourceType.DynamicService:
                    break;
                case enSourceType.ManagementDynamicService:
                    break;
                case enSourceType.PluginSource:
                    break;
                case enSourceType.Unknown:
                    break;
                case enSourceType.Dev2Server:
                    break;
                case enSourceType.EmailSource:
                    break;
                case enSourceType.WebSource:
                    break;
                case enSourceType.OauthSource:
                    break;
                case enSourceType.SharepointServerSource:
                    break;
                case enSourceType.RabbitMQSource:
                    break;
                case enSourceType.ExchangeSource:
                    break;
                case enSourceType.WcfSource:
                    break;
                case enSourceType.ComPluginSource:
                    break;
                default:
                    break;
            }
            return null;
        }

        private void TranslateDataTableToEnvironment(DataTable executeService, IExecutionEnvironment environment, int update)
        {
            var started = true;
            if (executeService != null && Outputs != null && Outputs.Count != 0)
            {
                if (executeService.Rows != null)
                {

                    try
                    {
                        var rowIdx = 1;

                        foreach (DataRow row in executeService.Rows)
                        {
                            foreach (var serviceOutputMapping in Outputs)
                            {
                                if (!string.IsNullOrEmpty(serviceOutputMapping?.MappedTo))
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
                                    if (rsType == enRecordsetIndexType.Star && started)
                                    {
                                        rowIdx = 1;
                                        started = false;
                                    }
                                    if (rsType == enRecordsetIndexType.Numeric)
                                    {
                                        rowIdx = int.Parse(rowIndex);
                                    }
                                    if (!executeService.Columns.Contains(serviceOutputMapping.MappedFrom))
                                    {
                                        continue;
                                    }
                                    var value = row[serviceOutputMapping.MappedFrom];
                                    if (update != 0)
                                    {
                                        rowIdx = update;
                                    }
                                    var displayExpression = DataListUtil.ReplaceRecordsetBlankWithIndex(DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo), rowIdx);
                                    if (rsType == enRecordsetIndexType.Star)
                                    {
                                        displayExpression = DataListUtil.ReplaceStarWithFixedIndex(displayExpression, rowIdx);
                                    }
                                    environment.Assign(displayExpression, value.ToString(), update);
                                }
                            }
                            rowIdx++;
                        }
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }
                }
            }
        }
        private void SqlExecution(ErrorResultTO errors, int update)
        {          
            ISqlConnection connection = new SqlConnectionWrapper(Source.ConnectionString);
            var startTime = Stopwatch.StartNew();
            var cmd = connection.CreateCommand();
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    var parameters = GetSqlParameters();
                    foreach (var item in parameters)
                    {
                        cmd.Parameters.Add(item);
                    }
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = ProcedureName;
                    connection.Open();
                    var reader = cmd.ExecuteReader();
                    var table = new DataTable();
                    table.Load(reader);
                    

                    // The Complete method commits the transaction. If an exception has been thrown,
                    // Complete is not  called and the transaction is rolled back.
                    Dev2Logger.Info("Time taken to process proc " + ProcedureName + ":" + startTime.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                    scope.Complete();
                    var startTime1 = Stopwatch.StartNew();
                    TranslateDataTableToEnvironment(table, DataObj.Environment, update);
                    Dev2Logger.Info("Time taken to TranslateDataTableToEnvironment " + ProcedureName + ":" + startTime1.Elapsed.Milliseconds + " Milliseconds", DataObj.ExecutionID.ToString());
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SQL Error:", ex, GlobalConstants.WarewolfError);
                errors.AddError($"SQL Error: {ex.StackTrace}");
            }
            finally
            {
                cmd.Dispose();
                connection.Dispose();
            }
        }

        private bool MySqlExecution(ErrorResultTO errors, int update)
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
            }
            return false;
        }

        private List<SqlParameter> GetSqlParameters()
        {
            var sqlParameters = new List<SqlParameter>();

            if (Inputs.Count > 0)
            {
                foreach (var parameter in Inputs)
                {
                    sqlParameters.Add(parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0) ? new SqlParameter($"@{parameter.Name}", DBNull.Value) : new SqlParameter($"@{parameter.Name}", parameter.Value));
                }
            }
            return sqlParameters;
        }

        private static List<MySqlParameter> GetMySqlParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<MySqlParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    var parameterName = parameter.Name.Replace("`", "");
                    sqlParameters.Add(parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0) ? new MySqlParameter($"@{parameterName}", DBNull.Value) : new MySqlParameter($"@{parameterName}", parameter.Value));
                }
            }
            return sqlParameters;
        }

        private OracleServer SetupOracleServer(ErrorResultTO errors)
        {
            var server = new OracleServer();
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

        private bool OracleExecution(ErrorResultTO errors, int update)
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
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return false;
        }

        private static List<OracleParameter> GetOracleParameters(ICollection<IServiceInput> methodParameters)
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

        private ODBCServer SetupODBCServer(ErrorResultTO errors)
        {
            var server = new ODBCServer();
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

        private bool ODBCExecution(ErrorResultTO errors, int update)
        {
            try
            {

                var parameters = GetODBCParameters(Inputs);
                using (var server = SetupODBCServer(errors))
                {

                    if (parameters != null)
                    {

                        using (var dataSet = server.FetchDataTable())

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
            }
            return false;
        }
        public string ODBCMethod(string command)
        {
            return ODBCParameterIterators(0, command);
        }
        private static List<OdbcParameter> GetODBCParameters(ICollection<IServiceInput> methodParameters)
        {
            var sqlParameters = new List<OdbcParameter>();

            if (methodParameters.Count > 0)
            {
                foreach (var parameter in methodParameters)
                {
                    sqlParameters.Add(parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0) ? new OdbcParameter($"@{parameter.Name}", DBNull.Value) : new OdbcParameter($"@{parameter.Name}", parameter.Value));
                }
            }
            return sqlParameters;
        }

        private PostgreServer SetupPostgreServer(ErrorResultTO errors)
        {
            var server = new PostgreServer();
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

        private bool PostgreSqlExecution(ErrorResultTO errors, int update)
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
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
            return false;
        }

        private static List<NpgsqlParameter> GetPostgreSqlParameters(ICollection<IServiceInput> methodParameters)
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
                             0) ? new NpgsqlParameter($"@{parameter.Name}", DBNull.Value) : new NpgsqlParameter($"@{parameter.Name}", parameter.Value));
                    }
                }
            }
            return sqlParameters;
        }

        public void Dispose()
        {
        }
    }
}
