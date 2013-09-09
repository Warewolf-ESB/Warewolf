using System;

namespace Dev2.Studio.Core.Network
{
    public class TcpClientAdapter
    {
        public bool IsConnected { get; private set; }
        public bool IsAuxiliary { get; private set; } 


        public bool Connect(Uri address)
        {
            if(address == null)
            {
                throw new ArgumentNullException("address");
            }
            return false;
        }
    }
}
