using System.Net.Sockets;
using System.Network;
using Moq;

namespace Dev2.DynamicServices.Test
{
    public abstract class MockConnection : Connection
    {
        protected MockConnection()
            : base(new Mock<MockNetworkHost>().Object, new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), NetworkDirection.Bidirectional)
        {

        }

        public int SendHitCount { get; set; }

        #region Overrides of Connection

        public override void Send(Packet p)
        {
            SendHitCount++;
        }


        #endregion
    }
}