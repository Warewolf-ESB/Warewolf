#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DeployResource : IEsbManagementEndpoint
    {
        bool _existingResource;
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            requestArgs.TryGetValue("ResourceDefinition", out StringBuilder resourceDefinition);
            if (resourceDefinition!=null && resourceDefinition.Length!=0)
            {
                var xml = resourceDefinition.ToXElement();
                var resource = new Resource(xml);

                var res = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
                if (res != null)
                {
                    _existingResource = true;
                    return res.ResourceID;
                }
            }
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            if (_existingResource)
            {
                return AuthorizationContext.Contribute;
            }
            return AuthorizationContext.DeployTo;
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            values.TryGetValue("savePath", out StringBuilder savePathValue);
            if (savePathValue == null)
            {
                throw new InvalidDataContractException("SavePath is missing");
            }
            values.TryGetValue("ResourceDefinition", out StringBuilder resourceDefinition);
            Dev2Logger.Info("Deploy Resource.", GlobalConstants.WarewolfInfo);
            if(resourceDefinition == null || resourceDefinition.Length == 0)
            {
                Dev2Logger.Info("Roles or ResourceDefinition missing", GlobalConstants.WarewolfInfo);
                throw new InvalidDataContractException("Roles or ResourceDefinition missing");
            }

            var msg = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition, savePathValue.ToString(),GlobalConstants.SaveReasonForDeploy, "unknown");
            WorkspaceRepository.Instance.RefreshWorkspaces();

            var result = new ExecuteMessage { HasError = msg.Status != ExecStatus.Success };
            result.SetMessage(msg.Message);
            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeployResourceService";
    }
}
