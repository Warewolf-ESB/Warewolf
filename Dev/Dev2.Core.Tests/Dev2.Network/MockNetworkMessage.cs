using System;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Tests.Dev2.Network
{
    public class MockNetworkMessage : NetworkMessage
    {
        #region Implementation of INetworkMessage

        public override void Read(IByteReaderBase reader)
        {
            Handle = reader.ReadInt64();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(Handle);
        }

        #endregion
    }
}
