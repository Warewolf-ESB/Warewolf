using System;
using System.Xml.Linq;

namespace Dev2.Network.Messaging.Messages
{
    public class SettingsMessage : ISettingsMessage
    {
        #region Properties

        public long Handle { get; set; }

        public byte[] Assembly { get; set; }

        public byte[] AssemblyHash { get; set; }

        public XElement Settings { get; set; }

        public NetworkMessageAction Action { get; set; }

        public NetworkMessageResult Result { get; set; }


        #endregion

        #region Read

        public void Read(IByteReaderBase reader)
        {
        }

        #endregion

        #region Write

        public void Write(IByteWriterBase writer)
        {
        }

        #endregion

    }
}