using System;
using System.Network;
using Dev2.Network.Messaging;

namespace Dev2.Network
{
    public interface INetworkMessageBroker
    {
        void Send<T>(T message, INetworkOperator networkOperator) where T : INetworkMessage;
        INetworkMessage Receive(IByteReaderBase reader);
    }
}
