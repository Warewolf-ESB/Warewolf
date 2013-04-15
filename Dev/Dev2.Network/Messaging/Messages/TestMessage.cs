using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Network.Messaging.Messages
{
    /// <summary>
    /// System message used for testing
    /// </summary>
    public class TestMessage : NetworkMessage
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

        public string StringVal { get; set; }
        public int IntVal { get; set; }

        #endregion Properties

        #region INetworkMessage

        public override void Read(IByteReaderBase reader)
        {
            StringVal = reader.ReadString();
            IntVal = reader.ReadInt32();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(StringVal);
            writer.Write(IntVal);
        }

        #endregion INetworkMessage
    }
}
