using Dev2.Network.Messages;
using System;
using System.Xml.Linq;

namespace Dev2.Network.Messaging.Messages
{
    public class SettingsMessage : INetworkMessage
    {
        #region Properties

        public long Handle { get; set; }

        public byte[] Assembly { get; set; }

        public string AssemblyHashCode { get; set; }

        public XElement ConfigurationXml { get; set; }

        public NetworkMessageAction Action { get; set; }

        public NetworkMessageResult Result { get; set; }


        #endregion

        #region Read

        public void Read(IByteReaderBase reader)
        {
            AssemblyHashCode = reader.ReadString();

            string configurationXml = reader.ReadString();
            try
            {
                ConfigurationXml = XElement.Parse(configurationXml);
            }
            catch(Exception)
            {
                //TODO 1018 Decide on empty xml
                ConfigurationXml = new XElement("NoData");
            }

            string action = reader.ReadString();
            NetworkMessageAction tmpAction;
            if (Enum.TryParse(action, true, out tmpAction))
            {
                Action = tmpAction;
            }
            else
            {
                Action = NetworkMessageAction.Unknown;
            }

            string result = reader.ReadString();
            NetworkMessageResult tmpResult;
            if (Enum.TryParse(result, true, out tmpResult))
            {
                Result = tmpResult;
            }
            else
            {
                Result = NetworkMessageResult.Unknown;
            }

            Assembly = reader.ReadByteArray();
        }

        #endregion

        #region Write

        public void Write(IByteWriterBase writer)
        {
            writer.Write(AssemblyHashCode);
            writer.Write((ConfigurationXml != null) ? ConfigurationXml.ToString() : "</NoData>");
            writer.Write(Action.ToString());
            writer.Write(Result.ToString());

            __IByteWriterBaseExtensions.Write(writer, Assembly ?? new byte[0]);
        }

        #endregion
    }
}