
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
using Dev2.Common.Interfaces.Hosting;
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
            string type = null;
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);
            Guid resourceId = Guid.Empty;
            if(tmp != null)
            {
                if(!Guid.TryParse(tmp.ToString(), out resourceId))
                {
                    Dev2Logger.Log.Info("Delete Resource Service. Invalid Parameter Guid:");
                    var failureResult = new ExecuteMessage { HasError = true };
                    failureResult.SetMessage("Invalid guid passed for ResourceID");
                    return serializer.SerializeToBuilder(failureResult);
                }
            }
            values.TryGetValue("ResourceType", out tmp);
            if(tmp != null)
            {
                type = tmp.ToString();
            }

            Dev2Logger.Log.Info("Delete Resource Service. Resource:" + resourceId);
            // BUG 7850 - TWR - 2013.03.11 - ResourceCatalog refactor
            var msg = ResourceCatalog.Instance.DeleteResource(theWorkspace.ID, resourceId, type);

            var result = new ExecuteMessage { HasError = false };
            result.SetMessage(msg.Message);
            result.HasError = msg.Status != ExecStatus.Success;
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
                DataListSpecification = new StringBuilder("<DataList><ResourceName ColumnIODirection=\"Input\"/><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>")
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
