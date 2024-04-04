#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Microsoft.AspNetCore.SignalR.Client;
using Dev2.Runtime.Interfaces;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Warewolf.Triggers;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class DirectDeploy : IEsbManagementEndpoint
    {
        bool _existingResource;
        IConnections _connections;
        IResourceCatalog _resourceCatalog;
        ITestCatalog _testCatalog;
        ITriggersCatalog _triggersCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            requestArgs.TryGetValue("ResourceDefinition", out StringBuilder resourceDefinition);
            if (resourceDefinition != null && resourceDefinition.Length != 0)
            {
                var xml = resourceDefinition.ToXElement();
                var resource = new Resource(xml);

                var res = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resource.ResourceID);
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
            var toReturn = new List<DeployResult>();
            var serializer = new Dev2JsonSerializer();
            if (values == null)
            {
                toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder("No WorkSpace or Values specified")}, "An Error has occurred"));
            }
            else
            {
                values.TryGetValue("destinationEnvironmentId", out StringBuilder destinationEnvironment);

                if (destinationEnvironment == null)
                {
                    toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder("destinationEnvironment is null")}, "An Error has occurred"));
                }
                else
                {
                    ExecuteDeployResult(values, toReturn, serializer, destinationEnvironment);
                }
            }

            return serializer.SerializeToBuilder(toReturn);
        }

        void ExecuteDeployResult(Dictionary<string, StringBuilder> values, List<DeployResult> toReturn, Dev2JsonSerializer serializer, StringBuilder destinationEnvironment)
        {
            var destination = serializer.Deserialize<Data.ServiceModel.Connection>(destinationEnvironment);
            var canConnectToServer = Connections.CanConnectToServer(destination);
            if (!canConnectToServer.IsValid)
            {
                toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder(canConnectToServer.ErrorMessage)}, "An Error has occurred"));
            }
            else
			{
				var proxy = Connections.GetHubConnection(destination);
				var roles = new StringBuilder("*");
                values.TryGetValue("deployTests", out StringBuilder deployTests);
                values.TryGetValue("deployTriggers", out StringBuilder deployTriggers);

                if (deployTests == null)
                {
                    toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder("deployTests is null")}, "An Error has occurred"));
                }

                if (deployTriggers == null)
                {
                    toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder("deployTriggers is null")}, "An Error has occurred"));
                }
                else
                {
                    ShouldExecuteDeploy(values, toReturn, serializer, proxy, roles, deployTests, deployTriggers);
                }
            }
        }

        void ShouldExecuteDeploy(Dictionary<string, StringBuilder> values, List<DeployResult> toReturn, Dev2JsonSerializer serializer, HubConnection hubconnection, StringBuilder roles, StringBuilder deployTests, StringBuilder deployTriggers)
        {
            var doTestDeploy = bool.Parse(deployTests.ToString());
            var doTriggerDeploy = bool.Parse(deployTriggers.ToString());
            values.TryGetValue("resourceIDsToDeploy", out StringBuilder resourceIDsToDeploy);
            var idsToDeploy = new List<Guid>();
            idsToDeploy.AddRange(serializer.Deserialize<List<Guid>>(resourceIDsToDeploy));
            if (idsToDeploy.Any())
            {
                var counter = 0;
                var count = idsToDeploy.Count;
                var amountToTake = 10;
                while (counter < count)
                {
                    var diff = count - counter;
                    if (diff < 10)
                    {
                        amountToTake = diff;
                    }

                    var throttledIds = idsToDeploy.Skip(counter).Take(amountToTake);
                    var taskList = new List<Task>();
                    foreach (var resourceId in throttledIds)
                    {
                        var lastTask = GetTaskForDeploy(resourceId, roles, serializer, hubconnection, doTestDeploy, doTriggerDeploy, toReturn);
                        taskList.Add(lastTask);
                    }

                    Task.WaitAll(taskList.ToArray());
                    counter = counter + 10;
                }
            }
            else
            {
                toReturn.Add(new DeployResult(new ExecuteMessage {HasError = true, Message = new StringBuilder("No resources specified")}, "An Error has occurred"));
            }
        }

        public async Task GetTaskForDeploy(Guid resourceId, StringBuilder roles, Dev2JsonSerializer serializer, HubConnection hubConnection, bool doTestDeploy, bool doTriggerDeploy, List<DeployResult> toReturn)
        {
            var lastTask = Task.Run(async () =>
            {
                var results = await DeployResourceAsync(resourceId, roles, serializer, hubConnection, doTestDeploy, doTriggerDeploy).ConfigureAwait(false);
                toReturn.AddRange(results);
            });
            await lastTask.ConfigureAwait(false);
        }

        public IConnections Connections
        {
            private get => _connections ?? (_connections = new Connections());
            set => _connections = value;
        }

        public ITestCatalog TestCatalog
        {
            private get => _testCatalog ?? Runtime.TestCatalog.Instance;
            set => _testCatalog = value;
        }

        public ITriggersCatalog TriggersCatalog
        {
            private get => _triggersCatalog ?? Hosting.TriggersCatalog.Instance;
            set => _triggersCatalog = value;
        }

        public IResourceCatalog ResourceCatalog
        {
            private get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        async Task<IEnumerable<DeployResult>> DeployResourceAsync(Guid resourceId, StringBuilder roles, Dev2JsonSerializer serializer, HubConnection hubConnection, bool doTestDeploy, bool doTriggerDeploy)
        {
            var toReturn = new List<DeployResult>();
            var savePath = new StringBuilder();
            var resourceContent = ResourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            if (!resource.IsService || resource.ToXml().Value.Contains("FileReadWithBase64") || resource.ToXml().Value.Contains("DsfFileWrite") || resource.ToXml().Value.Contains("DsfFileRead") || resource.ToXml().Value.Contains("DsfFolderRead") || resource.ToXml().Value.Contains("DsfPathCopy") || resource.ToXml().Value.Contains("DsfPathCreate") || resource.ToXml().Value.Contains("DsfPathDelete") || resource.ToXml().Value.Contains("DsfPathMove") || resource.ToXml().Value.Contains("DsfPathMove") || resource.ToXml().Value.Contains("DsfPathRename") || resource.ToXml().Value.Contains("DsfZip") || resource.ToXml().Value.Contains("DsfUnzip"))
            {
                var fetchResourceService = new FetchResourceDefinition();
                resourceContent = fetchResourceService.DecryptAllPasswords(resourceContent);
            }

            savePath.Append(resource.GetSavePath());

            var esbExecuteRequest = new EsbExecuteRequest {ServiceName = "DeployResourceService"};
            esbExecuteRequest.AddArgument("savePath", savePath);
            esbExecuteRequest.AddArgument("ResourceDefinition", resourceContent);
            esbExecuteRequest.AddArgument("Roles", roles);
            var envelope = new Envelope
            {
                Content = serializer.SerializeToBuilder(esbExecuteRequest).ToString(),
                PartID = 0,
                //Type = typeof(Envelope)
            };
            var messageId = Guid.NewGuid();
            await hubConnection.InvokeAsync<Receipt>("ExecuteCommand", envelope, true, Guid.Empty, Guid.Empty, messageId).ConfigureAwait(false);
            //var fragmentInvokeResult = await hubConnection.InvokeAsync<string>("FetchExecutePayloadFragment", new FutureReceipt {PartID = 0, RequestID = messageId}).ConfigureAwait(false);

            var fragmentInvokeResult = hubConnection.InvokeCoreAsync("FetchExecutePayloadFragment", typeof(String), new FutureReceipt[] { new FutureReceipt { PartID = 0, RequestID = messageId } },new System.Threading.CancellationToken()).Result ;

            var execResult = serializer.Deserialize<ExecuteMessage>(fragmentInvokeResult as string) ?? new ExecuteMessage {HasError = true, Message = new StringBuilder("Deploy Failed")};
            toReturn.Add(new DeployResult(execResult, resource.ResourceName));

            if (doTriggerDeploy)
            {
                var triggersToDeploy = TriggersCatalog.LoadQueuesByResourceId(resourceId);
                var message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(triggersToDeploy));
                var triggerDeployRequest = new EsbExecuteRequest {ServiceName = "SaveTriggers"};
                triggerDeployRequest.AddArgument("resourceID", resourceId.ToString().ToStringBuilder());
                triggerDeployRequest.AddArgument("resourcePath", savePath);
                triggerDeployRequest.AddArgument("triggerDefinitions", serializer.SerializeToBuilder(message));
                var deployEnvelope = new Envelope
                {
                    Content = serializer.SerializeToBuilder(triggerDeployRequest).ToString(),
                    PartID = 0,
                    //Type = typeof(Envelope),
                };
                var deployMessageId = Guid.NewGuid();
                await hubConnection.InvokeAsync<Receipt>("ExecuteCommand", deployEnvelope, true, Guid.Empty, Guid.Empty, deployMessageId).ConfigureAwait(false);
                //var deployFragmentInvokeResult = await hubConnection.InvokeAsync<string>("FetchExecutePayloadFragment", new FutureReceipt {PartID = 0, RequestID = deployMessageId}).ConfigureAwait(false);

                var deployFragmentInvokeResult = hubConnection.InvokeCoreAsync("FetchExecutePayloadFragment", typeof(String), new FutureReceipt[] { new FutureReceipt { PartID = 0, RequestID = deployMessageId } }, new System.Threading.CancellationToken()).Result;
                    
                var deployExecResult = serializer.Deserialize<ExecuteMessage>(deployFragmentInvokeResult as string) ?? new ExecuteMessage {HasError = true, Message = new StringBuilder("Trigger Deploy Failed")};
                toReturn.Add(new DeployResult(deployExecResult, $"{resource.ResourceName} Triggers"));
            }

            if (doTestDeploy)
            {
                var testsToDeploy = TestCatalog.Fetch(resourceId);
                var message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(testsToDeploy));
                var testDeployRequest = new EsbExecuteRequest {ServiceName = "SaveTests"};
                testDeployRequest.AddArgument("resourceID", resourceId.ToString().ToStringBuilder());
                testDeployRequest.AddArgument("resourcePath", savePath);
                testDeployRequest.AddArgument("testDefinitions", serializer.SerializeToBuilder(message));
                var deployEnvelope = new Envelope
                {
                    Content = serializer.SerializeToBuilder(testDeployRequest).ToString(),
                    PartID = 0,
                    //Type = typeof(Envelope)
                };
                var deployMessageId = Guid.NewGuid();
                await hubConnection.InvokeAsync<Receipt>("ExecuteCommand", deployEnvelope, true, Guid.Empty, Guid.Empty, deployMessageId).ConfigureAwait(false);

                //var deployFragmentInvokeResult = await hubConnection.InvokeAsync<string>("FetchExecutePayloadFragment", new FutureReceipt {PartID = 0, RequestID = deployMessageId}).ConfigureAwait(false);

                var deployFragmentInvokeResult = hubConnection.InvokeCoreAsync("FetchExecutePayloadFragment", typeof(String), new FutureReceipt[] { new FutureReceipt { PartID = 0, RequestID = deployMessageId } }, new System.Threading.CancellationToken()).Result;

                var deployExecResult = serializer.Deserialize<ExecuteMessage>(deployFragmentInvokeResult as string) ?? new ExecuteMessage {HasError = true, Message = new StringBuilder("Tests Deploy Failed")};
                toReturn.Add(new DeployResult(deployExecResult, $"{resource.ResourceName} Tests"));
            }

            return toReturn;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => "DirectDeploy";
    }
}