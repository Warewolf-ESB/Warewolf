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
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Unlimited.Framework.Converters.Graph;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public class PostgreSqlDataBaseBroker : AbstractDatabaseBroker<PostgreServer>
    {
        protected override string NormalizeXmlPayload(string payload)
        {
            var result = new StringBuilder();

            var xDoc = new XmlDocument();
            xDoc.LoadXml(payload);
            var nl = xDoc.SelectNodes("//NewDataSet/Table/*[starts-with(local-name(),'XML_')]");
            var foundXmlFrags = 0;

            if (nl != null)
            {
                foreach (XmlNode n in nl)
                {
                    var tmp = n.InnerXml;
                    result = result.Append(tmp);
                    foundXmlFrags++;
                }
            }

            var res = result.ToString();

            if (foundXmlFrags >= 1)
            {
                res = "<FromXMLPayloads>" + res + "</FromXMLPayloads>";
            }
            else
            {
                if (foundXmlFrags == 0)
                {
                    res = payload;
                }
            }

            return base.NormalizeXmlPayload(res);
        }

        public override List<string> GetDatabases(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            using (var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                return server.FetchDatabases();
            }
        }

        #region Overrides of AbstractDatabaseBroker<MySqlServer>

        public override ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);

            // Check the cache for a value ;)
            ServiceMethodList cacheResult;
            if (!dbSource.ReloadActions && GetCachedResult(dbSource, out cacheResult))
            {
                return cacheResult;
            }

            // else reload actions ;)

            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, IList<IDbDataParameter>, string, string, bool> procedureFunc = (command, parameters, outparameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, outparameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, IList<IDbDataParameter>, string, string, bool> functionFunc = (command, parameters, outparameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, outparameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using (var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                server.FetchStoredProcedures(procedureFunc, functionFunc, false, dbSource.DatabaseName);
            }

            // Add to cache ;)
            TheCache.AddOrUpdate(dbSource.ConnectionString, serviceMethods, (s, list) => serviceMethods);

            return GetCachedResult(dbSource, out cacheResult) ? cacheResult : serviceMethods;
        }

        #region Overrides of AbstractDatabaseBroker<MySqlServer>

        protected override PostgreServer CreateDbServer(DbSource dbSource) => new PostgreServer();

        static ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, IEnumerable<IDataParameter> outParameters, string sourceCode, string executeAction) => new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null, executeAction)
        {
            OutParameters = outParameters.Select(MethodParameterFromDataParameter).ToList()
        };

        #endregion Overrides of AbstractDatabaseBroker<MySqlServer>

        #endregion Overrides of AbstractDatabaseBroker<MySqlServer>

        public override IOutputDescription TestService(DbService dbService)
        {
            VerifyArgument.IsNotNull("dbService", dbService);
            VerifyArgument.IsNotNull("dbService.Source", dbService.Source);

            IOutputDescription result;
            using (var server = CreateDbServer(dbService.Source as DbSource))
            {
                server.Connect(((DbSource)dbService.Source).ConnectionString);
                server.BeginTransaction();
                try
                {
                    var command = CommandFromServiceMethod(server, dbService.Method);

                    // Store the original parameter values before they are replaced
                    var originalParamValues = command.Parameters.Cast<IDbDataParameter>()
                        .ToDictionary(p => p.ParameterName.TrimStart('@'), p => p.Value, StringComparer.OrdinalIgnoreCase);

                    server.GetProcedureInOutParams(command.CommandText, out List<NpgsqlParameter> inParameters, out List<NpgsqlParameter> outParams);

                    var returnType = server.GetProcedureReturnType(command.CommandText);

                    // Configure command using the new unified method
                    ConfigureCommandForExecution(command, inParameters, outParams, originalParamValues, returnType);

                    // Execute based on return type
                    var dataTable = returnType == "<procedure>" ? new DataTable() : server.FetchDataTable(command);

                    result = CreateOutputDescription(dataTable);
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error(ex.Message, GlobalConstants.WarewolfError);
                    throw;
                }
                finally
                {
                    server.RollbackTransaction();
                }
            }

            return result;
        }

        public static Dictionary<string, object> BuildInputParameterDictionary(ICollection<IServiceInput> inputs)
        {
            var originalParamValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (inputs != null)
            {
                foreach (var input in inputs)
                {
                    if (!string.IsNullOrEmpty(input.Name))
                    {
                        var value = input.EmptyIsNull && string.IsNullOrEmpty(input.Value)
                            ? DBNull.Value
                            : (object)input.Value;
                        originalParamValues[input.Name] = value;
                    }
                }
            }
            return originalParamValues;
        }

        public static void ConfigureCommandForExecution(
            IDbCommand command,
            IEnumerable<NpgsqlParameter> inParameters,
            IEnumerable<NpgsqlParameter> outParameters,
            Dictionary<string, object> originalParamValues,
            string returnType)
        {
            command.Parameters.Clear();

            // Add input parameters with preserved values
            AddParametersToCommand(command, inParameters, originalParamValues);

            // Add output parameters
            AddParametersToCommand(command, outParameters, null);

            // Handle different PostgreSQL routine types
            if (returnType == "<procedure>")
            {
                // Procedures use CALL statement
                command.CommandType = CommandType.Text;
                
                var allParams = command.Parameters.Cast<NpgsqlParameter>();
                //var paramList = string.Join(", ", allParams.Select(p => p.ParameterName));
                var paramList = string.Join(", ", allParams.Select(p =>
                    $"{p.ParameterName.TrimStart('@')} => @{p.ParameterName}"));
                command.CommandText = $"CALL {command.CommandText}({paramList})";
            }
            else if (returnType == "<void>")
            {
                // Functions with void return type - use SELECT
                command.CommandType = CommandType.Text;
                
                var inputParams = command.Parameters.Cast<NpgsqlParameter>()
                    .Where(p => p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput);
                
                var paramNames = string.Join(", ", inputParams.Select(p => 
                    $"{p.ParameterName.TrimStart('@')} => @{p.ParameterName}"));
                
                command.CommandText = $"SELECT {command.CommandText}({paramNames})";
            }
            else
            {
                // Functions with a return type - use SELECT * FROM
                TransformCommandForFunction(command);
            }
        }

        private static Dictionary<string, object> BuildParameterValueDictionary(IDbCommand command)
        {
            return command.Parameters.Cast<IDbDataParameter>()
                .ToDictionary(
                    p => p.ParameterName.TrimStart('@'), 
                    p => p.Value, 
                    StringComparer.OrdinalIgnoreCase);
        }

        private static void AddParametersToCommand(
            IDbCommand command, 
            IEnumerable<NpgsqlParameter> parameters, 
            Dictionary<string, object> originalParamValues)
        {
            foreach (var dbDataParameter in parameters)
            {
                var paramValue = GetParameterValue(dbDataParameter, originalParamValues);

                var newParam = new NpgsqlParameter(dbDataParameter.ParameterName, dbDataParameter.NpgsqlDbType)
                {
                    Value = paramValue,
                    Direction = dbDataParameter.Direction
                };
                command.Parameters.Add(newParam);
            }
        }

        private static object GetParameterValue(
            NpgsqlParameter dbDataParameter, 
            Dictionary<string, object> originalParamValues)
        {
            if (originalParamValues != null)
            {
                var paramName = dbDataParameter.ParameterName.TrimStart('@');
                
                if (originalParamValues.TryGetValue(paramName, out var originalValue) && originalValue != null)
                {
                    // Convert string values to appropriate types based on NpgsqlDbType
                    if (originalValue is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
                    {
                        try
                        {
                            switch (dbDataParameter.NpgsqlDbType)
                            {
                                case NpgsqlTypes.NpgsqlDbType.Numeric:
                                case NpgsqlTypes.NpgsqlDbType.Money:
                                    return decimal.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Integer:
                                case NpgsqlTypes.NpgsqlDbType.Oid:
                                    return int.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Bigint:
                                    return long.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Smallint:
                                    return short.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Real:
                                    return float.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Double:
                                    return double.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Boolean:
                                    return bool.Parse(stringValue);
                                
                                case NpgsqlTypes.NpgsqlDbType.Date:
                                case NpgsqlTypes.NpgsqlDbType.Timestamp:
                                case NpgsqlTypes.NpgsqlDbType.TimestampTz:
                                    return DateTime.Parse(stringValue);
                                
                                default:
                                    return originalValue;
                            }
                        }
                        catch
                        {
                            // If conversion fails, return original value and let Npgsql handle the error
                            return originalValue;
                        }
                    }
                    
                    return originalValue;
                }
            }

            return dbDataParameter.Value ?? DBNull.Value;
        }

        private static void TransformCommandForFunction(IDbCommand command)
        {
            command.CommandType = CommandType.Text;
            
            var inputParams = command.Parameters.Cast<NpgsqlParameter>()
                .Where(p => p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput);
            
            var paramNames = string.Join(", ", inputParams.Select(p => 
                $"{p.ParameterName.TrimStart('@')} => @{p.ParameterName}"));
            
            command.CommandText = $"SELECT * FROM {command.CommandText}({paramNames})";
        }

        private static IOutputDescription CreateOutputDescription(DataTable dataTable)
        {
            var result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
            var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
            result.DataSourceShapes.Add(dataSourceShape);

            var dataBrowser = DataBrowserFactory.CreateDataBrowser();
            dataSourceShape.Paths.AddRange(dataBrowser.Map(dataTable));

            return result;
        }
    }
}