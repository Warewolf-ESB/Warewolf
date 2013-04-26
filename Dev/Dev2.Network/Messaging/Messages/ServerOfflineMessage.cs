using System;

namespace Dev2.Network.Messaging.Messages
{

    /// <summary>
    /// System message which is published when a network server is offline
    /// </summary>
    public class ServerOfflineMessage : NetworkMessage
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
