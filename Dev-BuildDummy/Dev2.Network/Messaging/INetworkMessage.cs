using System;

namespace Dev2.Network.Messaging
{
    public interface INetworkMessage
    {
        long Handle { get; set; }
        bool HasError { get; set; }
        string ErrorMessage { get; set; }

        void Read(IByteReaderBase reader);
        void Write(IByteWriterBase writer);
    }

}
