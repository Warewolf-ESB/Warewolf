using System;
using System.Network;
using Caliburn.Micro;
using Dev2.DataList.Contract.Network;
using Dev2.Diagnostics;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Studio.Core.Network;

namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentConnection
    {
        Guid ServerID { get; }
        Guid WorkspaceID { get; }
        IFrameworkSecurityContext SecurityContext { get; }
        IEventAggregator EventAggregator { get; }
        IStudioNetworkMessageAggregator MessageAggregator { get; }
        INetworkMessageBroker MessageBroker { get; }

        Uri AppServerUri { get; }
        Uri WebServerUri { get; }

        event EventHandler<LoginStateEventArgs> LoginStateChanged;
        event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        event EventHandler<ServerStateEventArgs> ServerStateChanged;

        void SendNetworkMessage(INetworkMessage message);
        INetworkMessage RecieveNetworkMessage(IByteReaderBase reader);
        INetworkMessage SendReceiveNetworkMessage(INetworkMessage message);
        string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID);
        void AddDebugWriter(IDebugWriter writer);
        void RemoveDebugWriter(Guid writerID);

        bool IsConnected { get; }
        bool IsAuxiliary { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }
        IStudioEsbChannel DataChannel { get; }
        INetworkExecutionChannel ExecutionChannel { get; }
        INetworkDataListChannel DataListChannel { get; }

        void Connect(bool isAuxiliary = false);
        void Disconnect();

    }
}
