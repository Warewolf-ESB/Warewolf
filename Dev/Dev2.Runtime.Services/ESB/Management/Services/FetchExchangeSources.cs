#pragma warning disable
 using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchExchangeSources : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            var resourceCatalog = ResourceCatalog.Instance;
            var list = resourceCatalog.GetResourceList(GlobalConstants.ServerWorkspaceID).Where(a => a.ResourceType == "ExchangeSource").Select(a =>
            {
                // TODO: should not fetch resource a second time, use "a"
                var res = resourceCatalog.GetResource<ExchangeSource>(GlobalConstants.ServerWorkspaceID, a.ResourceID);
                if (res != null)
                {
                    return new ExchangeSourceDefinition
                    {
                        ResourceID = res.ResourceID,
                        ResourceName = res.ResourceName,
                        Path = res.GetSavePath(),
                        UserName = res.UserName,
                        Password = res.Password,
                        AutoDiscoverUrl = res.AutoDiscoverUrl,
                        Timeout = res.Timeout,
                        Type = res.Type,
                        ResourceType = res.ResourceType
                    };
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage() { HasError = false, Message = serializer.SerializeToBuilder(list) });
            
        }

        IResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchExchangeSources";
    }
}
