using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Deploy a resource
    /// </summary>
    public class DeployResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {

            string resourceDefinition;
            string roles;

            values.TryGetValue("ResourceDefinition", out resourceDefinition);
            values.TryGetValue("Roles", out roles);

            if(string.IsNullOrEmpty(roles) || string.IsNullOrEmpty(resourceDefinition))
            {
                throw new InvalidDataContractException("Roles or ResourceDefinition missing");
            }

            string result = WorkspaceRepository.Instance.ServerWorkspace.Save(resourceDefinition, roles);
            WorkspaceRepository.Instance.RefreshWorkspaces();

            return result;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService deployResourceDynamicService = new DynamicService();
            deployResourceDynamicService.Name = HandlesType();
            deployResourceDynamicService.DataListSpecification = "<root><ResourceDefinition/><Roles/></root>";

            ServiceAction deployResourceServiceAction = new ServiceAction();
            deployResourceServiceAction.Name = HandlesType();
            deployResourceServiceAction.ActionType = enActionType.InvokeManagementDynamicService;
            deployResourceServiceAction.SourceMethod = HandlesType();

            deployResourceDynamicService.Actions.Add(deployResourceServiceAction);

            return deployResourceDynamicService;
        }

        public string HandlesType()
        {
            return "DeployResourceService";
        }
    }
}
