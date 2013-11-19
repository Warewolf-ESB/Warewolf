using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Providers.Errors;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Newtonsoft.Json;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find resources in the service catalog
    /// </summary>
    public class FindResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceName;
            string type;
            string roles;

            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("ResourceType", out type);
            values.TryGetValue("Roles", out roles);

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var resources = ResourceCatalog.Instance.GetResourceList(theWorkspace.ID, resourceName, type, roles);

            IList<SerializableResource> resourceList = resources.Select(StripForShip).ToList();

            var result = JsonConvert.SerializeObject(resourceList);

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceType/><Roles/><ResourceName/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };

            var findServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(findServiceAction);

            return findServices;
        }

        public string HandlesType()
        {
            return "FindResourceService";
        }


        /// <summary>
        /// Strips for ship.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// TODO : Extract to util method and remove dup in FindResourcesByID
        /// <returns></returns>
        private SerializableResource StripForShip(IResource resource)
        {

            // convert the fliping errors due to json issues in c# ;(
            List<ErrorInfo> errors = new List<ErrorInfo>();
            var parseErrors = resource.Errors;
            if (parseErrors != null)
            {
                errors.AddRange(parseErrors.Select(error => (error as ErrorInfo)));
            }

            var datalist = "<DataList></DataList>";

            if(resource.DataList != null)
            {
                datalist = resource.DataList.Replace("\"", GlobalConstants.SerializableResourceQuote).Replace("'", GlobalConstants.SerializableResourceSingleQuote);
            }

            return new SerializableResource
            {
                ResourceCategory = resource.ResourcePath,
                ResourceID = resource.ResourceID,
                ResourceName = resource.ResourceName,
                ResourceType = resource.ResourceType,
                Version = resource.Version,
                IsValid = resource.IsValid,
                DataList = datalist,
                Errors =  errors
            };
        }
    }
}
