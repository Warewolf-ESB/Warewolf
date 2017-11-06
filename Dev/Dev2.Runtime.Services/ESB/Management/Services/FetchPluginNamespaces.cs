using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchPluginNamespaces : DefaultEsbManagementEndpoint
    {
        public override string HandlesType()
        {
            return "FetchPluginNameSpaces";
        }

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var dbSource = serializer.Deserialize<PluginSourceDefinition>(values["source"]);
                var containsKey = values.ContainsKey("fetchJson");
           
                
                PluginServices services = new PluginServices();
                var src = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, dbSource.Id);
                List<INamespaceItem> methods = containsKey ? services.NamespacesWithJsonObjects(src, Guid.Empty, Guid.Empty).Select(a => a as INamespaceItem).ToList() 
                    : services.Namespaces(src, Guid.Empty, Guid.Empty).Select(a => a as INamespaceItem).ToList();
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
                
            }
            catch (Exception e)
            {
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        public override DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}
