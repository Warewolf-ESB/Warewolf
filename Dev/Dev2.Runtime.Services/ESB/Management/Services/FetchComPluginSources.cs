using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{

    public class FetchComPluginSources : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            
            List<ComPluginSourceDefinition> list = Resources.GetResourceList<ComPluginSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                if (a is ComPluginSource res)
                {
                    return new ComPluginSourceDefinition
                    {
                        Id = res.ResourceID,
                        ResourceName = res.ResourceName,
                        ClsId = res.ClsId,
                        ResourcePath = res.GetSavePath(),
                        Is32Bit = res.Is32Bit,
                        SelectedDll = new DllListing
                        {
                            Name = res.ComName,
                            ClsId = res.ClsId,
                            Is32Bit = res.Is32Bit,
                            Children = new IFileListing[0]
                        }
                    };
                }
                return null;
            }).ToList();
            return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
            
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchComPluginSources";
    }
}