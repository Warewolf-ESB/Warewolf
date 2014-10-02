
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace System.Network
{
    public abstract class NetworkHost
    {
        #region Instance Fields
        private bool _disposed;
        protected bool _disposing;
        private string _name;
        private Action _onDisposing;

        protected ReaderWriterLockSlim _connectionLock;
        protected List<Connection> _connections;

        internal SynchronizedQueue<NetworkAsyncEventArgs> SocketPool;
        #endregion

        #region Public Properties
        public bool Disposed { get { return _disposed; } }
        public string Name { get { return _name; } }
        public List<Connection> Connections { get { return _connections; } }
        #endregion

        #region Events
        public event Action Disposing { add { if (!_disposing) _onDisposing += value; } remove { if (!_disposing) _onDisposing -= value; } }
        #endregion

        #region Constructor
        public NetworkHost(string name)
        {
            _name = name;
            
            _connectionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _connections = new List<Connection>();

            SocketPool = new SynchronizedQueue<NetworkAsyncEventArgs>();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return _name;
        }
        #endregion

        #region Incoming Data Handling
        protected internal abstract bool ValidateChannel(int channel);
        protected internal abstract PacketTemplate AcquirePacketTemplate(int channel, ushort packetID);
        protected internal abstract void Dispatch(Connection connection, int channel, ushort packetID, int dataLength, byte[] data, bool compressed);
        #endregion

        #region Extension Handling
        protected internal virtual PacketTemplate AcquireExtensionTemplate(int extension) { return null; }
        protected internal virtual void Dispatch(Connection connection, PacketData extension, PacketData packet, bool isResponse) { }
        #endregion

        #region Connection Handling
        public void Disconnect()
        {
            Disconnect(NetworkDirection.Bidirectional);
        }

        public void Disconnect(NetworkDirection direction)
        {
            if (_disposed) return;
            Connection[] connections = null;

            try
            {
                _connectionLock.EnterReadLock();
                connections = _connections.ToArray();
            }
            catch { }
            finally { _connectionLock.ExitReadLock(); }

            for (int i = 0; i < connections.Length; i++)
                if ((connections[i].Direction & direction) != NetworkDirection.None)
                    connections[i].Dispose();
        }

        internal void NotifyCreated(Connection connection)
        {
            if (_disposed) return;
            bool created = false;

            try
            {
                _connectionLock.EnterWriteLock();
                _connections.Add(connection);
                created = true;
            }
            catch { connection.Dispose(); }
            finally { _connectionLock.ExitWriteLock(); }

            if (created) OnConnectionCreated(connection);
        }

        internal void NotifyAuthenticationFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect)
        {
            OnLoginFailed(connection, broker, reason, expectDisconnect);
        }

        internal void NotifyAuthenticated(Connection connection, NetworkAccount account, AuthenticationBroker broker)
        {
            connection.Crypt = broker.CryptProvider;
            connection.Assembler = new PacketAssembler(connection, broker.CryptProvider);
            connection.AuthBroker = null;
            OnLoginSuccess(connection, account, broker);
        }

        internal virtual void NotifyDeauthenticated(Connection connection, InboundAuthenticationBroker broker, AuthenticationResponse reason)
        {
        }

        internal void NotifyDisposed(Connection connection)
        {
            if (_disposed) return;

            try
            {
                _connectionLock.EnterWriteLock();
                _connections.Remove(connection);
            }
            catch { connection.Dispose(); }
            finally { _connectionLock.ExitWriteLock(); }

            OnConnectionDisposed(connection);
        }

        protected abstract void OnLoginSuccess(Connection connection, NetworkAccount account, AuthenticationBroker broker);
        protected abstract void OnLoginFailed(Connection connection, OutboundAuthenticationBroker broker, AuthenticationResponse reason, bool expectDisconnect);
        protected virtual void OnConnectionCreated(Connection connection) { }
        protected virtual void OnConnectionDisposed(Connection connection) { }
        #endregion

        #region Disposal Handling
        ~NetworkHost()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposing) return;
            _disposing = true;

            if (_onDisposing != null)
            {
                _onDisposing();
                _onDisposing = null;
            }

            Disconnect(NetworkDirection.Bidirectional);
            _disposed = true;
            OnDisposing(disposing);

            _connectionLock.Dispose();
            _connectionLock = null;
            _connections = null;

            OnDisposed(disposing);

            SocketAsyncEventArgs args = null;
            while ((args = SocketPool.TryDequeue()) != null) args.Dispose();
            SocketPool = null;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void OnDisposing(bool disposing) { }
        protected virtual void OnDisposed(bool disposing) { }
        #endregion
    }
}
