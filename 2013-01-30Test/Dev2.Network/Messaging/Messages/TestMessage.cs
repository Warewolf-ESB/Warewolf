using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.Network.Messaging.Messages
{
    /// <summary>
    /// System message used for testing
    /// </summary>
    public class TestMessage : INetworkMessage
    {
        #region Constructors

        public TestMessage()
        {
        }

        public TestMessage(string stringVal, int intVal)
        {
            StringVal = stringVal;
            IntVal = intVal;
        }

        #endregion Constructors

        #region Properties

        public long Handle { get; set; }
        public string StringVal { get; set; }
        public int IntVal { get; set; }

        #endregion Properties

        #region INetworkMessage

        public void Read(IByteReaderBase reader)
        {
            StringVal = reader.ReadString();
            IntVal = reader.ReadInt32();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(StringVal);
            writer.Write(IntVal);
        }

        #endregion INetworkMessage
    }
}
