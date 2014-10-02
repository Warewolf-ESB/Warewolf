
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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net.Configuration;
using System.Threading;

namespace System.Network
{
    public sealed class AsyncConnection : Connection
    {
        #region Constants
        private const int PendingState = 2;
        #endregion

        #region Instance Fields
        private byte[] _recBuffer;
        private object _syncLock;
        private int _state;

        private AsyncCallback _onDataSent;
        private AsyncCallback _onDataReceived;
        #endregion

        #region Constructor
        public AsyncConnection(NetworkHost host, Socket socket, NetworkDirection direction)
            : base(host, socket, direction)
        {
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            _recBuffer = new byte[8192];
            _syncLock = new object();
            host.NotifyCreated(this);
        }
        #endregion

        #region [Start/Pause/Resume] Handling
        public override void Start()
        {
            if (_disposed || _alive) return;

            _onDataSent = new AsyncCallback(OnDataSent);
            _onDataReceived = new AsyncCallback(OnDataReceived);
            _alive = true;
            bool wasAlive = true;

            try
            {
                lock (_syncLock)
                {
                    if ((_state & PendingState) == 0)
                        BeginReceive();
                }
            }
            catch { wasAlive = false; Dispose(true); }

            _wasAlive = wasAlive;
        }
        #endregion

        #region Outbound Handling
        public override void Send(Packet p)
        {
            if (p == null || _socket == null) return;
            if (p._headerLength == Byte.MinValue) p.Compile();
            if (!SendEncrypted(p._header, 0, p._headerLength)) return;
            if (p._dataLength != 0 && !SendInternal(p.Buffer, 0, p._dataLength)) return;
        }

        public override void SendExtended(Packet p, byte[] extension, int extensionLength)
        {
            if (p == null || _socket == null) return;
            if (p._headerLength == Byte.MinValue) p.Compile();
            if (!SendEncrypted(AppendFlags(p._header[0], PacketHeaderFlags.Extended), p._header, 0, p._headerLength)) return;
            if (p._dataLength != 0 && !SendInternal(p.Buffer, 0, p._dataLength)) return;
            SendInternal(extension, 0, extensionLength);
        }

        public override void Send(byte[] data, int index, int length)
        {
            if (data == null || length <= 0) return;
            if (_socket == null) return;
            SendInternal(data, index, length);
        }

        public override void SendRange(params Packet[] packets)
        {
            if (packets == null || _socket == null) return;
            byte[] header = new byte[8];
            Packet p = null;
            
            for (int i = 0; i < packets.Length; i++)
            {
                if (_disposed || _socket == null) return;
                if ((p = packets[i])._headerLength == Byte.MinValue) p.Compile();
                if (!SendEncrypted(p._header, 0, p._headerLength)) return;
                if (p._dataLength != 0 && !SendInternal(p.Buffer, 0, p._dataLength)) return;
            }
        }

        private bool SendEncrypted(byte[] data, int index, int length)
        {
            byte[] header = new byte[length];
            _crypt.Encrypt(data, index, header, 0, length);

            try { _socket.BeginSend(header, 0, length, SocketFlags.None, _onDataSent, null); }
            catch
            {
                Dispose(true);
                return false;
            }

            return true;
        }

        private bool SendEncrypted(byte extended, byte[] data, int index, int length)
        {
            byte[] header = new byte[length];
            header[0] = _crypt.Encrypt(extended);
            _crypt.Encrypt(data, index + 1, header, 1, length - 1);

            try { _socket.BeginSend(header, 0, length, SocketFlags.None, _onDataSent, null); }
            catch
            {
                Dispose(true);
                return false;
            }

            return true;
        }


        private bool SendInternal(byte[] data, int index, int length)
        {
            try { _socket.BeginSend(data, index, length, SocketFlags.None, _onDataSent, null); }
            catch
            {
                Dispose(true);
                return false;
            }

            return true;
        }

        private void OnDataSent(IAsyncResult asyncResult)
        {
            if (_socket == null) return;

            try
            {
                int bCount = _socket.EndSend(asyncResult);

                if (bCount <= 0)
                {
                    Dispose(true);
                    return;
                }
            }
            catch { Dispose(true); }
        }
        #endregion

        #region Inbound Handling
        private void BeginReceive()
        {
            _state |= PendingState;
            _socket.BeginReceive(_recBuffer, 0, 8192, SocketFlags.None, _onDataReceived, null);
        }

        private void OnDataReceived(IAsyncResult asyncResult)
        {
            if (_socket == null) return;

            try
            {
                int bCount = _socket.EndReceive(asyncResult);

                if (bCount > 0)
                {
                    byte[] buffer = _recBuffer;

                    if (_authBroker != null)
                    {
                        if (!_authBroker.NotifyDataReceived(buffer, bCount))
                        {
                            Dispose(true);
                            return;
                        }
                    }
                    else if (_assembler != null && !_assembler.Assemble(buffer, 0, bCount))
                    {
                        Dispose(true);
                        return;
                    }

                    lock (_syncLock)
                    {
                        _state &= ~PendingState;
                        BeginReceive();
                    }
                }
                else Dispose(true);
            }
            catch { Dispose(true); }
        }
        #endregion

        #region Disposal Handling
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            if (_socket != null) NetworkHelper.ReleaseSocket(ref _socket);

            _alive = false;
            _recBuffer = null;
            _onDataSent = null;
            _onDataReceived = null;

            if (disposing)
            {
                _host.NotifyDisposed(this);
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
