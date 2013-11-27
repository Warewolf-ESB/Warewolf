using System;
using System.Net;
using System.Network;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Diagnostics;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Studio.Core.Network;
using Moq;

namespace Dev2.Core.Tests.Network
{
    public abstract class TestTcpClientHostAsync : ITcpClientHost
    {
        #region Implementation of INetworkOperator

        public abstract void Send(Packet p);

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        #endregion

        #region Implementation of ITcpClientHost

        public abstract event EventHandler<LoginStateEventArgs> LoginStateChanged;
        public abstract event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        public abstract event EventHandler<ServerStateEventArgs> ServerStateChanged;
        public abstract bool IsAuxiliary { get; }
        public abstract Guid ServerID { get; }
        public abstract Guid AccountID { get; }
        public abstract IDebugWriter DebugWriter { get; }

        public abstract void AddDebugWriter();

        public abstract void RemoveDebugWriter();

        public abstract void SendNetworkMessage(INetworkMessage message);

        public abstract INetworkMessage RecieveNetworkMessage(IByteReaderBase reader);

        public abstract INetworkMessage SendReceiveNetworkMessage(INetworkMessage message);

        public abstract string ExecuteCommand(string payload, Guid workspaceID, Guid dataListID);

        public abstract void Disconnect();

        public abstract bool StartReconnectHeartbeat(IPAddress address, int port);

        public abstract Task StartReconnectHeartbeat(string hostNameOrAddress, int port);

        #endregion

        public INetworkMessageBroker MessageBroker { get { return new Mock<INetworkMessageBroker>().Object; } }
        public IStudioNetworkMessageAggregator MessageAggregator { get { return new Mock<IStudioNetworkMessageAggregator>().Object; } }
        public bool IsConnected { get { return false; } }

        public int ConnectAsyncDelay { get; set; }
        public bool ConnectAsyncResult { get; set; }
        public int ConnectAsyncHitCount { get; set; }
        public bool LoginAsyncResult { get; set; }
        public int LoginAsyncHitCount { get; set; }

        public async Task<bool> ConnectAsync(string hostNameOrAddress, int port)
        {
            ConnectAsyncHitCount++;
            var task = Task<bool>.Factory.StartNew(() =>
            {
                if(ConnectAsyncDelay > 0)
                {
                    Thread.Sleep(ConnectAsyncDelay);
                }
                return ConnectAsyncResult;
            });
            await task;
            return task.Result;
        }

        public async Task<bool> LoginAsync(IIdentity identity)
        {
            return await LoginAsync("testUser", "abc123xyz");
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            LoginAsyncHitCount++;
            var task = Task<bool>.Factory.StartNew(() => LoginAsyncResult);
            await task;
            return task.Result;
        }

    }
}
