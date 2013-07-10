using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Network
{
    public sealed class IOCPPacketHandler<T> where T : NetworkContext
    {
        #region Instance Fields
        private ushort _packetID;
        private ulong _flags;
        private PacketTemplate _template;
        private PacketEventHandler<T> _handler;
        #endregion

        #region Internal Properties
        internal PacketEventHandler<T> Handler { get { return _handler; } }
        #endregion

        #region Public Properties
        public ushort PacketID { get { return _packetID; } }
        public ulong Flags { get { return _flags; } }
        public PacketTemplate Template { get { return _template; } }
        #endregion

        #region Constructor
        public IOCPPacketHandler(ushort packetID, ulong flags, PacketTemplate template, PacketEventHandler<T> handler)
        {
            _packetID = packetID;
            _flags = flags;
            _template = template;
            _handler = handler;
        }
        #endregion
    }
}
