#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Runtime.ServiceModel.Esb.Brokers
{
    public abstract class AbstractDatabaseBroker<TDbServer>
        where TDbServer : class, IDbServer, new()
    {
        public int? CommandTimeout { get; set; }

        #region TheCache

#pragma warning disable S2743 // Static fields should not be used in generic types
        public static ConcurrentDictionary<string, ServiceMethodList> TheCache = new ConcurrentDictionary<string, ServiceMethodList>();
#pragma warning restore S2743 // Static fields should not be used in generic types

        #endregion

        public virtual List<string> GetDatabases(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            using (var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                return server.FetchDatabases();
            }
        }

        public virtual ServiceMethodList GetServiceMethods(DbSource dbSource)
        {
            VerifyArgument.IsNotNull("dbSource", dbSource);
            ServiceMethodList cacheResult;
            if (!dbSource.ReloadActions && GetCachedResult(dbSource, out cacheResult))
            {
                return cacheResult;
            }


            var serviceMethods = new ServiceMethodList();
            
            Func<IDbCommand, IList<IDbDataParameter>, string, string, bool> procedureFunc = (command, parameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };
            
            Func<IDbCommand, IList<IDbDataParameter>, string, string, bool> functionFunc = (command, parameters, helpText, executeAction) =>
            {
                var serviceMethod = CreateServiceMethod(command, parameters, helpText, executeAction);
                serviceMethods.Add(serviceMethod);
                return true;
            };
            
            using (var server = CreateDbServer(dbSource))
            {
                server.Connect(dbSource.ConnectionString);
                server.FetchStoredProcedures(procedureFunc, functionFunc);
            }
            TheCache.AddOrUpdate(dbSource.ConnectionString, serviceMethods, (s, list) => serviceMethods);
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
            using (var server = CreateDbServer(dbService.Source as DbSource))
            {
                server.Connect(((DbSource)dbService.Source).ConnectionString);
                server.BeginTransaction();
                try
                {
                    var command = CommandFromServiceMethod(server, dbService.Method);
                    var dataTable = server.FetchDataTable(command);
                    
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
		public virtual IOutputDescription TestSqliteService(SqliteDBService dbService)
		{
			VerifyArgument.IsNotNull("SqliteDBService", dbService);
			VerifyArgument.IsNotNull("SqliteDBService.Source", dbService.Source);

			IOutputDescription result;
			using (var server = CreateSqliteDbServer(dbService.Source as SqliteDBSource))
			{
				server.Connect(((SqliteDBSource)dbService.Source).ConnectionString);
				server.BeginTransaction();
				try
				{
					var command = CommandFromServiceMethod(server, dbService.Method);
					var dataTable = server.FetchDataTable(command);

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

		protected virtual TDbServer CreateDbServer(DbSource dbSource) => new TDbServer();
		protected virtual TDbServer CreateSqliteDbServer(SqliteDBSource dbSource) => new TDbServer();
		protected virtual string NormalizeXmlPayload(string payload) => payload.Replace("&lt;", "<").Replace("&gt;", ">");

        static ServiceMethod CreateServiceMethod(IDbCommand command, IEnumerable<IDataParameter> parameters, string sourceCode, string executeAction) => new ServiceMethod(command.CommandText, sourceCode, parameters.Select(MethodParameterFromDataParameter), null, null, executeAction);

        protected static MethodParameter MethodParameterFromDataParameter(IDataParameter parameter) => new MethodParameter
        {
            Name = parameter.ParameterName.Replace("@", "")
        };

        protected IDbCommand CommandFromServiceMethod(TDbServer server, ServiceMethod serviceMethod)
        {
            var command = server.CreateCommand();

            command.CommandText = serviceMethod.ExecuteAction;
            command.CommandType = serviceMethod.ExecuteAction?.Contains("select") ?? true ? CommandType.Text : CommandType.StoredProcedure;
            if (CommandTimeout != null)
            {
                command.CommandTimeout = CommandTimeout.Value;
            }
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
