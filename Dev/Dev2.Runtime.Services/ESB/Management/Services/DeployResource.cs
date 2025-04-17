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

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Triggers;

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
                var resource = new ServiceModel.Data.Resource(xml);

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
        IConnections _connections = new Connections();
        Data.ServiceModel.Connection _destinationConnection;
        private bool shouldLoadQueue;

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
            var deployResultsList = new List<DeployResultDetails>();

            try
            {
                var deployResults = new DeployResultDetails() { Tests = new List<DeployResult>(), Triggers = new List<DeployResult>() };
                values.TryGetValue("destinationEnvironmentId", out StringBuilder destinationEnvironment);

                if (destinationEnvironment == null)
                {
                    deployResults.HasError = true; deployResults.Message = "destinationEnvironment is null";
                    deployResultsList.Add(deployResults);
                    return GetCompressedMessage(serializer, deployResultsList, true);
                }

                _destinationConnection = serializer.Deserialize<Data.ServiceModel.Connection>(destinationEnvironment);

                values.TryGetValue("resourceIDs", out StringBuilder sbResourceIDsValues);
                if (sbResourceIDsValues == null || sbResourceIDsValues.Length == 0)
                {
                    Dev2Logger.Info("resourceIDs missing", GlobalConstants.WarewolfInfo);

                    deployResults.HasError = true; deployResults.Message = "ResourceIds missing";
                    deployResultsList.Add(deployResults);
                    return GetCompressedMessage(serializer, deployResultsList, true);
                }

                var resourceIdList = serializer.Deserialize<List<Guid>>(sbResourceIDsValues);
                if (resourceIdList == null || resourceIdList.Count == 0)
                {
                    Dev2Logger.Info("Failed to get resourceIDs", GlobalConstants.WarewolfInfo);
                    deployResults.HasError = true; deployResults.Message = "ResourceIds are invalid";
                    deployResultsList.Add(deployResults);
                    return GetCompressedMessage(serializer, deployResultsList, true);
                }

                values.TryGetValue("deployTests", out StringBuilder deployTests);
                values.TryGetValue("deployTriggers", out StringBuilder deployTriggers);

                var doTestDeploy = bool.Parse(deployTests.ToString());
                var doTriggerDeploy = bool.Parse(deployTriggers.ToString());
                this.shouldLoadQueue = true; // set flag to load queue first time it is requested
                var batchSize = 10; // batch size considering only no. of resources 
                var batchList = new List<List<DeployResourceData>>();

                // Check connection to destination server
                var canConnectToServer = _connections.CanConnectToServer(_destinationConnection);
                if (!canConnectToServer.IsValid)
                {
                    Dev2Logger.Info("Can not connect to destination server", GlobalConstants.WarewolfInfo);
                    deployResults.HasError = true; deployResults.Message = "Can not connect to destination server";
                    deployResultsList.Add(deployResults);
                    return GetCompressedMessage(serializer, deployResultsList, true);
                }

                // Fetch Connection to destination Server
                HubConnection proxy = null;
                try
                {
                    proxy = _connections.GetHubConnection(_destinationConnection);
                }
                catch (Exception)
                {
                    Dev2Logger.Info("Failed to get destination server connection", GlobalConstants.WarewolfInfo);
                    deployResults.HasError = true; deployResults.Message = "Failed to get destination server connection";
                    deployResultsList.Add(deployResults);
                    return GetCompressedMessage(serializer, deployResultsList, true);
                }

                // Create batches
                for (int i = 0; i < resourceIdList.Count; i += batchSize)
                {
                    var range = resourceIdList.GetRange(i, Math.Min(batchSize, resourceIdList.Count - i));
                    var batch = CreateBatchForDeploy(range, doTestDeploy, doTriggerDeploy, deployResultsList);
                    batchList.Add(batch);
                }

                // Deploy Batch
                var taskList = new List<Task>();
                foreach (var batch in batchList)
                {
                    var lastTask = DeployBatch(batch, serializer, deployResultsList, proxy);
                    taskList.Add(lastTask);
                }

                Task.WaitAll(taskList.ToArray());

                WorkspaceRepository.Instance.RefreshWorkspaces();

                return GetCompressedMessage(serializer, deployResultsList, false);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("DeployAllResourcesService error", ex, GlobalConstants.WarewolfError);
                var errorMsg = new CompressedExecuteMessage { HasError = true, Message = new StringBuilder(ex.Message) };
                return serializer.SerializeToBuilder(errorMsg);
            }
        }

        private StringBuilder GetCompressedMessage(Dev2JsonSerializer serializer, List<DeployResultDetails> deployResultsList, bool isError)
        {
            var message = new CompressedExecuteMessage();
            message.SetMessage(serializer.Serialize(deployResultsList));
            message.HasError = isError;
            return serializer.SerializeToBuilder(message);
        }


        private List<DeployResourceData> CreateBatchForDeploy(IEnumerable<Guid> resourceIdList, bool doTestDeploy, bool doTriggerDeploy, List<DeployResultDetails> deployResultsList)
        {
            var batch = new List<DeployResourceData>();

            foreach (var resourceId in resourceIdList)
            {
                var data = new DeployResourceData();
                var deployResults = new DeployResultDetails() { ResourceId = resourceId.ToString(), Tests = new List<DeployResult>(), Triggers = new List<DeployResult>() };

                var resourceDefinition = GetResourceDefinition(resourceId, out IResource resource);
                if (resourceDefinition == null || resourceDefinition.Length == 0)
                {
                    var message = "ResourceDefinition missing for " + resourceId;
                    Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
                    deployResults.HasError = true; deployResults.Message = message;
                    deployResultsList.Add(deployResults);
                    continue;
                }

                var resourcePath = resource.GetSavePath();
                if (resourcePath == null)
                {
                    deployResults.HasError = true; deployResults.Message = "SavePath is missing";
                    deployResultsList.Add(deployResults);
                    continue;
                }


                if (doTestDeploy)
                    data.Tests = TestCatalog.Instance.Fetch(resourceId);

                if (doTriggerDeploy)
                    data.Triggers = TriggersCatalog.Instance.FetchQueuesByResourceId(resourceId, this.shouldLoadQueue);

                this.shouldLoadQueue = false; // reset so that it does not load queue every time

                data.ResourceDefinition = resourceDefinition;
                data.ResourceId = resourceId.ToString();
                data.SavePath = resourcePath;
                batch.Add(data);
            }

            return batch;
        }

        private async Task DeployBatch(List<DeployResourceData> batch, Dev2JsonSerializer serializer, List<DeployResultDetails> deployResultsList, HubConnection proxy)
        {
            await ExecuteDeployOnDestination(batch, serializer, deployResultsList, proxy);
        }

        private async Task ExecuteDeployOnDestination(List<DeployResourceData> batch, Dev2JsonSerializer serializer, List<DeployResultDetails> deployResultsList, HubConnection proxy)
        {
            var esbExecuteRequest = new EsbExecuteRequest { ServiceName = "DeployBatchResourcesService" };
            esbExecuteRequest.AddArgument("data", new StringBuilder(serializer.Serialize(batch)));
            var envelope = new Envelope
            {
                Content = serializer.SerializeToBuilder(esbExecuteRequest).ToString(),
                PartID = 0,
            };
            var messageId = Guid.NewGuid();
            await proxy.InvokeAsync<Receipt>("ExecuteCommand", envelope, true, Guid.Empty, Guid.Empty, messageId).ConfigureAwait(false);

            var fragmentInvokeResult = proxy.InvokeCoreAsync("FetchExecutePayloadFragment", typeof(String), new FutureReceipt[] { new FutureReceipt { PartID = 0, RequestID = messageId } }, new System.Threading.CancellationToken()).Result;

            var execResult = serializer.Deserialize<List<DeployResultDetails>>(fragmentInvokeResult as string);
            if (execResult == null || execResult.Count == 0)
            {
                foreach (var item in batch)
                {
                    var deployResults = new DeployResultDetails();
                    deployResults.ResourceId = item.ResourceId;
                    deployResults.HasError = true; deployResults.Message = "Deploy Failed";
                    deployResultsList.Add(deployResults);
                }
            }
            else
            {
                deployResultsList.AddRange(execResult);
            }
        }


        private StringBuilder GetResourceDefinition(Guid resourceId, out IResource resource)
        {
            var resourceDefinition = ResourceCatalog.Instance.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            resource = ResourceCatalog.Instance.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            if (resource != null && !resource.IsService)
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

    public class DeployBatchResources : IEsbManagementEndpoint
    {
        bool _existingResource;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            requestArgs.TryGetValue("ResourceDefinition", out StringBuilder resourceDefinition);
            if (resourceDefinition != null && resourceDefinition.Length != 0)
            {
                var xml = resourceDefinition.ToXElement();
                var resource = new ServiceModel.Data.Resource(xml);

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
            var deployResultsList = new List<DeployResultDetails>();


            values.TryGetValue("data", out StringBuilder data);
            if (data == null)
            {
                Dev2Logger.Info("Batch not found", GlobalConstants.WarewolfInfo);
                deployResultsList.Add(new DeployResultDetails() { HasError = true, Message = "Batch not found" });
                return serializer.SerializeToBuilder(deployResultsList);
            }


            var batch = serializer.Deserialize<List<DeployResourceData>>(data);
            if (batch == null || batch.Count == 0)
            {
                Dev2Logger.Info("Failed to extract batch", GlobalConstants.WarewolfInfo);
                deployResultsList.Add(new DeployResultDetails() { HasError = true, Message = "Failed to extract batch" });

                return serializer.SerializeToBuilder(deployResultsList);
            }


            foreach (var item in batch)
            {
                var deployResults = new DeployResultDetails() { Tests = new List<DeployResult>(), Triggers = new List<DeployResult>() };

                Guid.TryParse(item.ResourceId, out Guid resourceId);
                deployResults.ResourceId = item.ResourceId;

                var resourceDefinition = item.ResourceDefinition;
                if (resourceDefinition == null || resourceDefinition.Length == 0)
                {
                    var message = "ResourceDefinition missing for " + item.ResourceId;
                    Dev2Logger.Info(message, GlobalConstants.WarewolfInfo);
                    deployResults.HasError = true; deployResults.Message = message;

                    deployResultsList.Add(deployResults);
                    continue;
                }

                var resourcePath = item.SavePath;
                if (resourcePath == null)
                {
                    deployResults.HasError = true; deployResults.Message = "SavePath is missing";
                    deployResultsList.Add(deployResults);
                    continue;
                }

                var response = ResourceCatalog.Instance.SaveResource(WorkspaceRepository.ServerWorkspaceID, resourceDefinition, resourcePath, GlobalConstants.SaveReasonForDeploy, "unknown");

                var hasError = response.Status != ExecStatus.Success;
                deployResults.HasError = hasError;
                deployResults.Message = response.Message;

                if (hasError)
                {
                    deployResultsList.Add(deployResults);
                    continue;
                }

                if (item.Tests != null && item.Tests.Count > 0)
                {
                    var testResults = TestCatalog.Instance.PersistTests(resourceId, item.Tests);
                    foreach (DeployResult test in testResults)
                        deployResults.Tests.Add(test);
                }

                if (item.Triggers != null && item.Triggers.Count > 0)
                {
                    foreach (var queue in item.Triggers)
                    {
                        var status = TriggersCatalog.Instance.PersistTriggerQueue(queue);
                        deployResults.Triggers.Add(new DeployResult() { HasError = !status, Message = queue.QueueName });
                    }
                }

                deployResultsList.Add(deployResults);
                deployResults = new DeployResultDetails() { Tests = new List<DeployResult>(), Triggers = new List<DeployResult>() };
            } //end for loop

            WorkspaceRepository.Instance.RefreshWorkspaces();
            return serializer.SerializeToBuilder(deployResultsList);

        }


        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DeployBatchResourcesService";

    }

    public class DeployResourceData
    {
        public string ResourceId { get; set; }
        public StringBuilder ResourceDefinition { get; set; }
        public string SavePath { get; set; }
        public List<IServiceTestModelTO> Tests { get; set; }
        public List<ITriggerQueue> Triggers { get; set; }
    }

    public class DeployResultDetails
    {
        public string ResourceId { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
        public List<DeployResult> Tests { get; set; }
        public List<DeployResult> Triggers { get; set; }
    }
}
