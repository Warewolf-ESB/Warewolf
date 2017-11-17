using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDropBoxSource : DefaultEsbManagementEndpoint
    {
        #region Implementation of IEsbManagementEndpoint

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            List<DropBoxSource> resourceList = Resources.GetResourceList<DropBoxSource>(GlobalConstants.ServerWorkspaceID)
                .Cast<DropBoxSource>()
                .ToList();
            return serializer.SerializeToBuilder(resourceList);
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        #endregion

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchDropBoxSources";
    }
}