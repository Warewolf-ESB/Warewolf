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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Services.Sql;
using Dev2.Data.Util;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Sql;
using Unlimited.Framework.Converters.Graph;
// ReSharper disable UnusedMemberHierarchy.Global
// ReSharper disable UnusedParameter.Global

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public abstract class AbstractDatabaseBroker<TDbServer>
        where TDbServer : class, IDbServer, new()
    {
        #region TheCache

        // ReSharper disable StaticFieldInGenericType
        //
        // This means that the values of 
        //      AbstractDatabaseBroker<DbServer1>.TheCache 
        //      AbstractDatabaseBroker<DbServer2>.TheCache 
        // will have completely different, independent values.
        //
        public static ConcurrentDictionary<string, ServiceMethodList> TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
        //
        // ReSharper restore StaticFieldInGenericType

        #endregion
        
        public virtual List<string> GetDatabases(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            using(var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                return server.FetchDatabases();
            }
        }

        public virtual ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);

            // Check the cache for a value ;)
            ServiceMethodList cacheResult;
            if(!dbSource.ReloadActions)
            {
                if(GetCachedResult(dbSource, out cacheResult))
                {
                    return cacheResult;
                }
            }
            // else reload actions ;)

            var serviceMethods = new ServiceMethodList();

            //
            // Function to handle procedures returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, string, string, bool> procedureFunc = (command, parameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Function to handle functions returned by the data broker
            //
            Func<IDbCommand, IList<IDbDataParameter>, string, string, bool> functionFunc = (command, parameters, helpText,executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };

            //
            // Get stored procedures and functions for this database source
            //
            using(var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                server.FetchStoredProcedures(procedureFunc, functionFunc);
            }

            // Add to cache ;)
            TheCache.AddOrUpdate(dbSource.ConnectionString, serviceMethods,(s, list) => serviceMethods);

            return GetCachedResult(dbSource, out cacheResult) ? cacheResult : serviceMethods;
        }

    
        protected bool GetCachedResult(DbSource dbSource, out ServiceMethodList cacheResult)
        {
            TheCache.TryGetValue(dbSource.ConnectionString, out cacheResult);
            if (cacheResult != null)
            {
                return true;
            }
            return false;
        }

        public virtual IOutputDescription TestService(DbService dbService)
        {
            VerifyArgument.IsNotNull("dbService", dbService);
            VerifyArgument.IsNotNull("dbService.Source", dbService.Source);

            IOutputDescription result;
            using(var server = CreateDbServer(dbService.Source as DbSource))
            {
                server.Connect(((DbSource)dbService.Source).ConnectionString);
                server.BeginTransaction();
                try
                {
                    //
                    // Execute command and normalize XML
                    //
                    var command = CommandFromServiceMethod(server, dbService.Method);
                    var dataTable = server.FetchDataTable(command);
                    
                    //
                    // Map shape of XML
                    //
                    result = OutputDescriptionFactory.CreateOutputDescription(OutputFormats.ShapedXML);
                    var dataSourceShape = DataSourceShapeFactory.CreateDataSourceShape();
                    result.DataSourceShapes.Add(dataSourceShape);

                    var dataBrowser = DataBrowserFactory.CreateDataBrowser();
                    dataSourceShape.Paths.AddRange(dataBrowser.Map(dataTable));
                }
                catch (Exception ex)
                {
                    throw new WarewolfDbException(ex.Message);
                }
                finally
                {
                    server.RollbackTransaction();
                }
            }

            return result;
        }

        protected virtual TDbServer CreateDbServer(DbSource dbSource)
        {
            return new TDbServer();
        }

        protected virtual string NormalizeXmlPayload(string payload)
        {
            //
            // Unescape '<>' characters delimiting
            //
            return payload.Replace("&lt;", "<").Replace("&gt;", ">");
        }

        private static ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, string sourceCode,string executeAction)
        {
            return new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null,executeAction);
        }

        protected static MethodParameter MethodParameterFromDataParameter(IDataParameter parameter)
        {
            return new MethodParameter
            {
                Name = parameter.ParameterName.Replace("@", "")
            };
        }

        protected static IDbCommand CommandFromServiceMethod(TDbServer server, ServiceMethod serviceMethod)
        {
            var command = server.CreateCommand();

            command.CommandText = serviceMethod.ExecuteAction;
            command.CommandType = serviceMethod.ExecuteAction.Contains("select") ? CommandType.Text : CommandType.StoredProcedure;
            if (server.GetType() != typeof(ODBCServer))
            {
                foreach (var methodParameter in serviceMethod.Parameters)
                {
                    var dataParameter = DataParameterFromMethodParameter(command, methodParameter);
                    command.Parameters.Add(dataParameter);
                }
            }
            else
            {
                var newCommandText = serviceMethod.Parameters.Aggregate(command.CommandText ?? string.Empty, (current, parameter) => current.Replace(DataListUtil.AddBracketsToValueIfNotExist(parameter.Name), parameter.Value));
                command.CommandText = newCommandText;
            }
            return command;
        }

        static IDbDataParameter DataParameterFromMethodParameter(IDbCommand command, MethodParameter methodParameter)
        {
            var parameter = command.CreateParameter();

            parameter.ParameterName = $"@{methodParameter.Name.Replace("`", "")}";
            parameter.Value = methodParameter.Value;

            return parameter;
        }

    }
}
