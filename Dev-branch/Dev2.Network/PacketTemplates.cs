using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Network
{
    public static class PacketTemplates
    {
        // These should eventually be replaced buy the generic two way coms
        public static readonly PacketTemplate Server_OnAuxiliaryConnectionRequested = new PacketTemplate(0, 0, true);
        public static readonly PacketTemplate Client_OnAuxiliaryConnectionReply = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Server_OnDebugWriterAddition = new PacketTemplate(0, 1, true);
        public static readonly PacketTemplate Server_OnDebugWriterSubtraction = new PacketTemplate(0, 2, true);

        public static readonly PacketTemplate Client_OnDebugWriterWrite = new PacketTemplate(0, 1, true);
        // These should eventually be replaced buy the generic two way coms

        //Packet temmplate used for generic two way communication
        public static readonly PacketTemplate Both_OnNetworkMessageRevieved = new PacketTemplate(1, 0, true);
    }
}
