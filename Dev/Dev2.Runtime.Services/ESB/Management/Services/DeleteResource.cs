using Dev2.Common;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using System.Collections.Generic;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Delete a resource ;)
    /// </summary>
    public class DeleteResource : IEsbManagementEndpoint
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
            var result = ResourceCatalog.Instance.DeleteResource(theWorkspace.ID, resourceName, type, roles);

            // Delete resource from server workspace
            if (theWorkspace.ID != GlobalConstants.ServerWorkspaceID && result.Status == ExecStatus.Success)
            {
                var serverResult = ResourceCatalog.Instance.DeleteResource(GlobalConstants.ServerWorkspaceID, resourceName, type, roles);

                if (serverResult.Status != ExecStatus.Success)
                {
                    // If delete from server workspace failed, then sync server workspace back to client workspace and return server result
                    var workspacePath = GlobalConstants.GetWorkspacePath(theWorkspace.ID);
                    var serverworkspacePath = GlobalConstants.GetWorkspacePath(theWorkspace.ID);
                    ResourceCatalog.Instance.SyncTo(serverworkspacePath, workspacePath, false, false);

                    result = serverResult;
                }
            }

            return result.Message;
        }

        public string HandlesType()
        {
            return "DeleteResourceService";
        }

        public DynamicService CreateServiceEntry()
        {
            var deleteResourceService = new DynamicService
            {
                Name = HandlesType(),
                DataListSpecification = "<DataList><ResourceName/><ResourceType/><Roles/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
            };

            var deleteResourceAction = new ServiceAction
            {
                Name = HandlesType(),
                ActionType = enActionType.InvokeManagementDynamicService,
                SourceMethod = HandlesType()
            };

            deleteResourceService.Actions.Add(deleteResourceAction);

            return deleteResourceService;
        }
    }
}
