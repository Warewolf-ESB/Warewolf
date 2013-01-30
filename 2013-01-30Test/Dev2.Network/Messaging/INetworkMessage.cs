using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Network.Messages
{
    public interface INetworkMessage
    {
        long Handle { get; set; }
        void Read(IByteReaderBase reader);
        void Write(IByteWriterBase writer);
    }
}
