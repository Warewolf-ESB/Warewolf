using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Network;

namespace Dev2.DynamicServices
{
    public static class StudioTemplates
    {
        public static readonly PacketTemplate Server_OnAuxiliaryConnectionRequested = new PacketTemplate(0, 0, true);
        public static readonly PacketTemplate Client_OnAuxiliaryConnectionReply = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Server_OnDebugWriterAddition = new PacketTemplate(0, 1, true);
        public static readonly PacketTemplate Server_OnDebugWriterSubtraction = new PacketTemplate(0, 2, true);

        public static readonly PacketTemplate Client_OnDebugWriterWrite = new PacketTemplate(0, 1, true); 

    }
}
