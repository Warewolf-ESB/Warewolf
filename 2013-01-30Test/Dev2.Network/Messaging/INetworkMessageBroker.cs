using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using System.Threading.Tasks;
using Dev2.Network.Messages;

namespace Dev2.Network
{
    public interface INetworkMessageBroker
    {
        void Send<T>(T message, INetworkOperator networkOperator) where T : INetworkMessage, new();
        INetworkMessage Recieve(IByteReaderBase reader);
    }
}
