using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Network;
using System.Cryptography;
using System.IO;
using System.Collections.Concurrent;

namespace Dev2.DynamicServices
{
    public sealed class StudioAccountProvider
    {
        #region Instance Fields
        private string _debugName;
        private object _syncLock = new object();
        private bool _autoAccountCreation;
        private StudioNetworkServer _server;
        private IDictionary<string, StudioAccount> _accounts;
        #endregion

        #region Initialization Handling
        public StudioAccountProvider(string debugName, bool autoAccountCreation, StudioNetworkServer server)
        {
            _syncLock = new object();
            _autoAccountCreation = autoAccountCreation;
            if ((_server = server) == null) throw new ArgumentNullException("server");
            if (String.IsNullOrEmpty(_debugName = debugName)) _debugName = _server.Name + " Account Provider";
            _accounts = new Dictionary<string, StudioAccount>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region [Load/Save] Handling
        public void Load()
        {
            lock (_syncLock)
            {
                _accounts = new ConcurrentDictionary<string, StudioAccount>(StringComparer.OrdinalIgnoreCase);
                string path = _server.FileSystem.GetEnsuredPath("Accounts.dat", StudioFileSystem.SpecialFolder.Data);

                if (!String.IsNullOrEmpty(path))
                {
                    using (BinaryFileReader reader = new BinaryFileReader(path))
                    {
                        int count = reader.ReadInt32();

                        for (int i = 0; i < count; i++)
                        {
                            StudioAccount account = new StudioAccount(reader);
                            _accounts.Add(account.Username, account);

                            //
                            // Brendon.Page, 2012.11.20, Set all account passwords to a constant in order to provide 
                            //                            anonymouse connections. If/When usernames & passwords are 
                            //                            implemented this will have ot be changed.
                            //
                            account.SetPassword("abc123xyz");
                            
                        }
                        reader.Dispose();
                    }
                    
                }
            }

            if (_accounts.Count == 0)
            {
                // Brendon.Page, 2012.11.20, Called create account instead as it does the same and saves the new account.
                //StudioAccount admin = new StudioAccount("Ascent", "abc123xyz");
                //_accounts.Add(admin.Username, admin);
                CreateAccount("Ascent", "abc123xyz");
            }
            // Brendon.Page, 2012.11.20, Password setting logic moved so that it happens for all accounts
            //else
            //{
            //    StudioAccount admin = GetAccount("Ascent");
            //    if (admin != null) admin.SetPassword("abc123xyz");
            //}
            
        }

        public void Save()
        {
            lock (_syncLock)
            {
                string path = _server.FileSystem.GetRelativePath("Accounts.dat", StudioFileSystem.SpecialFolder.Data);

                using (BinaryFileWriter writer = new BinaryFileWriter(path, false))
                {
                    writer.Write(_accounts.Count);
                    foreach (StudioAccount account in _accounts.Values)
                    {
                        account.Serialize(writer, false);
                    }
                    writer.Dispose();
                }
            }
        }
        #endregion

        #region [Get/Set] Handling
        public StudioAccount GetAccount(Guid accountID)
        {
            StudioAccount account = null;

            lock (_syncLock)
            {
                foreach (KeyValuePair<string, StudioAccount> kvp in _accounts)
                    if (kvp.Value.AccountID == accountID)
                    {
                        account = kvp.Value;
                        break;
                    }
            }

            return account;
        }

        public StudioAccount GetAccount(string username)
        {
            if (!NetworkHelper.IsValidUsername(username)) return null;
            StudioAccount account = null;

            if (_autoAccountCreation)
            {
                lock (_syncLock)
                {
                    if (_accounts.TryGetValue(username, out account)) return account;
                    // Brendon.Page, 2012.11.20, Called create account instead as it does the same and saves the new account.
                    //account = new StudioAccount(username, "abc123xyz");
                    //_accounts.Add(account.Username, account);
                    return CreateAccount(username, "abc123xyz");
                }
            }
            else
            {
                lock (_syncLock)
                {
                    if (_accounts.TryGetValue(username, out account)) return account;
                    account = null;
                }
            }

            return account;
        }
        #endregion

        /// <summary>
        /// Creates a new account on this StudioAccountProvider with the provided username and password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public StudioAccount CreateAccount(string username, string password)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("username cannot be a null or empty string.", "username");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("password cannot be a null or empty string.", "password");
            StudioAccount account = null;

            lock (_syncLock)
            {
                if (_accounts.TryGetValue(username, out account))
                {
                    throw new ArgumentException("An account with that username already exists.");
                }

                account = new StudioAccount(username, password);
                _accounts.Add(account.Username, account);
            }

            Save();

            return account;
        }

        public void ClearAccounts()
        {
            lock (_syncLock)
                _accounts.Clear();
        }

    }
}
