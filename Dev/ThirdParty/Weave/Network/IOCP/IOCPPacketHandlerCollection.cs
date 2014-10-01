
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace System.Network
{
    public sealed class IOCPPacketHandlerCollection<T> : IDisposable where T : NetworkContext
    {
        #region Instance Fields
        private bool _disposed;
        private int _channel;
        private IOCPPacketHandler<T>[] _handlers;
        private IMessageContext _context;
        #endregion

        #region Internal Properties
        internal IOCPPacketHandler<T>[] Handlers { get { return _handlers; } }
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public int Channel { get { return _channel; } }
        #endregion

        #region Constructor
        public IOCPPacketHandlerCollection(int channel, IMessageContext context)
        {
            _channel = channel;
            _context = context;
            _handlers = new IOCPPacketHandler<T>[UInt16.MaxValue];
        }
        #endregion

        #region Registration Handling
        public void Register(ushort pID, PacketTemplate template, PacketEventHandler<T> handler)
        {
            _handlers[pID] = new IOCPPacketHandler<T>(pID, 0, template, handler);
        }

        public void Deregister(ushort pID)
        {
            if (pID < 0) return;
            _handlers[pID] = null;
        }
        #endregion

        #region Dispatch Handling
        internal void Dispatch(ushort packetID, INetworkOperator op, T context, ByteBuffer reader)
        {
            if (_context == null) _handlers[packetID].Handler(op, context, reader);
            else _context.Post(new PacketHandlerMessage(_handlers[packetID].Handler, op, context, reader));
        }
        #endregion

        #region Disposal Handling
        ~IOCPPacketHandlerCollection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            _handlers = null;
            if (disposing) GC.SuppressFinalize(this);
        }
        #endregion

        #region PacketHandlerMessage
        private sealed class PacketHandlerMessage : IMessage
        {
            private PacketEventHandler<T> _handler;
            private INetworkOperator _op;
            private T _context;
            private ByteBuffer _reader;

            public PacketHandlerMessage(PacketEventHandler<T> handler, INetworkOperator op, T context, ByteBuffer reader)
            {
                _handler = handler;
                _op = op;
                _context = context;
                _reader = reader;
            }

            public void Execute()
            {
                _handler(_op, _context, _reader);
                _reader = null;
                _context = null;
                _op = null;
                _handler = null;
            }
        }
        #endregion
    }
}
