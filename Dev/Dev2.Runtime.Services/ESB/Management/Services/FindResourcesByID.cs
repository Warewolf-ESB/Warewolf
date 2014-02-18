using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find a resource by its id
    /// </summary>
    public class FindResourcesByID : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string guidCsv = string.Empty;
            string type = null;

            StringBuilder tmp;
            values.TryGetValue("GuidCsv", out tmp);
            if(tmp != null)
            {
                guidCsv = tmp.ToString();
            }
            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                type = tmp.ToString();
            }

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, guidCsv, type);

            IList<SerializableResource> resourceList = resources.Select(new FindResourceHelper().SerializeResourceForStudio).ToList();

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(resourceList);
        }

        public DynamicService CreateServiceEntry()
        {
            var findResourcesByIDAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var findResourcesByIDService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><GuidCsv ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            findResourcesByIDService.Actions.Add(findResourcesByIDAction);

            return findResourcesByIDService;
        }

        public string HandlesType()
        {
            return "FindResourcesByID";
        }
    }
}
