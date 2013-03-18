using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

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
            var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, guidCsv, type);
            return string.IsNullOrEmpty(result) ? "<Nothing/>" : result;
        }

        public DynamicService CreateServiceEntry()
        {
            var findResourcesByIDAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var findResourcesByIDService = new DynamicService { Name = HandlesType(), DataListSpecification = "<root><GuidCsv/><Type/></root>" };
            findResourcesByIDService.Actions.Add(findResourcesByIDAction);



            return findResourcesByIDService;
        }

        public string HandlesType()
        {
            return "FindResourcesByID";
        }
    }
}
