using System.Net;
using System.Network;
using Dev2.Providers.Events;
using Dev2.Studio.Core.Network;

namespace Dev2.Core.Tests.Network
{
    public class TestTcpClientHost : TcpClientHostBase
    {
        readonly bool _pingResult;
        readonly bool _disconnectInvokesStartReconnectHeartbeat;

        public int StartReconnectHeartbeatHitCount { get; set; }
        public int StopReconnectHeartbeatHitCount { get; set; }
        public bool StartReconnectHeartbeatResult { get; set; }


        public TestTcpClientHost(bool pingResult = false, bool disconnectInvokesStartReconnectHeartbeat = true)
            : base(new EventPublisher())
        {
            _pingResult = pingResult;
            _disconnectInvokesStartReconnectHeartbeat = disconnectInvokesStartReconnectHeartbeat;
        }

        public override void Disconnect()
        {
            if(_disconnectInvokesStartReconnectHeartbeat)
            {
                // Overridden so that OnConnectionDisposed will invoke StartReconnectHeartbeat
                DisconnectImpl();
            }
            else
            {
                base.Disconnect(); // does not invoke StartReconnectHeartbeat
            }
        }

        public override bool StartReconnectHeartbeat(Connection connection)
        {
            StartReconnectHeartbeatHitCount++;
            StartReconnectHeartbeatResult = base.StartReconnectHeartbeat(connection);
            return StartReconnectHeartbeatResult;
        }

        public override void StopReconnectHeartbeat()
        {
            StopReconnectHeartbeatHitCount++;
            base.StopReconnectHeartbeat();
        }


        public override bool Ping(EndPoint endPoint)
        {
            return _pingResult;
        }
    }
}
