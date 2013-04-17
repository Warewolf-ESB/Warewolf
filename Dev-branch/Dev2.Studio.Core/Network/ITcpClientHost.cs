using System;
using System.Network;
using System.Security.Principal;
using System.Threading.Tasks;
using Dev2.Diagnostics;
using Dev2.Network;
using Dev2.Network.Messaging;

namespace Dev2.Studio.Core.Network
{
    public interface ITcpClientHost : INetworkOperator, IDisposable
    {
        event EventHandler<LoginStateEventArgs> LoginStateChanged;

        event EventHandler<NetworkStateEventArgs> NetworkStateChanged;

        bool IsAuxiliary { get; }

        Guid ServerID { get; }

        Guid AccountID { get; }

        bool IsConnected { get; }

        IStudioNetworkMessageAggregator MessageAggregator { get; }

        INetworkMessageBroker MessageBroker { get; }

        void AddDebugWriter(IDebugWriter writer);

        void RemoveDebugWriter(IDebugWriter writer);

        void RemoveDebugWriter(Guid writerID);

        Task<bool> ConnectAsync(string hostNameOrAddress, int port);

        Task<bool> LoginAsync(IIdentity identity);

        Task<bool> LoginAsync(string userName, string password);

        void SendNetworkMessage(INetworkMessage message);

        INetworkMessage RecieveNetworkMessage(IByteReaderBase reader);

        INetworkMessage SendReceiveNetworkMessage(INetworkMessage message);

        string ExecuteCommand(string payload, Guid workspaceID, Guid dataListID);

        void Disconnect();
    }
}