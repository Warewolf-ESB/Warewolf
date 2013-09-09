using System;

namespace Dev2.Network.Messaging.Messages
{
    public abstract class NetworkMessage : INetworkMessage
    {
        public long Handle { get; set; }

        public bool HasError { get; set; }

        public string ErrorMessage { get; set; }

        public abstract void Read(IByteReaderBase reader);

        public abstract void Write(IByteWriterBase writer);
    }
}