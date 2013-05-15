using System;

namespace Dev2.Network.Messaging.Messages
{
    public class ErrorMessage : NetworkMessage
    {
        #region Constructors

        public ErrorMessage()
        {
        }

        public ErrorMessage(long handle, string message)
        {
            Message = message;
            Handle = handle;
            HasError = true;
            ErrorMessage = message;
        }

        #endregion Constructors

        #region Properties

        public string Message { get; set; }

        #endregion Properties

        #region INetworkMessage

        public override void Read(IByteReaderBase reader)
        {
            Message = reader.ReadString();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(Message);
        }

        #endregion INetworkMessage
    }
}
