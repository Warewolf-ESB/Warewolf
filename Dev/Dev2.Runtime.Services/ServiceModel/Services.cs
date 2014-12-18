
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Services.Security;
using Dev2.Communication;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Runtime.ServiceModel.Utils;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class WebRequestPoco
    {
        public string ResourceType { get; set; }
        public string ResourceId { get; set; }
    }

    public class Services : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;
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

        #region Get

        // POST: Service/Services/Get
        public Service Get(string args, Guid workspaceId, Guid dataListId)
        {
            ResourceType resourceType = ResourceType.Unknown;
            try
            {
                var webRequestPoco = JsonConvert.DeserializeObject<WebRequestPoco>(args);
                var resourceTypeStr = webRequestPoco.ResourceType;
                resourceType = Resources.ParseResourceType(resourceTypeStr);
                var resourceId = webRequestPoco.ResourceId;
                var xmlStr = _resourceCatalog.GetResourceContents(workspaceId, Guid.Parse(resourceId));

                if(xmlStr != null && xmlStr.Length != 0)
                {
                    return DeserializeService(xmlStr.ToXElement(), resourceType);
                }
                return GetDefaultService(resourceType);

            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return GetDefaultService(resourceType);
            }
        }

        static Service GetDefaultService(ResourceType resourceType)
        {
            switch(resourceType)
            {
                case ResourceType.DbService:
                    return DbService.Create();
                case ResourceType.PluginService:
                    return PluginService.Create();
                case ResourceType.WebService:
                    return WebService.Create();
            }
            return DbService.Create();
        }

        #endregion

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var service = DeserializeService(args);
                _resourceCatalog.SaveResource(workspaceId, service);

                if(workspaceId != GlobalConstants.ServerWorkspaceID)
                {
                    _resourceCatalog.SaveResource(GlobalConstants.ServerWorkspaceID, service);
                }

                return service.ToString();
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new ValidationResult { IsValid = false, ErrorMessage = ex.Message }.ToString();
            }
        }

        #endregion

        #region DbMethods

        // POST: Service/Services/DbMethods
        public ServiceMethodList DbMethods(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceMethodList();
            if(!string.IsNullOrEmpty(args))
            {
                try
                {
                    Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
                    var source = serialiser.Deserialize<DbSource>(args);
                    var actualSource = _resourceCatalog.GetResource<DbSource>(workspaceId, source.ResourceID);
                    actualSource.ReloadActions = source.ReloadActions;
                    var serviceMethods = FetchMethods(actualSource);
                    result.AddRange(serviceMethods);
                }
                catch(Exception ex)
                {
                    RaiseError(ex);
                    result.Add(new ServiceMethod(ex.Message, ex.StackTrace));
                }
            }
            return result;
        }

        #endregion

        #region DbTest

        // POST: Service/Services/DbTest
        public Recordset DbTest(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<DbService>(args);
                service.Source = _resourceCatalog.GetResource<DbSource>(workspaceId, service.Source.ResourceID);
                if(string.IsNullOrEmpty(service.Recordset.Name))
                {
                    service.Recordset.Name = service.Method.Name;
                }

                var addFields = service.Recordset.Fields.Count == 0;
                if(addFields)
                {
                    service.Recordset.Fields.Clear();
                }
                service.Recordset.Records.Clear();

                return FetchRecordset(service, addFields);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new Recordset { HasErrors = true, ErrorMessage = ex.Message };
            }
        }

        #endregion

        #region FetchRecordset

        public virtual Recordset FetchRecordset(DbService dbService, bool addFields)
        {

            if(dbService == null)
            {
                throw new ArgumentNullException("dbService");
            }

            var broker = CreateDatabaseBroker();
            var outputDescription = broker.TestService(dbService);

            if(outputDescription == null || outputDescription.DataSourceShapes == null || outputDescription.DataSourceShapes.Count == 0)
            {
                throw new Exception("Error retrieving shape from service output.");
            }

            // Clear out the Recordset.Fields list because the sequence and
            // number of fields may have changed since the last invocation.
            //
            // Create a copy of the Recordset.Fields list before clearing it
            // so that we don't lose the user-defined aliases.
            //

            dbService.Recordset.Name = dbService.Recordset.Name.Replace(".", "_");
            dbService.Recordset.Fields.Clear();

            ServiceMappingHelper smh = new ServiceMappingHelper();

            smh.MapDbOutputs(outputDescription, ref dbService, addFields);

            return dbService.Recordset;
        }

        public virtual RecordsetList FetchRecordset(PluginService pluginService, bool addFields)
        {
            if(pluginService == null)
            {
                throw new ArgumentNullException("pluginService");
            }
            var broker = new PluginBroker();
            var outputDescription = broker.TestPlugin(pluginService);
            return outputDescription.ToRecordsetList(pluginService.Recordsets, GlobalConstants.PrimitiveReturnValueTag);
        }

        public virtual RecordsetList FetchRecordset(WebService webService, bool addFields)
        {
            if(webService == null)
            {
                throw new ArgumentNullException("webService");
            }

            var outputDescription = webService.GetOutputDescription();
            return outputDescription.ToRecordsetList(webService.Recordsets);
        }

        #endregion

        #region FetchMethods

        public virtual ServiceMethodList FetchMethods(DbSource dbSource)
        {
            var broker = CreateDatabaseBroker();
            return broker.GetServiceMethods(dbSource);
        }

        #endregion

        #region IsReadOnly

        public WebPermission IsReadOnly(string resourceId, Guid workspaceId, Guid dataListId)
        {
            return new WebPermission { IsReadOnly = !_authorizationService.IsAuthorized(AuthorizationContext.Contribute, resourceId) };
        }

        #endregion

        protected virtual SqlDatabaseBroker CreateDatabaseBroker()
        {
            return new SqlDatabaseBroker();
        }

        #region DeserializeService

        protected virtual Service DeserializeService(string args)
        {
            var service = JsonConvert.DeserializeObject<Service>(args);
            switch(service.ResourceType)
            {
                case ResourceType.DbService:
                    return JsonConvert.DeserializeObject<DbService>(args);
            }
            return service;
        }

        protected virtual Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            if(xml != null)
            {
                switch(resourceType)
                {
                    case ResourceType.DbService:
                        return new DbService(xml);
                }
            }
            else
            {
                switch(resourceType)
                {
                    case ResourceType.DbService:
                        return DbService.Create();
                }
            }
            return null;
        }

        #endregion

    }
}
