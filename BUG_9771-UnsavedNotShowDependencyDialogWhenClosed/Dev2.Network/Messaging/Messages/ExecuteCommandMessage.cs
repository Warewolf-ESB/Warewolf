using System;

namespace Dev2.Network.Messaging.Messages
{
    public class ExecuteCommandMessage : NetworkMessage
    {
        public Guid DataListID { get; set; }
        public string Payload { get; set; }

        public override void Read(IByteReaderBase reader)
        {
            DataListID = reader.ReadGuid();
            Payload = reader.ReadString();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(DataListID);
            writer.Write(Payload);
        }
    }
}
