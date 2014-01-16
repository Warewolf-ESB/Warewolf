using System;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Runtime.ServiceModel.Utils;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public class WebRequestPoco
    {
        public string ResourceType { get; set; }
        public string ResourceID { get; set; }
    }

    public class Services : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public Services()
            : this(ResourceCatalog.Instance)
        {
        }

        public Services(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion

        #region Get

        // POST: Service/Services/Get
        public Service Get(string args, Guid workspaceID, Guid dataListID)
        {
            ResourceType resourceType = ResourceType.Unknown;
            try
            {
                var webRequestPoco = JsonConvert.DeserializeObject<WebRequestPoco>(args);
                var resourceTypeStr = webRequestPoco.ResourceType;
                resourceType = Resources.ParseResourceType(resourceTypeStr);
                var resourceID = webRequestPoco.ResourceID;
                var xmlStr = _resourceCatalog.GetResourceContents(workspaceID, Guid.Parse(resourceID));

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
                    {
                        return DbService.Create();
                    }
                case ResourceType.PluginService:
                    {
                        return PluginService.Create();
                    }
                case ResourceType.WebService:
                    {
                        return WebService.Create();
                    }
            }
            return DbService.Create();
        }

        #endregion

        #region Save

        // POST: Service/Services/Save
        public string Save(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = DeserializeService(args);
                _resourceCatalog.SaveResource(workspaceID, service);

                if(workspaceID != GlobalConstants.ServerWorkspaceID)
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
        public ServiceMethodList DbMethods(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ServiceMethodList();
            if(!string.IsNullOrEmpty(args))
            {
                try
                {
                    // TODO : Extract IsForceUpdate flag
                    var source = JsonConvert.DeserializeObject<DbSource>(args);
                    var serviceMethods = FetchMethods(source);
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
        public Recordset DbTest(string args, Guid workspaceID, Guid dataListID)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<DbService>(args);

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
            return outputDescription.ToRecordsetList(pluginService.Recordsets);
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
