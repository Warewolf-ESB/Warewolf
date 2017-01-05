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
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Dev2.Runtime.ServiceModel.Utils;
using Dev2.Services.Security;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ServiceModel
{
    public class Services : ExceptionManager
    {
        protected readonly IResourceCatalog _resourceCatalog;
        readonly IAuthorizationService _authorizationService;

        #region CTOR

        public Services()
            : this(ResourceCatalog.Instance, ServerAuthorizationService.Instance)
        {
        }

        public Services(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
        {
            VerifyArgument.IsNotNull("resourceCatalog", resourceCatalog);
            VerifyArgument.IsNotNull("authorizationService", authorizationService);
            _resourceCatalog = resourceCatalog;
            _authorizationService = authorizationService;
        }

        #endregion

        #region DbTest

        // POST: Service/Services/DbTest
        // POST: Service/Services/DbTest
        public Recordset DbTest(DbService args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var service = args;

                if (string.IsNullOrEmpty(service.Recordset.Name))
                {
                    service.Recordset.Name = service.Method.Name;
                }

                var addFields = service.Recordset.Fields.Count == 0;
                if (addFields)
                {
                    service.Recordset.Fields.Clear();
                }
                service.Recordset.Records.Clear();

                return FetchRecordset(service, addFields);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new Recordset { HasErrors = true, ErrorMessage = ex.Message };
            }
        }
        #endregion

        #region FetchRecordset

        public virtual Recordset FetchRecordset(DbService dbService, bool addFields)
        {

            if (dbService == null)
            {
                throw new ArgumentNullException(nameof(dbService));
            }
            var source = dbService.Source as DbSource;
            if (source != null)
            {
                switch (source.ServerType)
                {
                    case enSourceType.SqlDatabase:
                        {
                            var broker = CreateDatabaseBroker();
                            var outputDescription = broker.TestService(dbService);

                            if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                            {
                                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                            }

                            // Clear out the Recordset.Fields list because the sequence and
                            // number of fields may have changed since the last invocation.
                            //
                            // Create a copy of the Recordset.Fields list before clearing it
                            // so that we don't lose the user-defined aliases.
                            //

                            if (dbService.Recordset != null)
                            {
                                dbService.Recordset.Name = dbService.Method.ExecuteAction;
                                if (dbService.Recordset.Name != null)
                                {
                                    dbService.Recordset.Name = dbService.Recordset.Name.Replace(".", "_");
                                }
                                dbService.Recordset.Fields.Clear();

                                ServiceMappingHelper smh = new ServiceMappingHelper();
                                smh.MapDbOutputs(outputDescription, ref dbService, addFields);
                            }
                            return dbService.Recordset;
                        }

                    case enSourceType.MySqlDatabase:
                        {

                            var broker = new MySqlDatabaseBroker();
                            var outputDescription = broker.TestService(dbService);

                            if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                            {
                                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                            }

                            dbService.Recordset.Fields.Clear();

                            ServiceMappingHelper smh = new ServiceMappingHelper();

                            smh.MySqlMapDbOutputs(outputDescription, ref dbService, addFields);

                            return dbService.Recordset;

                        }
                    case enSourceType.PostgreSQL:
                        {
                            var broker = new PostgreSqlDataBaseBroker();
                            var outputDescription = broker.TestService(dbService);

                            if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                            {
                                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                            }

                            dbService.Recordset.Fields.Clear();

                            ServiceMappingHelper smh = new ServiceMappingHelper();

                            smh.MySqlMapDbOutputs(outputDescription, ref dbService, addFields);

                            return dbService.Recordset;
                        }
                    case enSourceType.Oracle:
                        {
                            var broker = new OracleDatabaseBroker();
                            var outputDescription = broker.TestService(dbService);

                            if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                            {
                                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                            }

                            dbService.Recordset.Fields.Clear();

                            ServiceMappingHelper smh = new ServiceMappingHelper();

                            smh.MapDbOutputs(outputDescription, ref dbService, addFields);
                            
                            return dbService.Recordset;
                        }
                    case enSourceType.ODBC:
                        {
                            var broker = new ODBCDatabaseBroker();
                            var outputDescription = broker.TestService(dbService);

                            if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                            {
                                throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                            }

                            dbService.Recordset.Fields.Clear();

                            ServiceMappingHelper smh = new ServiceMappingHelper();

                            smh.MapDbOutputs(outputDescription, ref dbService, addFields);
                            dbService.Recordset.Name = @"Unnamed";
                            return dbService.Recordset;
                        }
                    default: return null;

                }
            }
            return null;


            // Clear out the Recordset.Fields list because the sequence and
            // number of fields may have changed since the last invocation.
            //
            // Create a copy of the Recordset.Fields list before clearing it
            // so that we don't lose the user-defined aliases.
            //

        }

        public virtual RecordsetList FetchRecordset(PluginService pluginService, bool addFields)
        {
            if (pluginService == null)
            {
                throw new ArgumentNullException(nameof(pluginService));
            }
            var broker = new PluginBroker();
            var outputDescription = broker.TestPlugin(pluginService);
            var dataSourceShape = outputDescription.DataSourceShapes[0];
            var recSet = outputDescription.ToRecordsetList(pluginService.Recordsets, GlobalConstants.PrimitiveReturnValueTag);
            if (recSet != null)
            {
                foreach (var recordset in recSet)
                {
                    foreach (var field in recordset.Fields)
                    {
                        if (string.IsNullOrEmpty(field.Name))
                        {
                            continue;
                        }
                        var path = field.Path;
                        var rsAlias = string.IsNullOrEmpty(field.RecordsetAlias) ? "" : field.RecordsetAlias.Replace("()", "");

                        var value = string.Empty;
                        if (!string.IsNullOrEmpty(field.Alias))
                        {
                            value = string.IsNullOrEmpty(rsAlias)
                                        ? $"[[{field.Alias}]]"
                                : $"[[{rsAlias}().{field.Alias}]]";
                        }

                        if (path != null)
                        {
                            path.OutputExpression = value;
                            var foundPath = dataSourceShape.Paths.FirstOrDefault(path1 => path1.OutputExpression == path.OutputExpression);
                            if (foundPath == null)
                            {
                                dataSourceShape.Paths.Add(path);
                            }
                            else
                            {
                                foundPath.OutputExpression = path.OutputExpression;
                            }                            

                        }
                    }
                }
            }
            return recSet;
        }

        protected virtual RecordsetList FetchRecordset(ComPluginService pluginService, bool addFields)
        {
            if (pluginService == null)
            {
                throw new ArgumentNullException(nameof(pluginService));
            }
            var broker = new ComPluginBroker();
            var outputDescription = broker.TestPlugin(pluginService);
            var dataSourceShape = outputDescription.DataSourceShapes[0];
            var recSet = outputDescription.ToRecordsetList(pluginService.Recordsets, GlobalConstants.PrimitiveReturnValueTag);
            if (recSet != null)
            {
                foreach (var recordset in recSet)
                {
                    foreach (var field in recordset.Fields)
                    {
                        if (string.IsNullOrEmpty(field.Name))
                        {
                            continue;
                        }
                        var path = field.Path;
                        var rsAlias = string.IsNullOrEmpty(field.RecordsetAlias) ? "" : field.RecordsetAlias.Replace("()", "");

                        var value = string.Empty;
                        if (!string.IsNullOrEmpty(field.Alias))
                        {
                            value = string.IsNullOrEmpty(rsAlias)
                                        ? $"[[{field.Alias}]]"
                                : $"[[{rsAlias}().{field.Alias}]]";
                        }

                        if (path != null)
                        {
                            path.OutputExpression = value;
                            var foundPath = dataSourceShape.Paths.FirstOrDefault(path1 => path1.OutputExpression == path.OutputExpression);
                            if (foundPath == null)
                            {
                                dataSourceShape.Paths.Add(path);
                            }
                            else
                            {
                                foundPath.OutputExpression = path.OutputExpression;
                            }

                        }
                    }
                }
            }
            return recSet;
        }

        public virtual RecordsetList FetchRecordset(WebService webService, bool addFields)
        {
            if (webService == null)
            {
                throw new ArgumentNullException(nameof(webService));
            }

            var outputDescription = webService.GetOutputDescription();
            return outputDescription.ToRecordsetList(webService.Recordsets);
        }

        public virtual RecordsetList FetchRecordset(WcfService wcfService)
        {
            if (wcfService == null)
            {
                throw new ArgumentNullException(nameof(wcfService));
            }
            var broker = new WcfSource();
            var outputDescription = broker.ExecuteMethod(wcfService);
            var dataSourceShape = outputDescription.DataSourceShapes[0];
            var recSet = outputDescription.ToRecordsetList(wcfService.Recordsets, GlobalConstants.PrimitiveReturnValueTag);
            if (recSet != null)
            {
                foreach (var recordset in recSet)
                {
                    foreach (var field in recordset.Fields)
                    {
                        if (string.IsNullOrEmpty(field.Name))
                        {
                            continue;
                        }
                        var path = field.Path;
                        var rsAlias = string.IsNullOrEmpty(field.RecordsetAlias) ? "" : field.RecordsetAlias.Replace("()", "");

                        var value = string.Empty;
                        if (!string.IsNullOrEmpty(field.Alias))
                        {
                            value = string.IsNullOrEmpty(rsAlias)
                                        ? $"[[{field.Alias}]]"
                                : $"[[{rsAlias}().{field.Alias}]]";
                        }

                        if (path != null)
                        {
                            path.OutputExpression = value;
                            dataSourceShape.Paths.Add(path);
                        }
                    }
                }
            }
            return recSet;
        }

        #endregion

        #region FetchMethods

        public virtual ServiceMethodList FetchMethods(DbSource dbSource)
        {
            switch (dbSource.ServerType)
            {
                case enSourceType.MySqlDatabase:
                    {
                        var broker = new MySqlDatabaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
                case enSourceType.PostgreSQL:
                    {
                        var broker = new PostgreSqlDataBaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
                case enSourceType.Oracle:
                    {
                        var broker = new OracleDatabaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
                default:
                    {
                        var broker = CreateDatabaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
            }

        }

        #endregion



        protected virtual SqlDatabaseBroker CreateDatabaseBroker()
        {

            return new SqlDatabaseBroker();
        }

        #region DeserializeService

        #endregion

        #region WcfTest

        public RecordsetList WcfTest(WcfService args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var service = args;

                return FetchRecordset(service);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return new RecordsetList { new Recordset { HasErrors = true, ErrorMessage = ex.Message } };
            }
        }

        #endregion WcfTest
    }
}
