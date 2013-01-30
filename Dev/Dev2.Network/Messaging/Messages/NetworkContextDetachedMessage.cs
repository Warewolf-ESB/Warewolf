using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.Network.Messaging.Messages
{

    /// <summary>
    /// System message which is published when a netowrk context is detached
    /// </summary>
    public class NetworkContextDetachedMessage : INetworkMessage
    {
        public long Handle { get; set; }

        public void Read(IByteReaderBase reader)
        {
            //Nothing needs to happen here because there are no properties
        }

        public void Write(IByteWriterBase writer)
        {
            //Nothing needs to happen here because there are no properties
        }
    }
}
