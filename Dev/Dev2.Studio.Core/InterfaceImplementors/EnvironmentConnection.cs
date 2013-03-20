using Caliburn.Micro;
using Dev2.Common;
using Dev2.Composition;
using Dev2.DataList.Contract.Network;
using Dev2.Diagnostics;
using Dev2.Network.Execution;
using Dev2.Network.Messages;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Account;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Network.DataList;
using Dev2.Studio.Core.Network.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Network;
using System.Security.Principal;
using System.Threading;

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
        public IStudioEsbChannel DataChannel { get; set; }
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
                if (!_client.WaitForClientDetails()) //Bug 8796, After logging in wait for client details to come through before proceeding
                {
                    throw new Exception("Retrieving client details from the server timed out.");
                }

                if (lArgs.LoggedIn)
                {
                    DataChannel = new FrameworkDataChannelWrapper(this, _client, dns, port);
                    ExecutionChannel = new ExecutionClientChannel(_client);
                    DataListChannel = new DataListClientChannel(_client);
                }
                else
                {
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
            #region Fields

            private string _username = WindowsIdentity.GetCurrent().User.Value;
            private string _password = "asd";

            private EnvironmentConnection _environment;
            private TCPDispatchedClient _client;
            private string _hostNameOrAddress;
            private int _port;

            private ConcurrentDictionary<long, DispatcherFrameToken<INetworkMessage>> _pendingMessages = new ConcurrentDictionary<long, DispatcherFrameToken<INetworkMessage>>();
            private ServerMessaging _serverMessaging;
            private long _handles;

            private ConcurrentDictionary<Type, Guid> _messageSubscriptions = new ConcurrentDictionary<Type, Guid>();
            Guid _errorMessageSubscription;

            #endregion

            #region Properties

            public Guid AccountID
            {
                get
                {
                    return (_client == null || !_client.LoggedIn) ? Guid.Empty : _client.AccountID;
                }
            }

            public Guid ServerID
            {
                get
                {
                    return (_client == null || !_client.LoggedIn) ? Guid.Empty : _client.ServerID;
                }
            }

            #endregion

            #region Constructors

            public FrameworkDataChannelWrapper(EnvironmentConnection environment, TCPDispatchedClient client, string hostNameOrAddress, int port)
            {
                _environment = environment;
                _client = client;
                _hostNameOrAddress = hostNameOrAddress;
                _port = port;

                _serverMessaging = new ServerMessaging();
            }

            #endregion

            #region Methods

            public TCPDispatchedClient AcquireAuxiliaryConnection()
            {
                if(_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                return _client.CreateAuxiliaryClient();
            }

            public void AddDebugWriter(IDebugWriter writer)
            {
                if(_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.AddDebugWriter(writer);
            }

            public void RemoveDebugWriter(IDebugWriter writer)
            {
                if(_client == null) return;
                if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                try
                {
                    //_client.RemoveDebugWriter(writer);

                    IEventAggregator eventAggregator = ImportService.GetExportValue<IEventAggregator>();

                    if(eventAggregator != null)
                    {
                        eventAggregator.Publish(new DebugStatusMessage(false));
                    }
                }
                catch
                {
                    // Empty catch because we want to avoid hanging sessions causing an exception
                }
            }

            public void RemoveDebugWriter(Guid writerID)
            {
                if(_client == null) return;
                if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.RemoveDebugWriter(writerID);
            }

            public string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID)
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");

                string payload = _client.ExecuteCommand(xmlRequest);

                if (payload != null)
                {
                    // Only return Dev2System.ManagmentServicePayload if present ;)
                    int start = payload.IndexOf("<" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);

                    if (start > 0)
                    {
                        int end = payload.IndexOf("</" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);
                        if (start < end && (end - start) > 1)
                        {
                            // we can return the trimed payload instead

                            start += (GlobalConstants.ManagementServicePayload.Length + 2);

                            return payload.Substring(start, (end - start));
                        }
                    }
                }

                return payload;
            }

            public void Send(Packet p)
            {
                if (_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                if (!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                _client.Send(p);
            }

            /// <summary>
            /// Sends the synchronous message.
            /// </summary>
            /// <typeparam name="T">Message Type</typeparam>
            /// <param name="message">The message.</param>
            /// <exception cref="System.InvalidOperationException">A duplicate message handle has been detected in the DataListChannel.</exception>
            public INetworkMessage SendSynchronousMessage<T>(T message) where T : INetworkMessage, new()
            {
                long handle = Interlocked.Increment(ref _handles);
                DispatcherFrameToken<INetworkMessage> messageToken = new DispatcherFrameToken<INetworkMessage>(new ErrorMessage(handle, "Send message timeout."));
                if (!_pendingMessages.TryAdd(handle, messageToken))
                {
                    throw new InvalidOperationException("A duplicate message handle has been detected in the DataListChannel.");
                }

                message.Handle = handle;

                try
                {
                    EnsureMessageSubscription<T>();
                    _serverMessaging.MessageBroker.Send(message, _client);
                }
                catch
                {
                    DispatcherFrameToken<INetworkMessage> tmpToken;
                    if (!_pendingMessages.TryRemove(handle, out tmpToken))
                    {
                        tmpToken.SetResponse(null);
                    }
                }
                return messageToken.WaitForResponse(GlobalConstants.NetworkTimeOut);
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Ensures there is a subscription to the message aggregator for the specified message type.
            /// </summary>
            /// <typeparam name="T">The message type.</typeparam>
            private void EnsureMessageSubscription<T>() where T : INetworkMessage, new()
            {
                Guid _messageSubscriptionToken;
                if (!_messageSubscriptions.TryGetValue(typeof(T), out _messageSubscriptionToken))
                {
                    _messageSubscriptionToken = _serverMessaging.MessageAggregator.Subscribe(new Action<T, INetworkOperator>(RecieveSynchronousMessage));
                    if (!_messageSubscriptions.TryAdd(typeof(T), _messageSubscriptionToken))
                    {
                        _serverMessaging.MessageAggregator.Unsubscibe(_messageSubscriptionToken);
                    }
                }

                if (_errorMessageSubscription == Guid.Empty)
                {
                    _errorMessageSubscription = _serverMessaging.MessageAggregator.Subscribe(new Action<ErrorMessage, INetworkOperator>(RecieveSynchronousMessage));
                }
            }

            /// <summary>
            /// Completes the recieve of a synchronous message.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="context">The network context.</param>
            private void RecieveSynchronousMessage<T>(T message, INetworkOperator context) where T : INetworkMessage
            {
                if (message is ErrorMessage)
                {
                    //
                    // Release all message in the event of an error, this is to prevent the studio from hanging.
                    //
                    DispatcherFrameToken<INetworkMessage> messageToken;
                    if (_pendingMessages.TryRemove(message.Handle, out messageToken))
                    {
                        messageToken.SetResponse(null);
                    }
                }
                else
                {
                    DispatcherFrameToken<INetworkMessage> messageToken;
                    if (_pendingMessages.TryRemove(message.Handle, out messageToken))
                    {
                        messageToken.SetResponse(message);
                    }
                }
            }

            private bool EnsureConnected()
            {
                if(!_client.WaitForClientDetails()) //Bug 8796, After logging in wait for client details to come through before proceeding
                {
                    throw new Exception("Retrieving client details from the server timed out.");
                }

                if(!_client.LoggedIn || _client.NetworkState != NetworkState.Online)
                {
                    if(_client.NetworkState == NetworkState.Offline)
                    {
                        NetworkStateEventArgs args = _client.Connect(_hostNameOrAddress, _port);

                        if(args.ToState == NetworkState.Online)
                        {
                            LoginStateEventArgs lArgs = _client.Login(_username, _password);
                            if(!_client.WaitForClientDetails()) //Bug 8796, After logging in wait for client details to come through before proceeding
                            {
                                throw new Exception("Retrieving client details from the server timed out.");
                            }

                            if(!lArgs.LoggedIn)
                            {
                                //System.Windows.MessageBox.Show(lArgs.Message, "Login Failed", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                                if(!string.IsNullOrEmpty(_environment.Alias))
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
                            if(!string.IsNullOrEmpty(_environment.Alias))
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

            #endregion

            #region Clean Up
           
            public void Dispose()
            {
                // Clear all waiting messages
                foreach (KeyValuePair<long, DispatcherFrameToken<INetworkMessage>> item in _pendingMessages.ToList())
                {
                    DispatcherFrameToken<INetworkMessage> messageToken;
                    if (_pendingMessages.TryRemove(item.Key, out messageToken))
            {
                        messageToken.SetResponse(null);
                    }
                }

                // Unsubscrible from message aggregator
                foreach (Guid item in _messageSubscriptions.Values.ToList())
                {
                    _serverMessaging.MessageAggregator.Unsubscibe(item);
            }
             
                if (_errorMessageSubscription != Guid.Empty)
            {
                    _serverMessaging.MessageAggregator.Unsubscibe(_errorMessageSubscription);
                }

                _serverMessaging = null;

                if(_client != null)
                {
                    try
                    {
                        _client.Disconnect();
                    }
                    catch
                    {
                    }

                    try
                    {
                        _client.Dispose();
                    }
                    catch
                    {
                    }

                    _client = null;
                }
            }

            #endregion
        }

        #endregion Private Classes
    }
}
