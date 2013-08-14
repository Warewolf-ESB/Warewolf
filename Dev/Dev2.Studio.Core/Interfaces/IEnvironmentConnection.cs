using System;
using System.Network;
using Caliburn.Micro;
using Dev2.DataList.Contract.Network;
using Dev2.Diagnostics;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentConnection
    {
        // PBI 6690 - 2013.07.04 - TWR : added
        IEventPublisher ServerEvents { get; }

        Guid ServerID { get; }
        Guid WorkspaceID { get; }
        IFrameworkSecurityContext SecurityContext { get; }
        IStudioNetworkMessageAggregator MessageAggregator { get; }
        INetworkMessageBroker MessageBroker { get; }
        IDebugWriter DebugWriter { get; }

        Uri AppServerUri { get; }
        Uri WebServerUri { get; }

        event EventHandler<LoginStateEventArgs> LoginStateChanged;
        event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        event EventHandler<ServerStateEventArgs> ServerStateChanged;

        void SendNetworkMessage(INetworkMessage message);
        INetworkMessage RecieveNetworkMessage(IByteReaderBase reader);
        INetworkMessage SendReceiveNetworkMessage(INetworkMessage message);
        string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID);
        void AddDebugWriter();
        void RemoveDebugWriter();

        bool IsConnected { get; }
        bool IsAuxiliary { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }
        IStudioEsbChannel DataChannel { get; }
        INetworkExecutionChannel ExecutionChannel { get; }
        INetworkDataListChannel DataListChannel { get; }

        void Connect(bool isAuxiliary = false);
        void Disconnect();

        // BUG 9634 - 2013.07.17 - TWR : added
        void Verify(Action<ConnectResult> callback);

    }
}
