using System;
using System.Net;
using Dev2.Network.Messaging;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpClientHost : TcpClientHostBase
    {
        public TestTcpClientHost()
            : base(new EventPublisher())
        {
        }

        public override bool Ping(EndPoint endPoint)
        {
            return false;
        }

        public int StartReconnectTimerHitCount { get; private set; }
        protected override void StartReconnectTimer()
        {
            StartReconnectTimerHitCount++;
            
        }

        /// <summary>
        /// Tests the complete send receive.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="msg">The MSG.</param>
        public void TestCompleteSendReceive<TMessage>(Func<TMessage> msg) where TMessage : INetworkMessage
        {
            CompleteSendReceive(msg);
        }
    }
}
