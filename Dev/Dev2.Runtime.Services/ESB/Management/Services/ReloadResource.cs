/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class ReloadResource : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var result = new ExecuteMessage { HasError = false };

            string resourceID = null;
            string resourceType = null;

            values.TryGetValue("ResourceID", out StringBuilder tmp);
            if (tmp != null)
            {
                resourceID = tmp.ToString();
            }

            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                resourceType = tmp.ToString();
            }
            Dev2Logger.Info($"Reload Resource. Id:{resourceID} Type:{resourceType}", GlobalConstants.WarewolfInfo);
            try
            {
                if(resourceID == "*")
                {
                    ResourceCatalog.Instance.LoadWorkspace(theWorkspace.ID);
                }
                else
                {
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
                        case "Server":
                            serviceType = enDynamicServiceObjectType.Source;
                            break;
                        default:
                            throw new Exception("Unexpected resource type '" + resourceType + "'.");
                    }
                    ResourceCatalog.Instance.LoadWorkspace(theWorkspace.ID);
                    result.SetMessage(string.Concat("'", resourceID, "' Reloaded..."));
                }
            }
            catch(Exception ex)
            {
                result.SetMessage(string.Concat("Error reloading '", resourceID, "'..."));
                Dev2Logger.Error(ex, GlobalConstants.WarewolfError);
            }

            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "ReloadResourceService";
    }
}
