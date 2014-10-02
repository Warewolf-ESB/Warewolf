
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
using System.ComponentModel;

namespace System.Network
{
    public sealed class AsyncPacketHandlerCollection : IDisposable
    {
        #region Instance Fields
        private bool _disposed;
        private AsyncPacketHandler[] _handlers;
        private IMessageContext _context;
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public IMessageContext Context { get { return _context; } set { _context = value; } }
        public AsyncPacketHandler[] Handlers { get { return _handlers; } }
        #endregion

        #region Constructor
        public AsyncPacketHandlerCollection()
        {
            _handlers = new AsyncPacketHandler[UInt16.MaxValue];
        }
        #endregion

        #region Registration Handling
        public void Register(ushort pID, PacketTemplate template, PacketEventHandler handler)
        {
            _handlers[pID] = new AsyncPacketHandler(pID, 0, template, handler);
        }

        public void Deregister(ushort pID)
        {
            if (pID < 0) return;
            _handlers[pID] = null;
        }
        #endregion

        #region Dispatch Handling
        public void Dispatch(ushort packetID, INetworkOperator op, ByteBuffer reader)
        {
            if (_context == null) _handlers[packetID].Handler(op, reader);
            else _context.Post(new PacketHandlerMessage(_handlers[packetID].Handler, op, reader));
        }
        #endregion

        #region Disposal Handling
        ~AsyncPacketHandlerCollection()
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
            private PacketEventHandler _handler;
            private INetworkOperator _op;
            private ByteBuffer _reader;

            public PacketHandlerMessage(PacketEventHandler handler, INetworkOperator op, ByteBuffer reader)
            {
                _handler = handler;
                _op = op;
                _reader = reader;
            }

            public void Execute()
            {
                _handler(_op, _reader);
                _reader = null;
                _op = null;
                _handler = null;
            }
        }
        #endregion
    }
}
