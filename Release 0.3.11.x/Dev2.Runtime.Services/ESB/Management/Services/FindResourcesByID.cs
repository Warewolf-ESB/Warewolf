using System.Collections.Generic;
using System.Linq;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find a resource by its id
    /// </summary>
    public class FindResourcesByID : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string guidCsv;
            string type;

            values.TryGetValue("GuidCsv", out guidCsv);
            values.TryGetValue("Type", out type);

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, guidCsv, type);

            IList<SerializableResource> resourceList = resources.Select(new FindResourceHelper().SerializeResourceForStudio).ToList();

            var result = JsonConvert.SerializeObject(resourceList);

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            var findResourcesByIDAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var findResourcesByIDService = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><GuidCsv/><Type/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            findResourcesByIDService.Actions.Add(findResourcesByIDAction);

            return findResourcesByIDService;
        }

        public string HandlesType()
        {
            return "FindResourcesByID";
        }
    }
}
