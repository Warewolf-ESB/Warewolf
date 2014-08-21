using System;
using System.Xml.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    // BUG 9500 - 2013.05.31 - TWR : created
    public class PluginServices : Services
    {
        #region CTOR

        public PluginServices()
        {
        }

        public PluginServices(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
            : base(resourceCatalog, authorizationService)
        {
        }

        #endregion

        #region DeserializeService

        protected override Service DeserializeService(string args)
        {
            return JsonConvert.DeserializeObject<PluginService>(args);
        }

        protected override Service DeserializeService(XElement xml, ResourceType resourceType)
        {
            return xml == null ? new PluginService() : new PluginService(xml);
        }

        #endregion

        #region Test

        // POST: Service/PluginServices/Test
        public RecordsetList Test(string args, Guid workspaceId, Guid dataListId)
        {
            try
            {
                var service = JsonConvert.DeserializeObject<PluginService>(args);
                return FetchRecordset(service, true);
            }
            catch(Exception ex)
            {
                RaiseError(ex);
                return new RecordsetList { new Recordset { HasErrors = true, ErrorMessage = ex.Message } };
            }
        }

        #endregion

        #region Namespaces

        // POST: Service/PluginServices/Namespaces
        public virtual NamespaceList Namespaces(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new NamespaceList();
            try
            {
                var pluginSource = JsonConvert.DeserializeObject<PluginSource>(args);
                if(pluginSource != null)
                {
                    var broker = new PluginBroker();
                    return broker.GetNamespaces(pluginSource);
                }
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Methods

        // POST: Service/PluginServices/Methods
        public ServiceMethodList Methods(string args, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceMethodList();
            try
            {
                // BUG 9500 - 2013.05.31 - TWR : changed to use PluginService as args 
                var service = JsonConvert.DeserializeObject<PluginService>(args);
                var broker = new PluginBroker();
                result = broker.GetMethods(((PluginSource)service.Source).AssemblyLocation, ((PluginSource)service.Source).AssemblyName, service.Namespace);
                return result;
            }
            catch(Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion
    }
}
