using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.DataList.Contract.Network
{
    public class DeleteDataListMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public Guid ID { get; set; }
        public bool OnlyIfNotPersisted { get; set; }

        public DeleteDataListMessage()
        {
        }

        public DeleteDataListMessage(long handle, Guid id, bool onlyIfNotPersisted)
        {
            Handle = handle;
            ID = id;
            OnlyIfNotPersisted = onlyIfNotPersisted;
        }
        
        public void Read(IByteReaderBase reader)
        {
            ID = reader.ReadGuid();
            OnlyIfNotPersisted = reader.ReadBoolean();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(ID);
            writer.Write(OnlyIfNotPersisted);
        }
    }

    public class DeleteDataListResultMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public ErrorResultTO Errors { get; set; }

        public DeleteDataListResultMessage()
        {
        }

        public DeleteDataListResultMessage(long handle, ErrorResultTO errors)
        {
            Handle = handle;
            Errors = errors;
        }

        public void Read(IByteReaderBase reader)
        {
            //Nothing to do since handle is the only property and it is handled by the broker
        }

        public void Write(IByteWriterBase writer)
        {
            //Nothing to do since handle is the only property and it is handled by the broker
        }
    }
}
