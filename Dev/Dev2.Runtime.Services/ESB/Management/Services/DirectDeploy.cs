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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.AspNet.SignalR.Client;
using Dev2.Runtime.Interfaces;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Deploy a resource
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class DirectDeploy : IEsbManagementEndpoint
    {
        private bool _existingResource;
        private IConnections _connections;
        private IResourceCatalog _resourceCatalog;
        private ITestCatalog _testCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            _existingResource = false;
            StringBuilder resourceDefinition;
            requestArgs.TryGetValue("ResourceDefinition", out resourceDefinition);
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
                toReturn.Add(new DeployResult(new ExecuteMessage { HasError = true, Message = new StringBuilder("No WorkSpace or Values specified") }, "An Error has occurred"));
            }
            else
            {
                StringBuilder destinationEnvironment;

                values.TryGetValue("destinationEnvironmentId", out destinationEnvironment);


                if (destinationEnvironment == null)
                {
                    toReturn.Add(new DeployResult(new ExecuteMessage { HasError = true, Message = new StringBuilder("destinationEnvironment is null") }, "An Error has occurred"));
                }
                else
                {
                    var destination = serializer.Deserialize<Data.ServiceModel.Connection>(destinationEnvironment);
                    var canConnectToServer = Connections.CanConnectToServer(destination);
                    if (!canConnectToServer.IsValid)
                    {
                        toReturn.Add(new DeployResult(new ExecuteMessage { HasError = true, Message = new StringBuilder(canConnectToServer.ErrorMessage) }, "An Error has occurred"));
                    }
                    else
                    {
                        var proxy = Connections.CreateHubProxy(destination);

                        var roles = new StringBuilder("*");

                        StringBuilder deployTests;

                        values.TryGetValue("deployTests", out deployTests);
                        bool doTestDeploy;

                        if (deployTests == null)
                        {
                            toReturn.Add(new DeployResult(new ExecuteMessage { HasError = true, Message = new StringBuilder("deployTests is null") }, "An Error has occurred"));
                        }
                        else
                        {
                            doTestDeploy = bool.Parse(deployTests.ToString());

                            StringBuilder resourceIDsToDeploy;

                            values.TryGetValue("resourceIDsToDeploy", out resourceIDsToDeploy);
                            var idsToDeploy = serializer.Deserialize<List<Guid>>(resourceIDsToDeploy);
                            if (idsToDeploy.Any())
                            {
                                Parallel.ForEach(idsToDeploy, resourceId => toReturn.AddRange(DeployResource(resourceId, roles, serializer, proxy, doTestDeploy)));
                            }
                            else
                            {
                                toReturn.Add(new DeployResult(new ExecuteMessage { HasError = true, Message = new StringBuilder("No resources specified") }, "An Error has occurred"));
                            }
                        }
                    }
                }
            }

            return serializer.SerializeToBuilder(toReturn);
        }

        public IConnections Connections
        {
            private get
            {
                return _connections ?? (_connections = new Connections());
            }
            set
            {
                _connections = value;
            }
        }

        public ITestCatalog TestCatalog
        {
            private get
            {
                return _testCatalog ?? Runtime.TestCatalog.Instance;
            }
            set
            {
                _testCatalog = value;
            }
        }

        public IResourceCatalog ResourceCatalog
        {
            private get
            {
                return _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }

        private IEnumerable<DeployResult> DeployResource(Guid resourceId, StringBuilder roles, Dev2JsonSerializer serializer, IHubProxy proxy, bool doTestDeploy)
        {
            var toReturn = new List<DeployResult>();
            var savePath = new StringBuilder();
            var resourceContent = ResourceCatalog.GetResourceContents(GlobalConstants.ServerWorkspaceID, resourceId);
            var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId);
            if (!resource.IsService)
            {
                var fetchResourceService = new FetchResourceDefinition();
                resourceContent = fetchResourceService.DecryptAllPasswords(resourceContent);
            }
            savePath.Append(resource.GetSavePath());

            var esbExecuteRequest = new EsbExecuteRequest { ServiceName = "DeployResourceService" };
            esbExecuteRequest.AddArgument("savePath", savePath);
            esbExecuteRequest.AddArgument("ResourceDefinition", resourceContent);
            esbExecuteRequest.AddArgument("Roles", roles);
            Envelope envelope = new Envelope
            {
                Content = serializer.SerializeToBuilder(esbExecuteRequest).ToString(),
                PartID = 0,
                Type = typeof(Envelope)
            };
            var messageId = Guid.NewGuid();
            proxy.Invoke<Receipt>("ExecuteCommand", envelope, true, Guid.Empty, Guid.Empty, messageId).Wait();
            Task<string> fragmentInvoke = proxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = 0, RequestID = messageId });
            var fragmentInvokeResult = fragmentInvoke.Result;
            var execResult = serializer.Deserialize<ExecuteMessage>(fragmentInvokeResult) ?? new ExecuteMessage { HasError = true, Message = new StringBuilder("Deploy Fialed") };
            toReturn.Add(new DeployResult(execResult, resource.ResourceName));

            if (doTestDeploy)
            {
                var testsToDeploy = TestCatalog.Fetch(resourceId);
                CompressedExecuteMessage message = new CompressedExecuteMessage();
                message.SetMessage(serializer.Serialize(testsToDeploy));
                var testDeployRequest = new EsbExecuteRequest { ServiceName = "SaveTests" };
                testDeployRequest.AddArgument("resourceID", resourceId.ToString().ToStringBuilder());
                testDeployRequest.AddArgument("resourcePath", savePath);
                testDeployRequest.AddArgument("testDefinitions", serializer.SerializeToBuilder(message));
                Envelope deployEnvelope = new Envelope
                {
                    Content = serializer.SerializeToBuilder(testDeployRequest).ToString(),
                    PartID = 0,
                    Type = typeof(Envelope)
                };
                var deployMessageId = Guid.NewGuid();
                proxy.Invoke<Receipt>("ExecuteCommand", deployEnvelope, true, Guid.Empty, Guid.Empty, deployMessageId).Wait();
                Task<string> deployFragmentInvoke = proxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = 0, RequestID = deployMessageId });
                var deployFragmentInvokeResult = deployFragmentInvoke.Result;
                var deployExecResult = serializer.Deserialize<ExecuteMessage>(deployFragmentInvokeResult) ?? new ExecuteMessage { HasError = true, Message = new StringBuilder("Deploy Fialed") };
                toReturn.Add(new DeployResult(deployExecResult, $"{resource.ResourceName} Tests"));
            }
            return toReturn;
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
            return "DirectDeploy";
        }
    }
}
