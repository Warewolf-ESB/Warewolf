
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
    public sealed class IOCPConnection : Connection
    {
        #region Instance Fields
        private EventHandler<SocketAsyncEventArgs> _onDataSent;
        private EventHandler<SocketAsyncEventArgs> _onDataReceived;
        #endregion

        #region Constructor
        public IOCPConnection(NetworkHost host, Socket socket, NetworkDirection direction)
            : base(host, socket, direction)
        {
            AcquireOutbound();
            host.NotifyCreated(this);
        }
        #endregion

        #region Start Handling
        public override void Start()
        {
            if (_disposed || _alive) return;

            _onDataSent = new EventHandler<SocketAsyncEventArgs>(OnDataSent);
            _onDataReceived = new EventHandler<SocketAsyncEventArgs>(OnDataReceived);
            _alive = true;
            _wasAlive = true;

            try { BeginReceive(); }
            catch { _wasAlive = false; Dispose(true); }
        }
        #endregion

        #region Outbound Handling
        public override void SendExtended(Packet p, byte[] extension, int extensionLength)
        {
            if (p == null || _socket == null) return;
            if (extension == null || extensionLength <= 0) return;
            if (p._headerLength == Byte.MinValue) p.Compile();

            byte encoded = p._header[0];
            p._header[0] = AppendFlags(encoded, PacketHeaderFlags.Extended);
            NetworkAsyncEventArgs sendBuffer = SendEncrypted(null, p._header, 0, p._headerLength);
            p._header[0] = encoded;

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (p._dataLength != 0) sendBuffer = SendInternal(sendBuffer, p.Buffer, 0, p._dataLength);
            
            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            sendBuffer = SendInternal(sendBuffer, extension, 0, extensionLength);

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (sendBuffer != null)
            {
                if (sendBuffer.Index > 0)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        int index = toSend.Index;
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, index);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                    }
                }
                else ReleaseOutbound(ref sendBuffer);
            }
        }

        public override void Send(Packet p)
        {
            if (p == null || _socket == null) return;
            if (p._headerLength == Byte.MinValue) p.Compile();

            NetworkAsyncEventArgs sendBuffer = SendEncrypted(null, p._header, 0, p._headerLength);

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (p._dataLength != 0) sendBuffer = SendInternal(sendBuffer, p.Buffer, 0, p._dataLength);

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (sendBuffer != null)
            {
                if (sendBuffer.Index > 0)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        int index = toSend.Index;
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, index);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                    }
                }
                else ReleaseOutbound(ref sendBuffer);
            }
        }

        public override void Send(byte[] data, int index, int length)
        {
            if (data == null || length <= 0 || _socket == null) return;
            NetworkAsyncEventArgs sendBuffer = SendInternal(null, data, index, length);

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (sendBuffer != null)
            {
                if (sendBuffer.Index > 0)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        index = toSend.Index;
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, index);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                    }
                }
                else ReleaseOutbound(ref sendBuffer);
            }
        }

        public override void SendRange(params Packet[] packets)
        {
            if (packets == null || _socket == null) return;
            Packet p = null;
            NetworkAsyncEventArgs sendBuffer = null;

            for (int i = 0; i < packets.Length; i++)
            {
                if (_socket == null || _disposed)
                {
                    ReleaseOutbound(ref sendBuffer);
                    return;
                }

                if ((p = packets[i])._headerLength == Byte.MinValue) p.Compile();
                sendBuffer = SendEncrypted(sendBuffer, p._header, 0, p._headerLength);

                if (_socket == null || _disposed)
                {
                    ReleaseOutbound(ref sendBuffer);
                    return;
                }

                if (p._dataLength != 0) sendBuffer = SendInternal(sendBuffer, p.Buffer, 0, p._dataLength);
            }

            if (_socket == null || _disposed)
            {
                ReleaseOutbound(ref sendBuffer);
                return;
            }

            if (sendBuffer != null)
            {
                if (sendBuffer.Index > 0)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        int index = toSend.Index;
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, index);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                    }
                }
                else ReleaseOutbound(ref sendBuffer);
            }
        }

        private NetworkAsyncEventArgs SendEncrypted(NetworkAsyncEventArgs sendBuffer, byte[] data, int index, int length)
        {
            while (index != length)
            {
                if (sendBuffer == null) sendBuffer = AcquireOutbound();
                int totalOut = length - index;
                int available = 8192 - sendBuffer.Index;
                int overflow = totalOut - available;
                bool send = false;

                if (overflow >= 0)
                {
                    _crypt.Encrypt(data, index, sendBuffer.BufferInternal, sendBuffer.Index, available);
                    index += available;
                    send = true;
                }
                else
                {
                    _crypt.Encrypt(data, index, sendBuffer.BufferInternal, sendBuffer.Index, totalOut);
                    sendBuffer.Index += totalOut;
                    index += totalOut;
                }

                if (send)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, 8192);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                        return null;
                    }
                }
            }

            return sendBuffer;
        }

        private NetworkAsyncEventArgs SendInternal(NetworkAsyncEventArgs sendBuffer, byte[] data, int index, int length)
        {
            while (index != length)
            {
                if (sendBuffer == null) sendBuffer = AcquireOutbound();
                int totalOut = length - index;
                int available = 8192 - sendBuffer.Index;
                int overflow = totalOut - available;
                bool send = false;

                if (overflow >= 0)
                {
                    Buffer.BlockCopy(data, index, sendBuffer.BufferInternal, sendBuffer.Index, available);
                    index += available;
                    send = true;
                }
                else
                {
                    Buffer.BlockCopy(data, index, sendBuffer.BufferInternal, sendBuffer.Index, totalOut);
                    sendBuffer.Index += totalOut;
                    index += totalOut;
                }

                if (send)
                {
                    NetworkAsyncEventArgs toSend = sendBuffer;
                    sendBuffer = null;

                    try
                    {
                        toSend.Index = 0;
                        toSend.SetBuffer(toSend.BufferInternal, 0, 8192);
                        if (!_socket.SendAsync(toSend)) ReleaseOutbound(ref toSend);
                    }
                    catch
                    {
                        ReleaseOutbound(ref toSend);
                        Dispose(true);
                        return null;
                    }
                }
            }

            return sendBuffer;
        }

        private void OnDataSent(object sender, SocketAsyncEventArgs args)
        {
            NetworkAsyncEventArgs buffer = args as NetworkAsyncEventArgs;
            ReleaseOutbound(ref buffer);
        }
        #endregion

        #region Inbound Handling
        private void BeginReceive()
        {
            if (_socket == null || _disposed) return;
            NetworkAsyncEventArgs args = AcquireInbound();
            if (!_socket.ReceiveAsync(args)) OnDataReceived(this, args);
        }

        private void OnDataReceived(object sender, SocketAsyncEventArgs args)
        {
            NetworkAsyncEventArgs buffer = args as NetworkAsyncEventArgs;

            if (_socket == null)
            {
                ReleaseInbound(ref buffer);
                return;
            }

            try
            {
                int bCount = buffer.BytesTransferred;

                if (bCount > 0)
                {
                    if (_authBroker != null)
                    {
                        if (!_authBroker.NotifyDataReceived(buffer.BufferInternal, bCount)) Dispose(true);
                        else BeginReceive();
                    }
                    else if (_assembler != null)
                    {
                        if (!_assembler.Assemble(buffer.BufferInternal, 0, bCount)) Dispose(true);
                        else BeginReceive();
                    }
                }
                else Dispose(true);
            }
            catch (ObjectDisposedException) { Dispose(true); }
            catch (Exception) { Dispose(true); }
            finally { ReleaseInbound(ref buffer); }
        }
        #endregion

        #region Buffer Handling
        private NetworkAsyncEventArgs AcquireOutbound()
        {
            NetworkAsyncEventArgs sendBuffer = _host.SocketPool.TryDequeue();
            if (sendBuffer == null) sendBuffer = new NetworkAsyncEventArgs(8192);
            else sendBuffer.Index = 0;
            sendBuffer.Completed += _onDataSent;
            sendBuffer.UserToken = this;
            return sendBuffer;
        }

        private NetworkAsyncEventArgs AcquireInbound()
        {
            NetworkAsyncEventArgs args = _host.SocketPool.TryDequeue();
            if (args == null) args = new NetworkAsyncEventArgs(8192);
            args.SetBuffer(args.BufferInternal, 0, 8192);
            args.Completed += _onDataReceived;
            args.UserToken = this;
            return args;
        }

        private void ReleaseOutbound(ref NetworkAsyncEventArgs args)
        {
            if (args != null)
            {
                args.Completed -= _onDataSent;
                if (_host.SocketPool != null) _host.SocketPool.Enqueue(args);
                else args.Dispose();
                args = null;
            }
        }

        private void ReleaseInbound(ref NetworkAsyncEventArgs args)
        {
            if (args != null)
            {
                args.Completed -= _onDataReceived;
                if (_host.SocketPool != null) _host.SocketPool.Enqueue(args);
                else args.Dispose();
                args = null;
            }
        }
        #endregion

        #region Disposal Handling
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (_socket != null) NetworkHelper.ReleaseSocket(ref _socket);

            _alive = false;

            if (disposing)
            {
                _host.NotifyDisposed(this);
                GC.SuppressFinalize(this);
            }
        }
        #endregion
    }
}
