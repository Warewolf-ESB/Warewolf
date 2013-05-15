using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Network;
using System.Cryptography;
using System.IO;

namespace Dev2.DynamicServices.Network.Auxiliary
{
    public sealed class StudioAuxiliaryAccountProvider
    {
        #region Instance Fields
        private string _debugName;
        private object _syncLock;
        private StudioAuxiliaryServer _server;
        private Dictionary<string, StudioAuxiliaryAccount> _accounts;
        #endregion

        #region Initialization Handling
        public StudioAuxiliaryAccountProvider(string debugName, StudioAuxiliaryServer server)
        {
            _syncLock = new object();
            if ((_server = server) == null) throw new ArgumentNullException("server");
            if (String.IsNullOrEmpty(_debugName = debugName)) _debugName = _server.Name + " Account Provider";
            _accounts = new Dictionary<string, StudioAuxiliaryAccount>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region [Get/Set] Handling
        public StudioAuxiliaryAccount GetAccount(string username)
        {
            StudioAuxiliaryAccount account = null;

            lock (_syncLock)
            {
                if (_accounts.TryGetValue(username, out account)) return account;
                account = null;
            }

            return account;
        }
        #endregion

        /// <summary>
        /// Creates a new account on this StudioAuxiliaryAccountProvider with the provided authentication GUID.
        /// </summary>
        public StudioAuxiliaryAccount CreateAccount(Guid auth)
        {
            byte[] raw = auth.ToByteArray();
            string username = "Auth";
            string password = "P";

            for (int i = 0; i < 8; i++)
            {
                 username += raw[i].ToString();
                 password += raw[i + 8].ToString();
            }

            StudioAuxiliaryAccount account = null;

            lock (_syncLock)
            {
                if (_accounts.TryGetValue(username, out account))
                {
                    throw new ArgumentException("An account with that username already exists.");
                }

                account = new StudioAuxiliaryAccount(username, password);
                _accounts.Add(account.Username, account);
            }

            return account;
        }

        public void ClearAccounts()
        {
            lock (_syncLock)
                _accounts.Clear();
        }

    }
}
