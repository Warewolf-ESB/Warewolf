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
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.SignalR.Wrappers;
using Dev2.Studio.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Warewolf.Esb;

namespace Warewolf.Common
{
    public interface IResourceCatalogProxy
    {
        T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T : class;
    }
    public class ResourceCatalogProxy : IResourceCatalogProxy
    {
        readonly IEnvironmentConnection _environmentConnection;
        public ResourceCatalogProxy(IEnvironmentConnection environmentConnection)
        {
            _environmentConnection = environmentConnection;
        }

        public T GetResourceById<T>(Guid workspaceId, Guid resourceId) where T: class
        {
            var communicationController = new CommunicationController
            {
                ServiceName = nameof(Service.GetResourceById)
            };
            communicationController.AddPayloadArgument(Service.GetResourceById.WorkspaceId, workspaceId.ToString());
            communicationController.AddPayloadArgument(Service.GetResourceById.ResourceId, resourceId.ToString());
            var result = communicationController.ExecuteCommand<T>(_environmentConnection, workspaceId);

            return result;
        }
    }

    public class ResourceRequest<T> : ICatalogRequest
    {
        private readonly Guid _workspaceId;
        private readonly Guid _resourceId;

        public ResourceRequest(Guid workspaceId, Guid resourceId)
        {
            _workspaceId = workspaceId;
            _resourceId = resourceId;
        }

        public IEsbRequest Build()
        {
            var servicePayload = new EsbExecuteRequest
            {
                ServiceName = nameof(Service.GetResourceById)
            };
            servicePayload.AddArgument(Service.GetResourceById.WorkspaceId, new StringBuilder(_workspaceId.ToString()));
            servicePayload.AddArgument(Service.GetResourceById.ResourceId, new StringBuilder(_resourceId.ToString()));
            return servicePayload;
        }
    }

    public static class ProxyWrapperExtensionMethods
    {
        public static Task<T> NewResourceRequest<T>(this IEnvironmentConnection _environmentConnection, Guid workspaceId, Guid resourceId) where T : class
        {
            var req = new ResourceRequest<T>(workspaceId, resourceId);
            var task = _environmentConnection.ExecuteCommandAsync(req, workspaceId)
                .ContinueWith(t =>
                {
                    var serializer = new Dev2JsonSerializer();
                    var payload = t.Result;
                    return serializer.Deserialize<T>(payload);
                });
            return task;
        }
/*
        public static Task<T> ExecReq<T>(this IHubProxy proxy)
        {
            var serializer = new Dev2JsonSerializer();

            var testDeployRequest = new EsbExecuteRequest {ServiceName = "SaveTests"};
            testDeployRequest.AddArgument("resourceID", resourceId.ToString().ToStringBuilder());
            testDeployRequest.AddArgument("resourcePath", savePath);
            testDeployRequest.AddArgument("testDefinitions", serializer.SerializeToBuilder(message));

            return ExecReq2<T>(proxy);
        }*/
        public static Task<T> ExecReq2<T>(this IHubProxy proxy, ICatalogRequest request)
        {
            var serializer = new Dev2JsonSerializer();
            var deployEnvelope = new Envelope
            {
                Content = serializer.Serialize(request.Build()),
                PartID = 0,
                Type = typeof(Envelope)
            };
            var messageId = Guid.NewGuid();
            return proxy.Invoke<Receipt>("ExecuteCommand", deployEnvelope, true, Guid.Empty, Guid.Empty, messageId)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        return default;
                    }
                    return proxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt {PartID = 0, RequestID = messageId})
                        .ContinueWith((task1) =>
                        {
                            var payload = task1.Result;
                            return serializer.Deserialize<T>(payload);
                        }).Result;
                });
        }

        public static HubWatcher<T> Watch<T>(this IHubProxyWrapper proxy, ICatalogSubscribeRequest request)
        {
            return new HubWatcher<T>(proxy);
        }
    }

    public class HubWatcher<T>
    {
        private ISubscriptionWrapper _registeredEventWatcher;

        public HubWatcher(IHubProxyWrapper proxy)
        {
            _registeredEventWatcher = proxy.Subscribe(typeof(T).Name);
            _registeredEventWatcher.Received += (tokens) =>
            {
                if (tokens.Count > 0)
                {
                    var o = tokens[0].ToObject<T>();
                    OnChange?.Invoke(o);
                }
                else
                {
                    Dev2Logger.Error("watcher stream encountered empty value", GlobalConstants.WarewolfError);
                }
            };
        }

        public event Action<T> OnChange;
    }
}
