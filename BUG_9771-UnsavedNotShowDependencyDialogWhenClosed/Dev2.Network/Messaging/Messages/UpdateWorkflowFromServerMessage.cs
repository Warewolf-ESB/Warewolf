using System;

namespace Dev2.Network.Messaging.Messages
{
    /// <summary>
    /// System message to say that a workflow needs to be updated from the server
    /// </summary>
    public class UpdateWorkflowFromServerMessage : NetworkMessage
    {
        public Guid ResourceID { get; set; }

        #region Overrides of NetworkMessage

        public override void Read(IByteReaderBase reader)
        {
            ResourceID = reader.ReadGuid();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(ResourceID);
        }

        #endregion
    }
}