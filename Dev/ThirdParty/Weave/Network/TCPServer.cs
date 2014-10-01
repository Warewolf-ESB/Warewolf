
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace System.Network
{
    public abstract class TCPServer<T> : NetworkServer where T : NetworkContext, new()
    {
        #region Instance Fields
        private bool _running;
        private Firewall _firewall;
        private Listener[] _listeners;
        protected InboundAuthenticationBroker _authenticationBroker;
        private SocketEventHandler _onSocketConnected;
        private ConnectionEventHandler _onConnectionCreated;
        private ConnectionEventHandler _onConnectionDisposed;
        private NetworkContextEventHandler<T> _onContextAttached;
        private NetworkContextEventHandler<T> _onContextDetached;

        protected IOCPPacketHandlerCollection<T>[] _channels;
        protected IOCPExtensionHandler<T>[] _extensions;
        protected ReaderWriterLockSlim _contextLock;
        protected IndexedList<T> _attachedContexts;
        #endregion

        #region Public Properties
        public bool Running { get { return _running; } }
        public IOCPPacketHandlerCollection<T>[] Channels { get { return _channels; } }
        public IOCPExtensionHandler<T>[] Extensions { get { return _extensions; } }
        public IndexedList<T> AttachedContexts { get { return _attachedContexts; } }
        #endregion

        #region Events
        public event SocketEventHandler SocketConnected { add { if(!_disposing) _onSocketConnected += value; } remove { if(!_disposing) _onSocketConnected -= value; } }
        public event ConnectionEventHandler ConnectionCreated { add { if(!_disposing) _onConnectionCreated += value; } remove { if(!_disposing) _onConnectionCreated -= value; } }
        public event ConnectionEventHandler ConnectionDisposed { add { if(!_disposing) _onConnectionDisposed += value; } remove { if(!_disposing) _onConnectionDisposed -= value; } }
        public event NetworkContextEventHandler<T> ContextAttached { add { if(!_disposing) _onContextAttached += value; } remove { if(!_disposing) _onContextAttached -= value; } }
        public event NetworkContextEventHandler<T> ContextDetached { add { if(!_disposing) _onContextDetached += value; } remove { if(!_disposing) _onContextDetached -= value; } }
        #endregion

        #region Constructor
        public TCPServer(string name, InboundAuthenticationBroker authenticationBroker)
            : base(name)
        {
            _firewall = new Firewall();
            _authenticationBroker = authenticationBroker;
            _channels = new IOCPPacketHandlerCollection<T>[16];
            _extensions = new IOCPExtensionHandler<T>[16];
            _contextLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _attachedContexts = new IndexedList<T>();

            _channels[15] = new IOCPPacketHandlerCollection<T>(15, null);
            _channels[15].Register(1, InternalTemplates.Server_OnExecuteStringCommandReceived, OnExecuteStringCommandReceived);
            _channels[15].Register(2, InternalTemplates.Server_OnExecuteBinaryCommandReceived, OnExecuteBinaryCommandReceived);
        }

        #endregion

        #region [Start/Stop] Handling
        public bool Start(params ListenerConfig[] listeners)
        {
            List<Listener> nListeners = new List<Listener>();

            if(_listeners != null && _listeners.Length != 0)
            {
                if(!_running)
                {
                    for(int i = 0; i < _listeners.Length; i++)
                        if(_listeners[i].Start())
                            nListeners.Add(_listeners[i]);
                }
                else nListeners.AddRange(_listeners);
            }

            if(listeners != null && listeners.Length != 0)
            {
                for(int i = 0; i < listeners.Length; i++)
                {
                    Listener listener = new Listener(this, listeners[i]);
                    if(listener.Start()) nListeners.Add(listener);
                }
            }

            _listeners = nListeners.ToArray();
            OnStarted(listeners);
            return (_running = _listeners.Length > 0);
        }

        public void Stop()
        {
            if(!_running) return;
            _running = false;
            OnStopped();
            for(int i = 0; i < _listeners.Length; i++) _listeners[i].Stop();
        }

        protected virtual void OnStarted(ListenerConfig[] configs)
        {
        }

        protected virtual void OnStopped()
        {
        }
        #endregion

        #region ExecuteCommand Handling
        private void OnExecuteStringCommandReceived(INetworkOperator op, T context, ByteBuffer reader)
        {
            var serial = reader.ReadInt64();
            var datalistID = reader.ReadGuid(); // PBI : 5376 Add -- TODO : Hook up the client side....
            var payload = reader.ReadString();
            var result = OnExecuteCommand(context, payload, datalistID);

            var p = new Packet(InternalTemplates.Client_OnExecuteStringCommandReceived);
            p.Write(serial);
            p.Write(datalistID);
            p.Write(result);
            op.Send(p);
        }

        private void OnExecuteBinaryCommandReceived(INetworkOperator op, T context, ByteBuffer reader)
        {
            var serial = reader.ReadInt64();
            var result = new Packet(InternalTemplates.Client_OnExecuteBinaryCommandReceived);
            result.Write(serial);
            OnExecuteCommand(context, reader, result);
            op.Send(result);
        }

        protected abstract string OnExecuteCommand(T context, string payload, Guid dataListID);
        protected abstract void OnExecuteCommand(T context, ByteBuffer payload, Packet writer);
        #endregion

        #region Incoming Connection Handling
        protected override bool ValidateSocketConnection(Listener source, Socket socket, IPAddress address, string addressString, int port)
        {
            if(_firewall == null) return true;
            return !_firewall.IsBlocked(address);
        }

        protected override void ApplySocketOptions(Listener source, Socket socket)
        {
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
        }

        protected override void OnSocketConnectionAccepted(Listener source, Socket socket, IPAddress address, string addressString, int port)
        {
            try
            {
                IOCPConnection connection = new IOCPConnection(this, socket, NetworkDirection.Inbound);
                connection.AuthBroker = _authenticationBroker.Instantiate(_firewall, connection);
                connection.Start();
                if(!connection.Alive) connection.Dispose();
            }
            catch { NetworkHelper.ReleaseSocket(ref socket); }
        }

        protected override void OnConnectionCreated(Connection connection)
        {
            if(_onConnectionCreated != null) _onConnectionCreated(this, connection);
        }
        #endregion

        #region Context Handling

        protected override void OnConnectionDisposed(Connection connection)
        {
            if(connection.WasAlive)
            {
                if(connection.Context != null)
                {
                    T context = connection.Context as T;

                    if(context != null)
                    {
                        try
                        {
                            _contextLock.EnterWriteLock();
                            _attachedContexts.Remove(context);
                        }
                        finally { _contextLock.ExitWriteLock(); }

                        try
                        {
                            if(context.Attached && _onContextDetached != null) _onContextDetached(this, context);
                        }
                        finally
                        {
                            context.NotifyConnectionDisposed();
                        }

                        if(_onConnectionDisposed != null) _onConnectionDisposed(this, connection);
                    }
                    else if(_onConnectionDisposed != null) _onConnectionDisposed(this, connection);
                }
                else if(_onConnectionDisposed != null) _onConnectionDisposed(this, connection);
            }
        }
        #endregion

        #region [Login/Logout] Handling
        protected override void OnLoginSuccess(Connection connection, NetworkAccount account, AuthenticationBroker broker)
        {
            if(_disposing) return;
            T context = new T();

            try
            {
                _contextLock.EnterWriteLock();
                _attachedContexts.Add(context);
                context.NotifyLogin(broker as InboundAuthenticationBroker, connection, account);
            }
            catch { }
            finally { _contextLock.ExitWriteLock(); }

            if(_onContextAttached != null) _onContextAttached(this, context);
        }

        protected override void OnLoginFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
        }

        protected override void OnLogoutRequired(Connection connection, InboundAuthenticationBroker broker, AuthenticationResponse reason)
        {
            Packet p = new Packet(InternalTemplates.Client_LogoutReceived);
            p.Write((byte)reason);
            connection.Send(p);

            if(connection.Context != null)
            {
                T context = connection.Context as T;

                _contextLock.EnterWriteLock();

                try
                {
                    _attachedContexts.Remove(context);
                }
                finally { _contextLock.ExitWriteLock(); }

                try
                {
                    if(context.Attached && _onContextDetached != null) _onContextDetached(this, context);
                }
                finally
                {
                    context.NotifyLogout(reason);
                }

                connection.AuthBroker = _authenticationBroker.Instantiate(_firewall, connection);
                connection.Crypt = NullCryptProvider.Singleton;
                connection.Assembler = null;
            }
        }

        private void OnLogoutReceived(INetworkOperator op, T context, ByteBuffer reader)
        {
            string name = context.ToString();
            Console.WriteLine("Logout Received From " + name);
            AuthenticationResponse response = AuthenticationResponse.Logout;

            Packet p = new Packet(InternalTemplates.Client_LogoutReceived);
            p.Write((byte)response);
            op.Send(p);

            Connection connection = context.Connection;

            _contextLock.EnterWriteLock();

            try
            {
                _attachedContexts.Remove(context);
            }
            finally { _contextLock.ExitWriteLock(); }

            try
            {
                if(context.Attached && _onContextDetached != null) _onContextDetached(this, context);
            }
            finally
            {
                context.NotifyLogout(response);
            }

            connection.AuthBroker = _authenticationBroker.Instantiate(_firewall, connection);
            connection.Crypt = NullCryptProvider.Singleton;
            connection.Assembler = null;
        }
        #endregion

        #region Incoming Data Handling
        protected internal override bool ValidateChannel(int channel)
        {
            return _channels[channel] != null;
        }

        protected internal override PacketTemplate AcquirePacketTemplate(int channel, ushort packetID)
        {
            IOCPPacketHandler<T> handler = _channels[channel].Handlers[packetID];
            return handler == null ? null : handler.Template;
        }

        protected internal override void Dispatch(Connection connection, int channel, ushort packetID, int dataLength, byte[] data, bool compressed)
        {
            _channels[channel].Dispatch(packetID, connection.Context, connection.Context as T, new ByteBuffer(data, dataLength));
        }
        #endregion

        #region Extension Handling
        protected internal override PacketTemplate AcquireExtensionTemplate(int extension)
        {
            IOCPExtensionHandler<T> handler = _extensions[extension];
            return handler == null ? null : handler.Template;
        }

        protected internal override void Dispatch(Connection connection, PacketData extension, PacketData packet, bool isResponse)
        {
            if(isResponse) _extensions[extension.Channel].Response(_channels[packet.Channel], connection.Context as T, extension, packet);
            else _extensions[extension.Channel].Dispatch(_channels[packet.Channel], connection.Context as T, extension, packet);
        }
        #endregion

        #region Disposal Handling
        protected override void OnDisposing(bool disposing)
        {
            _running = false;
            _onSocketConnected = null;
            _onConnectionCreated = null;
            _onConnectionDisposed = null;
            _onContextAttached = null;
            _onContextDetached = null;
        }
        #endregion
    }
}
