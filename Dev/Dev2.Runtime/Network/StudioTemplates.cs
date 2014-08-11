using System.Network;

// ReSharper disable CheckNamespace
namespace Dev2.DynamicServices
{
    // ReSharper disable InconsistentNaming
    public static class StudioTemplates
    {
        public static readonly PacketTemplate Server_OnAuxiliaryConnectionRequested = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Client_OnAuxiliaryConnectionReply = new PacketTemplate(0, 0, true);

        public static readonly PacketTemplate Server_OnDebugWriterAddition = new PacketTemplate(0, 1, true);
        public static readonly PacketTemplate Server_OnDebugWriterSubtraction = new PacketTemplate(0, 2, true);

        public static readonly PacketTemplate Client_OnDebugWriterWrite = new PacketTemplate(0, 1, true); 

    }
}
