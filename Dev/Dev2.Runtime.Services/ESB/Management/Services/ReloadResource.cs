using System;
using System.Collections.Generic;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Reload a resource from disk ;)
    /// </summary>
    public class ReloadResource : IEsbManagementEndpoint
    {
        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            StringBuilder result = new StringBuilder();

            string resourceName;
            string resourceType;

            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("ResourceType", out resourceType);


            try
            {
                // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
                if(resourceName == "*")
                {
                    ResourceCatalog.Instance.LoadWorkspace(theWorkspace.ID);
                }
                else
                {
                    //
                    // Ugly conversion between studio resource type and server resource type
                    //
                    enDynamicServiceObjectType serviceType;
                    switch(resourceType)
                    {
                        case "HumanInterfaceProcess":
                        case "Website":
                        case "WorkflowService":
                            serviceType = enDynamicServiceObjectType.WorkflowActivity;
                            break;
                        case "Service":
                            serviceType = enDynamicServiceObjectType.DynamicService;
                            break;
                        case "Source":
                            serviceType = enDynamicServiceObjectType.Source;
                            break;
                        default:
                            throw new Exception("Unexpected resource type '" + resourceType + "'.");
                    }

                    //
                    // Copy the file from the server workspace into the current workspace
                    //
                    theWorkspace.Update(new WorkspaceItem(theWorkspace.ID, HostSecurityProvider.Instance.ServerID)
                    {
                        Action = WorkspaceItemAction.Edit,
                        ServiceName = resourceName,
                        ServiceType = serviceType.ToString()
                    });

                    //
                    // Reload resources
                    //
                    ResourceCatalog.Instance.LoadWorkspace(theWorkspace.ID);
                }
                result.Append(string.Concat("'", resourceName, "' Reloaded..."));
            }
            catch(Exception ex)
            {
                result.Append(string.Concat("Error reloading '", resourceName, "'..."));
                TraceWriter.WriteTrace(ex.StackTrace);
            }

            return result.ToString();
        }

        public string HandlesType()
        {
            return "ReloadResourceService";
        }


        public DynamicService CreateServiceEntry()
        {
            DynamicService reloadResourceServicesBinder = new DynamicService();
            reloadResourceServicesBinder.Name = HandlesType();
            reloadResourceServicesBinder.DataListSpecification = "<root><ResourceName/><ResourceType/></root>";

            ServiceAction reloadResourceServiceActionBinder = new ServiceAction();
            reloadResourceServiceActionBinder.Name = HandlesType();
            reloadResourceServiceActionBinder.SourceMethod = HandlesType();
            reloadResourceServiceActionBinder.ActionType = enActionType.InvokeManagementDynamicService;

            reloadResourceServicesBinder.Actions.Add(reloadResourceServiceActionBinder);

            return reloadResourceServicesBinder;
        }
    }
}
