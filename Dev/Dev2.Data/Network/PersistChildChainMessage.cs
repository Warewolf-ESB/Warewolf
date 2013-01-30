using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Network.Messages;

namespace Dev2.DataList.Contract.Network
{
    public class PersistChildChainMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public Guid ID { get; set; }

        public PersistChildChainMessage()
        {
        }

        public PersistChildChainMessage(long handle, Guid id)
        {
            Handle = handle;
            ID = id;
        }

        public void Read(IByteReaderBase reader)
        {
            ID = reader.ReadGuid();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(ID);
        }
    }

    public class PersistChildChainResultMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public bool Result { get; set; }
        public ErrorResultTO Errors { get; set; }

        public PersistChildChainResultMessage()
        {
        }

        public PersistChildChainResultMessage(long handle, bool result, ErrorResultTO errors)
        {
            Handle = handle;
            Result = result;
            Errors = errors;
        }

        public void Read(IByteReaderBase reader)
        {
            Result = reader.ReadBoolean();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(Result);
        }
    }
}
