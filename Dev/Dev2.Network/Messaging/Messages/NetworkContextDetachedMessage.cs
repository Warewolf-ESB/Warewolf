using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Network.Messaging.Messages
{

    /// <summary>
    /// System message which is published when a netowrk context is detached
    /// </summary>
    public class NetworkContextDetachedMessage : NetworkMessage
    {
        #region Overrides of NetworkMessage

        public override void Read(IByteReaderBase reader)
        {
        }

        public override void Write(IByteWriterBase writer)
        {
        }

        #endregion
    }
}
