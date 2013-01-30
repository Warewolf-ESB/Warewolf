using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.Network.Messaging.Messages
{
    public class ErrorMessage : INetworkMessage
    {
        #region Constructors

        public ErrorMessage()
        {
        }

        public ErrorMessage(long handle, string message)
        {
            Message = message;
            Handle = handle;
        }

        #endregion Constructors

        #region Properties

        public long Handle { get; set; }
        public string Message { get; set; }

        #endregion Properties

        #region INetworkMessage

        public void Read(IByteReaderBase reader)
        {
            Message = reader.ReadString();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(Message);
        }

        #endregion INetworkMessage
    }
}
