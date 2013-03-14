using Dev2.Network.Messages;
using System;

namespace Dev2.Tests.Dev2.Network
{
    public class MockNetworkMessage : INetworkMessage
    {
        #region Implementation of INetworkMessage

        public long Handle { get; set; }
        public void Read(IByteReaderBase reader)
        {
            Handle = reader.ReadInt64();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(Handle);
        }

        #endregion
    }
}
