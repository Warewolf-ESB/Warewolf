using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Network
{
    public abstract class IOCPExtensionHandler<T> where T : NetworkContext
    {
        private PacketTemplate _template;

        public PacketTemplate Template { get { return _template; } }

        public IOCPExtensionHandler(PacketTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _template = template;
        }

        protected internal abstract void Dispatch(IOCPPacketHandlerCollection<T> channel, T context, PacketData extension, PacketData packet);
        protected internal abstract void Response(IOCPPacketHandlerCollection<T> channel, T context, PacketData extension, PacketData packet);
    }
}
