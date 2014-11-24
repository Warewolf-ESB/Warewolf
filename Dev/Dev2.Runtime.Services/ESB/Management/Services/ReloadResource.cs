
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
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
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            ExecuteMessage result = new ExecuteMessage { HasError = false };

            string resourceID = null;
            string resourceType = null;

            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            if(tmp != null)
            {
                resourceID = tmp.ToString();
            }

            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                resourceType = tmp.ToString();
            }
            Dev2Logger.Log.Info(String.Format("Reload Resource. Id:{0} Type:{1}",resourceID,resourceType));
            try
            {
                // 2012.10.01: TWR - 5392 - Server does not dynamically reload resources 
                if(resourceID == "*")
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
                        case "Server":
                            serviceType = enDynamicServiceObjectType.Source;
                            break;
                        default:
                            throw new Exception("Unexpected resource type '" + resourceType + "'.");
                    }
                    Guid getID;
                    if(resourceID != null && Guid.TryParse(resourceID, out getID))
                    {
                        //
                        // Copy the file from the server workspace into the current workspace
                        //
                        theWorkspace.Update(
                            new WorkspaceItem(theWorkspace.ID, HostSecurityProvider.Instance.ServerID, Guid.Empty, getID)
                            {
                                Action = WorkspaceItemAction.Edit,
                                IsWorkflowSaved = true,
                                ServiceType = serviceType.ToString()
                            }, false);

                    }
                    else
                    {
                        theWorkspace.Update(
                            new WorkspaceItem(theWorkspace.ID, HostSecurityProvider.Instance.ServerID, Guid.Empty, Guid.Empty)
                            {
                                Action = WorkspaceItemAction.Edit,
                                ServiceName = resourceID,
                                IsWorkflowSaved = true,
                                ServiceType = serviceType.ToString()
                            }, false);
                    }
                    //
                    // Reload resources
                    //
                    ResourceCatalog.Instance.LoadWorkspace(theWorkspace.ID);
                    result.SetMessage(string.Concat("'", resourceID, "' Reloaded..."));
                }
            }
            catch(Exception ex)
            {
                result.SetMessage(string.Concat("Error reloading '", resourceID, "'..."));
                Dev2Logger.Log.Error(ex);
            }

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public string HandlesType()
        {
            return "ReloadResourceService";
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService reloadResourceServicesBinder = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction reloadResourceServiceActionBinder = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            reloadResourceServicesBinder.Actions.Add(reloadResourceServiceActionBinder);

            return reloadResourceServicesBinder;
        }
    }
}
