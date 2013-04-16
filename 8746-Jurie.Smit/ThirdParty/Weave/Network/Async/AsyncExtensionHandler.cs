using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Network
{
    public abstract class AsyncExtensionHandler
    {
        private PacketTemplate _template;

        public PacketTemplate Template { get { return _template; } }

        public AsyncExtensionHandler(PacketTemplate template)
        {
            if (template == null) throw new ArgumentNullException("template");
            _template = template;
        }

        public abstract void Dispatch(AsyncPacketHandlerCollection channel, Connection connection, PacketData extension, PacketData packet);
        public abstract void Response(AsyncPacketHandlerCollection channel, Connection connection, PacketData extension, PacketData packet);
    }
}
