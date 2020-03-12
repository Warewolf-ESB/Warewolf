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
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.ServiceModel;
using Dev2.Workspaces;
using Dev2.Runtime.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Warewolf.Configuration;
using Warewolf.Service;
using Connection = Dev2.Data.ServiceModel.Connection;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestClusterConnection : EsbManagementEndpointBase
    {
        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            Dev2Logger.Info("Test Cluster Connection", GlobalConstants.WarewolfInfo);
            values.TryGetValue("ClusterSettingsData", out StringBuilder resourceDefinition);
            var clusterSettings = serializer.Deserialize<ClusterSettingsData>(resourceDefinition);
            
            var tc = new TestConn(serializer, clusterSettings);
            var result = tc.TestConnection();
            return serializer.SerializeToBuilder(result);
        }

        class TestConn
        {
            private readonly Dev2JsonSerializer _serializer;
            private readonly ClusterSettingsData _clusterSettings;
            private IConnections _connections;
            private IResourceCatalog _resourceCatalog;

            public TestConn(Dev2JsonSerializer serializer, ClusterSettingsData clusterSettings)
            {
                _serializer = serializer;
                _clusterSettings = clusterSettings;
            }

            public ExecuteMessage TestConnection()
            {
                var resource = ResourceCatalog.GetResource(GlobalConstants.ServerWorkspaceID, _clusterSettings.LeaderServerResource.Value);
                var destination = resource as Connection;
                
                var canConnectToServer = Connections.CanConnectToServer(destination);
                if (!canConnectToServer.IsValid)
                {
                    return new ExecuteMessage {HasError = true, Message = new StringBuilder(canConnectToServer.ErrorMessage)};
                }

                if (!string.IsNullOrWhiteSpace(_clusterSettings.LeaderServerKey))
                {
                    var esbExecuteRequest = new EsbExecuteRequest {ServiceName = Cluster.TestClusterLeaderConnection};
                    esbExecuteRequest.AddArgument("LeaderServerKey", _clusterSettings.LeaderServerKey.ToStringBuilder());

                    var envelope = BuildEnvelope(esbExecuteRequest);
                    var result = InvokeRemoteInternalServiceAsync(destination, envelope).Result;
                    if (!result.Success)
                    {
                        return new ExecuteMessage {HasError = true, Message = new StringBuilder("failed verifying cluster key with leader")};
                    }
                    return new ExecuteMessage {HasError = false};
                }

                return new ExecuteMessage {HasError = true, Message = new StringBuilder("no cluster key for leader specified")};
            }

            public IConnections Connections
            {
                private get => _connections ?? (_connections = new Connections());
                set => _connections = value;
            }

            public IResourceCatalog ResourceCatalog
            {
                private get => _resourceCatalog ?? Hosting.ResourceCatalog.Instance;
                set => _resourceCatalog = value;
            }

            private async Task<TestClusterResult> InvokeRemoteInternalServiceAsync(Connection destination, Envelope envelope)
            {
                var proxy = Connections.CreateHubProxy(destination);
                var messageId = Guid.NewGuid();

                return proxy.Invoke<TestClusterResult>("ExecuteCommand", envelope, true, Guid.Empty, Guid.Empty, messageId).Result;
            }

            private Envelope BuildEnvelope(EsbExecuteRequest esbExecuteRequest)
            {
                var envelope = new Envelope
                {
                    Content = _serializer.SerializeToBuilder(esbExecuteRequest).ToString(),
                    PartID = 0,
                    Type = typeof(Envelope)
                };
                return envelope;
            }
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceDefinition ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => Cluster.TestClusterConnection;
    }

    public class TestClusterResult : ExecuteMessage
    {
        public bool Success { get; set; }
    }
}
