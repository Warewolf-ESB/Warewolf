using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Network
{
    public interface INetworkChannel
    {
        object Context { get; set; }
    }
}
