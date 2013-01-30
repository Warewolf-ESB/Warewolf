using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Network;
using System.Cryptography;

namespace System
{
    public abstract class NetworkContext : IIndexed, INetworkOperator
    {
        #region Instance Fields
        private string _username;
        private Guid _accountID;
        private Guid _sessionID;
        private NetworkAccount _account;
        protected Version _version;
        protected PlatformID _platform;
        protected string _servicePack;
        protected uint _fingerprint;
        protected bool _attached;
        protected Connection _connection;
        #endregion

        #region Internal Properties
        internal Connection Connection { get { return _connection; } }
        #endregion

        #region Public Properties
        public Guid AccountID { get { return _accountID; } }
        public Guid SessionID { get { return _sessionID; } }

        public Version Version { get { return _version; } }
        public PlatformID Platform { get { return _platform; } }
        public string ServicePack { get { return _servicePack; } }
        public uint Fingerprint { get { return _fingerprint; } }
        public bool Attached { get { return _attached; } }
        
        int IIndexed.Index { get; set; }
        #endregion

        #region Constructors
        public NetworkContext()
        {
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return _username;
        }
        #endregion

        #region [Get/Set] Handling

        #endregion

        #region Connection Handling
        public void Send(Packet p)
        {
            if (p == null || !_attached || _connection == null) return;
            _connection.Send(p);
        }

        public void Kill()
        {
            if (_connection != null)
                _connection.Dispose();
        }
        #endregion

        #region Notification Handling
        internal void NotifyLogin(InboundAuthenticationBroker broker, Connection connection, NetworkAccount account)
        {
            (_connection = connection).Context = this;
            _version = broker.Version;
            _platform = broker.Platform;
            _servicePack = broker.ServicePack;
            _fingerprint = broker.Fingerprint;
            _username = account.Username;
            _accountID = account.AccountID;
            _sessionID = Guid.NewGuid();
            (_account = account).BindTo(this, connection.Address);

            _attached = true;
            OnAttached(broker, account);
        }

        internal void NotifyLogout(AuthenticationResponse response)
        {
            if (_attached)
            {
                _attached = false;
                _account.Unbind();
                OnDetached();
            }

            _connection.Context = null;
            _connection = null;
        }

        internal void NotifyConnectionDisposed()
        {
            if (_attached)
            {
                _attached = false;
                _account.Unbind();
                OnDetached();
            }

            _connection.Context = null;
            _connection = null;
        }

        protected virtual void OnAttached(InboundAuthenticationBroker broker, NetworkAccount account) { }
        protected virtual void OnDetached() { }
        #endregion
    }
}
