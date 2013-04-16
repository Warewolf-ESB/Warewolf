using System;
using Dev2.Network.Messaging;

namespace Dev2.DataList.Contract.Network
{
    public abstract class DataListMessage : INetworkMessage
    {
        public long Handle { get; set; }

        public bool HasError
        {
            get
            {
                return Errors.HasErrors();
            }
            set
            {
               
            }
        }

        public string ErrorMessage
        {
            get
            {
                return string.Join("\n", Errors.FetchErrors());
            }
            set
            {
                Errors.AddError(value);
            }
        }

        public ErrorResultTO Errors { get; set; }

        public abstract void Read(IByteReaderBase reader);

        public abstract void Write(IByteWriterBase writer);
    }
}