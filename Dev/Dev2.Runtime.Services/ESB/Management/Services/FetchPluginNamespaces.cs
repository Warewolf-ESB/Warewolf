using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchPluginNamespaces : IEsbManagementEndpoint
    {


        public string HandlesType()
        {
            return "FetchPluginNameSpaces";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {




                var dbSource = serializer.Deserialize<IPluginSource>(values["source"]);
                // ReSharper disable MaximumChainedReferences
                ServiceModel.PluginServices services = new ServiceModel.PluginServices();
                var src = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, dbSource.Id);
                var methods = services.Namespaces(serializer.Serialize(src), Guid.Empty, Guid.Empty).Select( a=> a as INamespaceItem).ToList()  ;
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
                // ReSharper restore MaximumChainedReferences
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



        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources
        {
            get
            {
                return ResourceCatalog.Instance;
            }

        }
    }
}