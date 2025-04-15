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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
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
            if (resourceDefinition != null && resourceDefinition.Length != 0)
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
            if (resourceDefinition == null || resourceDefinition.Length == 0)
            {
                Dev2Logger.Info("Roles or ResourceDefinition missing", GlobalConstants.WarewolfInfo);
                throw new InvalidDataContractException("Roles or ResourceDefinition missing");
            }

            var msg = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition, savePathValue.ToString(), GlobalConstants.SaveReasonForDeploy, "unknown");
            WorkspaceRepository.Instance.RefreshWorkspaces();

            var result = new ExecuteMessage { HasError = msg.Status != ExecStatus.Success };
            result.SetMessage(msg.Message);
            var serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(result);
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeployResourceService";
    }

    public class DeployAllResources : IEsbManagementEndpoint
    {
        bool _existingResource;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            requestArgs.TryGetValue("ResourceDefinition", out StringBuilder resourceDefinition);
            if (resourceDefinition != null && resourceDefinition.Length != 0)
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
            var serializer = new Dev2JsonSerializer();
            var deployResults = new List<DeployResult>();

            values.TryGetValue("resourceIDs", out StringBuilder sbResourceIDsValues);
            if (sbResourceIDsValues == null || sbResourceIDsValues.Length == 0)
            {
                Dev2Logger.Info("resourceIDs missing", GlobalConstants.WarewolfInfo);

                deployResults.Add(new DeployResult() { HasError = true, Message = "ResourceIds missing" });
                return serializer.SerializeToBuilder(deployResults);
            }

            var resourceIdList = serializer.Deserialize<List<Guid>>(sbResourceIDsValues);
            if (resourceIdList == null || resourceIdList.Count == 0)
            {
                Dev2Logger.Info("Failed to get resourceIDs", GlobalConstants.WarewolfInfo);
                deployResults.Add(new DeployResult() { HasError = true, Message = "ResourceIds are invalid" });
                return serializer.SerializeToBuilder(deployResults);
            }

            values.TryGetValue("deployTests", out StringBuilder deployTests);
            values.TryGetValue("deployTriggers", out StringBuilder deployTriggers);

            var doTestDeploy = bool.Parse(deployTests.ToString());
            var doTriggerDeploy = bool.Parse(deployTriggers.ToString());

            foreach (var resourceId in resourceIdList)
            {
                var strResourceId = resourceId.ToString();
                var resourceDefinition = GetResourceDefinition(resourceId, out IResource resource);
                if (resourceDefinition == null || resourceDefinition.Length == 0)
                {
                    var message = "ResourceDefinition missing for " + resourceId;
                    Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
                    deployResults.Add(new DeployResult() { HasError = true, Message = message });
                    continue;
                }

                var resourcePath = resource.GetSavePath();
                if (resourcePath == null)
                {
                    deployResults.Add(new DeployResult() { HasError = true, Message = "SavePath is missing" });
                    continue;
                }

                var response = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition, resourcePath, GlobalConstants.SaveReasonForDeploy, "unknown");
                var hasError = response.Status != ExecStatus.Success;
                deployResults.Add(new DeployResult() { HasError = hasError , ErrorDetails = strResourceId});
                if (hasError) continue;

                if (doTestDeploy)
                {
                    var testsToDeploy = TestCatalog.Instance.Fetch(resourceId);
                    var testResults = TestCatalog.Instance.PersistTests(resourceId, testsToDeploy);

                    foreach (DeployResult test in testResults)
                    {
                        test.ErrorDetails = strResourceId;
                        deployResults.Add(test);
                    }
                }

                if (doTriggerDeploy)
                {
                    var triggersToDeploy = TriggersCatalog.Instance.LoadQueuesByResourceId(resourceId);
                    foreach (var queue in triggersToDeploy)
                    {
                        var status = TriggersCatalog.Instance.PersistTriggerQueue(queue);
                        deployResults.Add(new DeployResult() { HasError = !status, ErrorDetails = strResourceId, Message = queue.QueueName });
                    }
                }

            } //end for loop

            WorkspaceRepository.Instance.RefreshWorkspaces();
            return serializer.SerializeToBuilder(deployResults);
        }

        private StringBuilder GetResourceDefinition(Guid resourceId, out IResource resource)
        {
            var resourceDefinition = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            if (!resource.IsService)
            {
                var xml = resource.ToXml().Value;

                if (!resource.IsService || xml.Contains("FileReadWithBase64") || xml.Contains("DsfFileWrite") || xml.Contains("DsfFileRead") || xml.Contains("DsfFolderRead") || xml.Contains("DsfPathCopy") || xml.Contains("DsfPathCreate") || xml.Contains("DsfPathDelete") || xml.Contains("DsfPathMove") || xml.Contains("DsfPathMove") || xml.Contains("DsfPathRename") || xml.Contains("DsfZip") || xml.Contains("DsfUnzip"))
                {
                    var fetchResourceService = new FetchResourceDefinition();
                    resourceDefinition = fetchResourceService.DecryptAllPasswords(resourceDefinition);
                }
            }
            return resourceDefinition;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeployAllResourcesService";
    }
}
