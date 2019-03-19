#pragma warning disable
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
        }

        #endregion

        #region DbTest

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
        public Recordset SqliteDbTest(SqliteDBService args, Guid workspaceId, Guid dataListId)
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
        public virtual Recordset FetchRecordset(SqliteDBService dbService, bool addFields)
        {

            if (dbService == null)
            {
                throw new ArgumentNullException(nameof(dbService));
            }
            if (dbService.Source is SqliteDBSource source)
            {


                var broker = new SqliteDatabaseBroker();
                var outputDescription = broker.TestSqliteService(dbService);

                if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                {
                    throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                }

                dbService.Recordset.Fields.Clear();

                var smh = new ServiceMappingHelper();

                smh.SqliteMapDbOutputs(outputDescription, ref dbService, addFields);

                return dbService.Recordset;

            }
            return null;
        }
        public virtual Recordset FetchRecordset(DbService dbService, bool addFields)
        {

            if (dbService == null)
            {
                throw new ArgumentNullException(nameof(dbService));
            }
            if (dbService.Source is DbSource source)
            {
                return FetchDbSourceRecordset(ref dbService, addFields, source);
            }
            return null;
        }

        private Recordset FetchDbSourceRecordset(ref DbService dbService, bool addFields, DbSource source)
        {
            switch (source.ServerType)
            {
                case enSourceType.SqlDatabase:
                    {
                        var broker = new SqlDatabaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }
                        if (dbService.Recordset != null)
                        {
                            dbService.Recordset.Name = dbService.Method.ExecuteAction;
                            if (dbService.Recordset.Name != null)
                            {
                                dbService.Recordset.Name = dbService.Recordset.Name.Replace(".", "_");
                            }
                            dbService.Recordset.Fields.Clear();

                            var smh = new ServiceMappingHelper();
                            smh.MapDbOutputs(outputDescription, ref dbService, addFields);
                        }
                        return dbService.Recordset;
                    }

                case enSourceType.MySqlDatabase:
                    {
                        var broker = new MySqlDatabaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }

                        dbService.Recordset.Fields.Clear();

                        var smh = new ServiceMappingHelper();

                        smh.MySqlMapDbOutputs(outputDescription, ref dbService, addFields);

                        return dbService.Recordset;
                    }
                case enSourceType.SQLiteDatabase:
                    {

                        var broker = new SqliteDatabaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }

                        dbService.Recordset.Fields.Clear();

                        var smh = new ServiceMappingHelper();

                        smh.MySqlMapDbOutputs(outputDescription, ref dbService, addFields);

                        return dbService.Recordset;

                    }
                case enSourceType.PostgreSQL:
                    {
                        var broker = new PostgreSqlDataBaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }

                        dbService.Recordset.Fields.Clear();

                        var smh = new ServiceMappingHelper();

                        smh.MySqlMapDbOutputs(outputDescription, ref dbService, addFields);

                        return dbService.Recordset;
                    }
                case enSourceType.Oracle:
                    {
                        var broker = new OracleDatabaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }

                        dbService.Recordset.Fields.Clear();

                        var smh = new ServiceMappingHelper();

                        smh.MapDbOutputs(outputDescription, ref dbService, addFields);

                        return dbService.Recordset;
                    }
                case enSourceType.ODBC:
                    {
                        var broker = new ODBCDatabaseBroker
                        {
                            CommandTimeout = dbService.CommandTimeout
                        };
                        var outputDescription = broker.TestService(dbService);

                        if (outputDescription?.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
                        {
                            throw new Exception(ErrorResource.ErrorRetrievingShapeFromServiceOutput);
                        }

                        dbService.Recordset.Fields.Clear();

                        var smh = new ServiceMappingHelper();

                        smh.MapDbOutputs(outputDescription, ref dbService, addFields);
                        dbService.Recordset.Name = @"Unnamed";
                        return dbService.Recordset;
                    }
                default: return null;

            }
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
                    FetchRecordsetFields_PluginService(dataSourceShape, recordset);
                }
            }
            return recSet;
        }

        private static void FetchRecordsetFields_PluginService(Common.Interfaces.Core.Graph.IDataSourceShape dataSourceShape, Recordset recordset)
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
                    FetchRecordsetFields_ComPluginService(dataSourceShape, recordset);
                }
            }
            return recSet;
        }

        private static void FetchRecordsetFields_ComPluginService(Common.Interfaces.Core.Graph.IDataSourceShape dataSourceShape, Recordset recordset)
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
                    FetchRecordsetFields_WcfService(dataSourceShape, recordset);
                }
            }
            return recSet;
        }

        private static void FetchRecordsetFields_WcfService(Common.Interfaces.Core.Graph.IDataSourceShape dataSourceShape, Recordset recordset)
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

        #endregion

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
                case enSourceType.SQLiteDatabase:
                    {
                        var broker = new SqliteDatabaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
                default:
                    {
                        var broker = new SqlDatabaseBroker();
                        return broker.GetServiceMethods(dbSource);
                    }
            }
        }

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
    }
}
