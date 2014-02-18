using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Delete a resource ;)
    /// </summary>
    public class DeleteResource : IEsbManagementEndpoint
    {
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            string resourceName = null;
            string type = null;

            StringBuilder tmp;
            values.TryGetValue("ResourceName", out tmp);
            if(tmp != null)
            {
                resourceName = tmp.ToString();
            }

            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                type = tmp.ToString();
            }


            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var msg = ResourceCatalog.Instance.DeleteResource(theWorkspace.ID, resourceName, type);

            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg.Message);

            // Delete resource from server workspace
            if(theWorkspace.ID != GlobalConstants.ServerWorkspaceID && msg.Status == ExecStatus.Success)
            {
                var serverResult = ResourceCatalog.Instance.DeleteResource(GlobalConstants.ServerWorkspaceID,
                                                                           resourceName, type);

                if(serverResult.Status != ExecStatus.Success)
                {
                    // If delete from server workspace failed, then sync server workspace back to client workspace and return server result
                    var workspacePath = EnvironmentVariables.GetWorkspacePath(theWorkspace.ID);
                    var serverworkspacePath = EnvironmentVariables.GetWorkspacePath(theWorkspace.ID);
                    ResourceCatalog.Instance.SyncTo(serverworkspacePath, workspacePath, false, false);

                    result.SetMessage(serverResult.Message);
                }
            }
            else
            {
                result.HasError = true;
            }


            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
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
                DataListSpecification = "<DataList><ResourceName ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>"
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
