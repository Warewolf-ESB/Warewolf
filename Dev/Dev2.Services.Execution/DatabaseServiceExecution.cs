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
using Dev2.DataList.Contract;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using MySql.Data.MySqlClient;
using Warewolf.Storage;
using Oracle.ManagedDataAccess.Client;
using System.Data.Odbc;
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Npgsql;
using Warewolf.Resource.Errors;

// ReSharper disable InconsistentNaming

namespace Dev2.Services.Execution
{
    public class DatabaseServiceExecution : ServiceExecutionAbstract<DbService, DbSource>
    {
        public DatabaseServiceExecution(IDSFDataObject dataObj)
            : base(dataObj, true)
        {
            _sqlServer = new SqlServer();
        }




        private IDbServer _sqlServer; 
        public string ProcedureName { private get; set; }

        private void SetupSqlServer(IErrorResultTO errors)
        {
            try
            {
                var connected = _sqlServer.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString));
                }
            }
            catch (SqlException sex)
            {
                var errorMessages = new StringBuilder();
                for (var i = 0; i < sex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + Environment.NewLine +
                                         "Message: " + sex.Errors[i].Message + Environment.NewLine +
                                         "LineNumber: " + sex.Errors[i].LineNumber + Environment.NewLine +
                                         "Source: " + sex.Errors[i].Source + Environment.NewLine +
                                         "Procedure: " + sex.Errors[i].Procedure + Environment.NewLine);
                }
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex);
            }
        }

        private MySqlServer SetupMySqlServer(ErrorResultTO errors)
        {
            var server = new MySqlServer();
            try
            {
                var connected = server.Connect(Source.ConnectionString, CommandType.StoredProcedure, ProcedureName);
                if (!connected)
                {
                    Dev2Logger.Error(string.Format(ErrorResource.FailedToConnectWithConnectionString,
                        Source.ConnectionString));
                }
                return server;
            }
            catch (MySqlException sex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(sex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex);
            }
            return server;
        }

        private void DestroySqlServer()
        {
            if (_sqlServer == null)
            {
                return;
            }
            _sqlServer.Dispose();
            _sqlServer = null;
        }

        public override void BeforeExecution(ErrorResultTO errors)
        {
            if (Source != null && Source.ServerType == enSourceType.SqlDatabase)
            {
                SetupSqlServer(errors);
            }
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
            if (Source != null && Source.ServerType == enSourceType.SqlDatabase)
            {
                DestroySqlServer();
            }
        }

        // ReSharper disable once OptionalParameterHierarchyMismatch
        protected override object ExecuteService(int update, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            var invokeErrors = new ErrorResultTO();
            switch (Source.ServerType)
            {
                case enSourceType.SqlDatabase:
                    {
                        SqlExecution(invokeErrors, update);
                        ErrorResult.MergeErrors(invokeErrors);
                        return Guid.NewGuid();
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

                    var rowIdx = 1;
                    foreach (DataRow row in executeService.Rows)
                    {
                        foreach (var serviceOutputMapping in Outputs)
                        {

                            if (!string.IsNullOrEmpty(serviceOutputMapping.MappedTo))
                            {
                                var rsType = DataListUtil.GetRecordsetIndexType(serviceOutputMapping.MappedTo);
                                var rowIndex = DataListUtil.ExtractIndexRegionFromRecordset(serviceOutputMapping.MappedTo);
                                var rs = serviceOutputMapping.RecordSetName;

                                if(environment.HasRecordSet(rs))
                                {
                                    if(started)
                                    {
                                        rowIdx = environment.GetLength(rs) + 1;
                                        started = false;
                                    }
                                }
                                else
                                {
                                    environment.AssignDataShape(serviceOutputMapping.MappedTo);
                                }
                                if(rsType == enRecordsetIndexType.Star && started)
                                {
                                    rowIdx = 1;
                                    started = false;
                                }
                                if(rsType == enRecordsetIndexType.Numeric)
                                {
                                    rowIdx = int.Parse(rowIndex);
                                }
                                if(!executeService.Columns.Contains(serviceOutputMapping.MappedFrom))
                                {
                                    continue;
                                }
                                var value = row[serviceOutputMapping.MappedFrom];
                                if(update != 0)
                                {
                                    rowIdx = update;
                                }
                                var displayExpression = DataListUtil.ReplaceRecordsetBlankWithIndex(DataListUtil.AddBracketsToValueIfNotExist(serviceOutputMapping.MappedTo), rowIdx);
                                if(rsType == enRecordsetIndexType.Star)
                                {
                                    displayExpression = DataListUtil.ReplaceStarWithFixedIndex(displayExpression, rowIdx);
                                }
                                environment.Assign(displayExpression, value.ToString(), update);
                            }                            
                        }
                        rowIdx++;
                    }
                }
            }
        }

        private void SqlExecution(ErrorResultTO errors, int update)
        {
            try
            {
                if (_sqlServer != null)
                {
                    var parameters = GetSqlParameters();
                    if (parameters != null)
                    {
                        // ReSharper disable CoVariantArrayConversion
                        using (var dataSet = ((SqlServer)_sqlServer).FetchDataTable(parameters.ToArray()))
                        // ReSharper restore CoVariantArrayConversion
                        {
                            TranslateDataTableToEnvironment(dataSet, DataObj.Environment, update);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("SQL Error:", ex);
                errors.AddError($"SQL Error: {ex.Message}");
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
                        // ReSharper disable CoVariantArrayConversion
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName, Source.DatabaseName)))
                        // ReSharper restore CoVariantArrayConversion
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
                    if (parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        sqlParameters.Add(new SqlParameter($"@{parameter.Name}", DBNull.Value));
                    }
                    else
                    {
                        sqlParameters.Add(new SqlParameter($"@{parameter.Name}", parameter.Value));
                    }
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
                    var parameterName = parameter.Name.Replace("`","");
                    if (parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        sqlParameters.Add(new MySqlParameter($"@{parameterName}", DBNull.Value));
                    }
                    else
                    {
                        sqlParameters.Add(new MySqlParameter($"@{parameterName}", parameter.Value));
                    }
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
                        Source.ConnectionString));
                }
                return server;
            }
            catch (OracleException oex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(oex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex);
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
                        // ReSharper disable CoVariantArrayConversion
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName, Source.DatabaseName)))
                        // ReSharper restore CoVariantArrayConversion
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
                        Source.ConnectionString));
                }
                return server;
            }
            catch (OdbcException oex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(oex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex);
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
                        // ReSharper disable CoVariantArrayConversion
                        using (var dataSet = server.FetchDataTable())
                        //// ReSharper restore CoVariantArrayConversion
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
                    if (parameter.EmptyIsNull &&
                        (parameter.Value == null ||
                         string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        sqlParameters.Add(new OdbcParameter($"@{parameter.Name}", DBNull.Value));
                    }
                    else
                    {
                        sqlParameters.Add(new OdbcParameter($"@{parameter.Name}", parameter.Value));
                    }
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
                        Source.ConnectionString));
                }
                return server;
            }
            catch (NpgsqlException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.Append(ex.Message);
                errors.AddError(errorMessages.ToString());
                Dev2Logger.Error(errorMessages.ToString());
            }
            catch (Exception ex)
            {
                errors.AddError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                Dev2Logger.Error(ex);
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
                        // ReSharper disable CoVariantArrayConversion
                        using (var dataSet = server.FetchDataTable(parameters.ToArray(), server.GetProcedureOutParams(ProcedureName)))
                        // ReSharper restore CoVariantArrayConversion
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
                        if (parameter.EmptyIsNull &&
                            (parameter.Value == null ||
                             string.Compare(parameter.Value, string.Empty, StringComparison.InvariantCultureIgnoreCase) ==
                             0))
                        {
                            sqlParameters.Add(new NpgsqlParameter($"@{parameter.Name}", DBNull.Value));
                        }
                        else
                        {
                            sqlParameters.Add(new NpgsqlParameter($"@{parameter.Name}", parameter.Value));
                        }
                    }
                }
            }
            return sqlParameters;
        }
    }

    
}
