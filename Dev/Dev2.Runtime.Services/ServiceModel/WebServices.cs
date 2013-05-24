using System;
using System.Linq;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel
{
    // PBI 1220 - 2013.05.20 - TWR - Created
    public class WebServices : ExceptionManager
    {
        readonly IResourceCatalog _resourceCatalog;

        #region CTOR

        public WebServices()
            : this(ResourceCatalog.Instance)
        {
        }
//
//        public RecordsetList PluginTest(string args, Guid workspaceID, Guid dataListID)
//        {
//            try
//            {
//                var service = JsonConvert.DeserializeObject<WebServices>(args);
//
//                if (string.IsNullOrEmpty(service.Recordset.Name))
//                {
//                    service.Recordset.Name = service.Method.Name;
//                }
//
//                var addFields = service.Recordset.Fields.Count == 0;
//                if (addFields)
//                {
//                    service.Recordset.Fields.Clear();
//                }
//                service.Recordset.Records.Clear();
//                return FetchRecordset(service, addFields);
//            }
//            catch (Exception ex)
//            {
//                RaiseError(ex);
//                return new RecordsetList { new Recordset { HasErrors = true, ErrorMessage = ex.Message } };
//            }
//        }

        public WebServices(IResourceCatalog resourceCatalog)
        {
            if(resourceCatalog == null)
            {
                throw new ArgumentNullException("resourceCatalog");
            }
            _resourceCatalog = resourceCatalog;
        }

        #endregion

        #region Get

        // POST: Service/WebServices/Sources
        public ResourceList Sources(string args, Guid workspaceID, Guid dataListID)
        {
            var result = new ResourceList();
            try
            {
                //var sources = _resourceCatalog.GetResources(workspaceID, ResourceType.WebSource);
                //result.AddRange(sources.Cast<WebSource>());
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