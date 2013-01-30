using Dev2.Common;
using Dev2.DataList.Contract.Network;
using Dev2.Network.Execution;
using Dev2.Studio.Core.Account;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network.DataList;
using Dev2.Studio.Core.Network.Execution;
using System;
using System.ComponentModel.Composition;
using System.Network;
using System.Security.Principal;

namespace Dev2.Studio.Core
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(IEnvironmentConnection))]
    public class EnvironmentConnection : IEnvironmentConnection
    {
        #region Class Members
        private IUserAccountProvider _userAccountProvider;
        private string _allowedRoles = "Business Design Studio Developers";

        private TCPDispatchedClient _client;
        //TODO Use the currend windows user SID, this should be changed when proper secturity and accound support is introduced.
        private string _username;
        private string _password;
        private string _alias;

        #endregion Class Members

        #region Events

        #region NetworkStateChanged

        public event EventHandler<LoginStateEventArgs> LoginStateChanged;

        protected void OnLoginStateChanged(LoginStateEventArgs args)
        {
            if (LoginStateChanged != null)
            {
                LoginStateChanged(this, args);
            }
        }

        #endregion NetworkStateChanged

        #endregion Events

        #region Properties

        public Uri Address { get; set; }
        public string DisplayName { get; set; }
        public IFrameworkDataChannel DataChannel { get; set; }
        public INetworkExecutionChannel ExecutionChannel { get; set; }
        public INetworkDataListChannel DataListChannel { get; set; }

        public bool IsConnected { get { return _client != null && _client.NetworkState == NetworkState.Online && _client.LoggedIn; } }
        public bool IsAuxiliry { get { return _client != null && _client.IsAuxiliary; } }

        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                DisplayName = value;
            }
        }

        [Import]
        public IFrameworkSecurityContext SecurityContext { get; set; }

        public string AllowedRoles
        {
            get { return _allowedRoles; }
            set { _allowedRoles = value; }
        }

        public bool HasAccess
        {
            get
            {
                if (string.IsNullOrEmpty(AllowedRoles))
                {
                    return false;
                }

                if (SecurityContext == null)
                {
                    return false;
                }

                return
                    SecurityContext.IsUserInRole(AllowedRoles.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        #endregion Properties

        #region Constructor

        public EnvironmentConnection()
        {
            _userAccountProvider = new UserAccountProvider(WindowsIdentity.GetCurrent().User.Value, "asd");
            _username = _userAccountProvider.UserName;
            _password = _userAccountProvider.Password;
        }

        public EnvironmentConnection(string userName, string password)
        {
            IUserAccountProvider userAccountProvider = new UserAccountProvider(userName, password);
            _username = userAccountProvider.UserName;
            _password = userAccountProvider.Password;
        }

        #endregion Constructor

        #region Methods

        public void Connect()
        {
            if (_client != null && _client.NetworkState == NetworkState.Online && _client.LoggedIn)
            {
                return;
            }

            IDisposable disposableExecutionChannel = ExecutionChannel as IDisposable;
            if (disposableExecutionChannel != null)
            {
                disposableExecutionChannel.Dispose();
                disposableExecutionChannel = null;
            }
            ExecutionChannel = null;

            IDisposable disposableDataListChannel = DataListChannel as IDisposable;
            if (disposableDataListChannel != null)
            {
                disposableDataListChannel.Dispose();
                disposableDataListChannel = null;
            }
            DataListChannel = null;

            if (DataChannel != null && DataChannel is FrameworkDataChannelWrapper)
            {
                FrameworkDataChannelWrapper wrapper = DataChannel as FrameworkDataChannelWrapper;
                wrapper.Dispose();
                wrapper = null;
                _client = null;
            }
            DataChannel = null;

            if (Address == null)
            {
                throw new ArgumentNullException("Address");
            }

            string dns = Address.DnsSafeHost;
            int port = Address.Port;

            _client = new TCPDispatchedClient("Studio Client");
            _client.LoginStateChanged += _client_LoginStateChanged;

            //TODO Brendon.Page, 2012-10-24, Check for null client, this happens when the studio is closed and the environment isn't connected
            NetworkStateEventArgs args = _client.Connect(dns, port);

            if (args.ToState == NetworkState.Online)
             {
                 LoginStateEventArgs lArgs = _client.Login(_username, _password);

                 if (lArgs.LoggedIn)
                 {
                     DataChannel = new FrameworkDataChannelWrapper(this, _client, dns, port);

                     DataChannel.ExecuteCommand(string.Format("<x><Service>{0}</Service></x>", Guid.NewGuid().ToString()), _client.AccountID, GlobalConstants.NullDataListID);
                     ExecutionChannel = new ExecutionClientChannel(_client);
                     DataListChannel = new DataListClientChannel(_client);
                 }
                 else
                 {
                     //System.Windows.MessageBox.Show(lArgs.Message, "Login Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                     if (!string.IsNullOrEmpty(Alias))
                     {
                         DisplayName = string.Format("{0} - (Unavailable) ", Alias);
                     }

                     _client.LoginStateChanged -= _client_LoginStateChanged;
                     _client.Dispose();
                     _client = null;
                 }
             }
             else
             {
                 DataChannel = new FrameworkDataChannelWrapper(this, _client, dns, port);
                 ExecutionChannel = new ExecutionClientChannel(_client);
                 DataListChannel = new DataListClientChannel(_client);

                 if (!string.IsNullOrEmpty(Alias))
                 {
                     DisplayName = string.Format("{0} - (Unavailable) ", Alias);
                 }
             }
        }

        public void Disconnect()
        {
            if (_client != null && _client.NetworkState == NetworkState.Online)
            {
                _client.Disconnect();
                _client.LoginStateChanged -= _client_LoginStateChanged;
                _client.Dispose();
                _client = null;

                IDisposable disposableExecutionChannel = ExecutionChannel as IDisposable;
                if (disposableExecutionChannel != null)
                {
                    disposableExecutionChannel.Dispose();
                    disposableExecutionChannel = null;
                }
                ExecutionChannel = null;

                IDisposable disposableDataListChannel = DataListChannel as IDisposable;
                if (disposableDataListChannel != null)
                {
                    disposableDataListChannel.Dispose();
                    disposableDataListChannel = null;
                }
                DataListChannel = null;
            }
        }

        #endregion Methods

        #region Event Handlers

        private void _client_LoginStateChanged(NetworkHost client, LoginStateEventArgs args)
        {
            OnLoginStateChanged(args);
        }

        #endregion Event Handlers

        #region Private Classes

        private sealed class FrameworkDataChannelWrapper : IStudioClientContext, INetworkOperator, IDisposable
        {
            private string _username = WindowsIdentity.GetCurrent().User.Value;
            private string _password = "asd";

            private EnvironmentConnection _environment;
            private TCPDispatchedClient _client;
            private string _hostNameOrAddress;
            private int _port;

            public Guid AccountID { get { return (_client == null || !_client.LoggedIn) ? Guid.Empty : _client.AccountID; } }
            public Guid ServerID { get { return (_client == null || !_client.LoggedIn) ? Guid.Empty : _client.ServerID; } }

            public FrameworkDataChannelWrapper(EnvironmentConnection environment, TCPDispatchedClient client, string hostNameOrAddress, int port)
            {
                _environment = environment;
                _client = client;
                _hostNameOrAddress = hostNameOrAddress;
                _port = port;
            }

            public TCPDispatchedClient AcquireAuxiliaryConnection()
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                return _client.CreateAuxiliaryClient();
            }

            public void AddDebugWriter(Dev2.Diagnostics.IDebugWriter writer)
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.AddDebugWriter(writer);
            }

            public void RemoveDebugWriter(Dev2.Diagnostics.IDebugWriter writer)
            {
                if (_client == null) return;
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.RemoveDebugWriter(writer);
            }

            public void RemoveDebugWriter(Guid writerID)
            {
                if (_client == null) return;
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.RemoveDebugWriter(writerID);
            }


            private bool EnsureConnected()
            {
                if (!_client.LoggedIn || _client.NetworkState != NetworkState.Online)
                {
                    if (_client.NetworkState == NetworkState.Offline)
                    {
                        NetworkStateEventArgs args = _client.Connect(_hostNameOrAddress, _port);

                        if (args.ToState == NetworkState.Online)
                        {
                            LoginStateEventArgs lArgs = _client.Login(_username, _password);

                            if (!lArgs.LoggedIn)
                            {
                                //System.Windows.MessageBox.Show(lArgs.Message, "Login Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                                if (!string.IsNullOrEmpty(_environment.Alias))
                                {
                                    _environment.DisplayName = string.Format("{0} - (Unavailable) ", _environment.Alias);
                                }

                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_environment.Alias))
                            {
                                _environment.DisplayName = string.Format("{0} - (Unavailable) ", _environment.Alias);
                            }

                            return false;
                        }
                    }
                    else return true;
                }
                else return true;
            }

            public string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID)
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");

                return _client.ExecuteCommand(xmlRequest);
            }

            public void Dispose()
            {
                if (_client != null)
                {
                    try
                    {
                        _client.Disconnect();
                    }
                    catch { }

                    try
                    {
                        _client.Dispose();
                    }
                    catch { }

                    _client = null;
                }
            }

            public void Send(Packet p)
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.Send(p);
            }
        }

        #endregion Private Classes
    }
}
