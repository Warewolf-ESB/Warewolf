using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Network
{
    public sealed class AsyncPacketHandler
    {
        #region Instance Fields
        private ushort _packetID;
        private ulong _flags;
        private PacketTemplate _template;
        private PacketEventHandler _handler;
        #endregion

        #region Public Properties
        public ushort PacketID { get { return _packetID; } }
        public ulong Flags { get { return _flags; } }
        public PacketTemplate Template { get { return _template; } }
        public PacketEventHandler Handler { get { return _handler; } }
        #endregion

        #region Constructor
        public AsyncPacketHandler(ushort packetID, ulong flags, PacketTemplate template, PacketEventHandler handler)
        {
            _packetID = packetID;
            _flags = flags;
            _template = template;
            _handler = handler;
        }
        #endregion
    }
}
