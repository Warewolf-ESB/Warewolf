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
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchComPluginSources : IEsbManagementEndpoint
    {
        public string HandlesType()
        {
            return "FetchComPluginSources";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            // ReSharper disable MaximumChainedReferences
            List<ComPluginSourceDefinition> list = Resources.GetResourceList<ComPluginSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                var res = a as ComPluginSource;
                if (res != null)
                {
                    return new ComPluginSourceDefinition()
                    {
                        Id = res.ResourceID,
                        Name = res.ResourceName,
                        Path = res.ResourcePath,
                        ClsId = res.ClsId,
                        ProgId = res.ProgId,
                        SelectedDll = new DllListing
                        {
                            Name = res.ResourceName,
                            ClsId = res.ClsId,
                            Is32Bit = res.Is32Bit,
                            Children = new IFileListing[0]
                        }
                    };
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage() { HasError = false, Message = serializer.SerializeToBuilder(list) });
            // ReSharper restore MaximumChainedReferences
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;
    }
}