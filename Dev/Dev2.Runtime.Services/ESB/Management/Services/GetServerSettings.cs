using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Workspaces;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Warewolf.Resource.Errors;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class GetServerSettings : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();

            if (values == null)
            {
                throw new InvalidDataContractException(ErrorResource.NoParameter);
            }

            var settings = Config.Server.Get();
            return serializer.SerializeToBuilder(settings);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Database ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "GetServerSettings";
    }
}
