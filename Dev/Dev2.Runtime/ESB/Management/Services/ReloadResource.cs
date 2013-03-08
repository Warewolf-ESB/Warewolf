using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Dev2.DynamicServices;
using System.Text;
using Dev2.DynamicServices.Security;
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
            IDynamicServicesHost theHost = theWorkspace.Host;

            string resourceName;
            string resourceType;
            
            values.TryGetValue("ResourceName", out resourceName);
            values.TryGetValue("ResourceType", out resourceType);
    

            try
            {
                // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
                if (resourceName == "*")
                {
                    theHost.RestoreResources(new[] { "Sources", "Services" });
                }
                else
                {
                    //
                    // Ugly conversion between studio resource type and server resource type
                    //
                    enDynamicServiceObjectType serviceType;
                    string directory;
                    if (resourceType == "WorkflowService" ||
                        resourceType == "Website" || resourceType == "HumanInterfaceProcess")
                    {
                        directory = "Services";
                        serviceType = enDynamicServiceObjectType.WorkflowActivity;
                    }
                    else if (resourceType == "Service")
                    {
                        directory = "Services";
                        serviceType = enDynamicServiceObjectType.DynamicService;
                    }
                    else if (resourceType == "Source")
                    {
                        directory = "Sources";
                        serviceType = enDynamicServiceObjectType.Source;
                    }
                    else
                    {
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
                    theHost.RestoreResources(new[] { directory }, resourceName);
                }
                result.Append(string.Concat("'", resourceName, "' Reloaded..."));
            }
            catch (Exception ex)
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
