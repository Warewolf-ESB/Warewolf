using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddServerToDeployMessage
    {
        public IServer Server { get; set; }
        public bool IsSource { get; set; }
        public bool IsDestination { get; set; }

        public AddServerToDeployMessage(IServer server, bool isSource, bool isDestination)
        {
            Server = server;
            IsSource = isSource;
            IsDestination = isDestination;
        }
    }
}
