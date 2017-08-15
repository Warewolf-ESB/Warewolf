using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Runtime.ServiceModel.Esb.Brokers.ComPlugin;
using Dev2.Services.Security;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    public interface IComPluginServices
    {
        RecordsetList Test(string args, out string serializedResult);

        NamespaceList Namespaces(ComPluginSource args, Guid workspaceId, Guid dataListId);

        ServiceMethodList Methods(ComPluginService args, Guid workspaceId, Guid dataListId);
    }
    public class ComPluginServices : Services, IComPluginServices
    {
        #region CTOR

        public ComPluginServices()
        {
        }

        public ComPluginServices(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
            : base(resourceCatalog, authorizationService)
        {
        }

        #endregion

        #region DeserializeService

    
        protected virtual Service DeserializeService(string args)
        {
            return JsonConvert.DeserializeObject<ComPluginService>(args);
        }

    
        protected virtual Service DeserializeService(XElement xml, string resourceType)
        {
            return xml == null ? new ComPluginService() : new ComPluginService(xml);
        }

        #endregion

        #region Test

        // POST: Service/PluginServices/Test
        public RecordsetList Test(string args, out string serializedResult)
        {
            try
            {


                var service = JsonConvert.DeserializeObject<ComPluginService>(args, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
                var fetchRecordset = FetchRecordset(service, true);
                serializedResult = service.SerializedResult;
                return fetchRecordset;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                serializedResult = null;
                return new RecordsetList { new Recordset { HasErrors = true, ErrorMessage = ex.Message } };
            }
        }

        #endregion

        #region Namespaces

        // POST: Service/PluginServices/Namespaces
        public virtual NamespaceList Namespaces(ComPluginSource pluginSource, Guid workspaceId, Guid dataListId)
        {
            var result = new NamespaceList();
            try
            {

                if (pluginSource != null)
                {
                    var broker = new ComPluginBroker();
                    return broker.GetNamespaces(pluginSource);
                }
            }
            catch (BadImageFormatException e)
            {
                RaiseError(e);
                throw;
            }
            catch (Exception ex) when (ex is COMException)
            {
                throw;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion

        #region Methods

        // POST: Service/PluginServices/Methods
        public ServiceMethodList Methods(ComPluginService service, Guid workspaceId, Guid dataListId)
        {
            var result = new ServiceMethodList();
            try
            {
                var broker = new ComPluginBroker();
                var comPluginSource = (ComPluginSource)service.Source;
                result = broker.GetMethods(comPluginSource.ClsId, comPluginSource.Is32Bit);
                return result;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
            return result;
        }

        #endregion
    }
}