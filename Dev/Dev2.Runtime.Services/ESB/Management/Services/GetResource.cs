using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Gets a resource definition
    /// </summary>
    public class GetResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string resourceType;
            string resourceName;
            string roles;

            values.TryGetValue("ResourceType", out resourceType);
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("Roles", out roles);

            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var result = ResourceCatalog.Instance.GetPayload(theWorkspace.ID, resourceName, resourceType, roles, false);
            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            var getResourceServicesBinder = new DynamicService { Name = HandlesType(), DataListSpecification = "<root><ResourceType/><Roles/><ResourceName/></root>" };

            var getResourceServiceActionBinder = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            getResourceServicesBinder.Actions.Add(getResourceServiceActionBinder);

            return getResourceServicesBinder;
        }

        public string HandlesType()
        {
            return "GetResourceService";
        }
    }
}
