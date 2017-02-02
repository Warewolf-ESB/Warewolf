/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Deploy a resource
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DeployResource : IEsbManagementEndpoint
    {
        private bool _existingResource;
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            StringBuilder resourceDefinition;
            requestArgs.TryGetValue("ResourceDefinition", out resourceDefinition);
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
            StringBuilder resourceDefinition;

            StringBuilder savePathValue;
            values.TryGetValue("savePath", out savePathValue);
            if (savePathValue == null)
            {
                throw new InvalidDataContractException("SavePath is missing");
            }
            values.TryGetValue("ResourceDefinition", out resourceDefinition);
            Dev2Logger.Info("Deploy Resource.");
            if(resourceDefinition == null || resourceDefinition.Length == 0)
            {
                Dev2Logger.Info("Roles or ResourceDefinition missing");
                throw new InvalidDataContractException("Roles or ResourceDefinition missing");
            }

            var msg = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition, savePathValue.ToString(), "Deploy", "unknown");
            WorkspaceRepository.Instance.RefreshWorkspaces();

            var result = new ExecuteMessage { HasError = msg.Status != ExecStatus.Success };
            result.SetMessage(msg.Message);
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService deployResourceDynamicService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction deployResourceServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            deployResourceDynamicService.Actions.Add(deployResourceServiceAction);

            return deployResourceDynamicService;
        }

        public string HandlesType()
        {
            return "DeployResourceService";
        }
    }
}
