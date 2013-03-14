using Caliburn.Micro;
using Dev2.Common;
using Dev2.DataList.Contract.Network;
using Dev2.Diagnostics;
using Dev2.Network.Execution;
using Dev2.Network.Messages;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Dev2.Studio.Core.Network.DataList;
using Dev2.Studio.Core.Network.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Threading;
using System.Windows;
using System.Xml.Linq;
using Action = System.Action;

namespace Dev2.Studio.Core.Models
{
    public class EnvironmentModel : IEnvironmentModel
    {
        #region Class Members

        private IResourceRepository _resources;
        private int _webServerPort = int.Parse(StringResources.Default_WebServer_Port);

        #endregion Class Members

        #region Constructor

        public EnvironmentModel(IEventAggregator eventAggregator, IFrameworkSecurityContext securityContext, IEnvironmentConnection environmentConnection)
        {
            EventAggregator = eventAggregator;
            SecurityContext = securityContext;
            EnvironmentConnection = environmentConnection;

            EnvironmentConnection.LoginStateChanged += EnvironmentConnection_LoginStateChanged;
        }

        #endregion Constructor

        #region Properties

        public Guid ID { get; set; }

        // BUG: 8786 - TWR - 2013.02.20 - Added category
        public string Category { get; set; }

        public IEventAggregator EventAggregator { get; private set; }

        public IFrameworkSecurityContext SecurityContext { get; private set; }

        public IEnvironmentConnection EnvironmentConnection { get; set; }

        public string Name
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.DisplayName;
                }
                return null;
            }
            set
            {
                if (EnvironmentConnection != null)
                {
                    EnvironmentConnection.DisplayName = value;
                }
            }

        }

        public Uri DsfAddress
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.Address;
                }
                return null;
            }
            set
            {
                if (EnvironmentConnection != null)
                {
                    EnvironmentConnection.Address = value;
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.IsConnected;
                }
                return false;
            }
        }

        public IResourceRepository ResourceRepository
        {
            get
            {
                return _resources;
            }
            set
            {
                _resources = value;
            }
        }

        public int WebServerPort
        {
            get { return _webServerPort; }
            set { _webServerPort = value; }
        }

        public Uri WebServerAddress
        {
            get
            {
                if (DsfAddress != null)
                {
                    return new Uri(string.Format("{0}://{1}:{2}", DsfAddress.Scheme, DsfAddress.Host, WebServerPort));
                }
                else
                {
                    return new Uri(StringResources.Uri_WebServer);
                }
            }
        }

        public IStudioEsbChannel DsfChannel
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.DataChannel;
                }
                return null;
            }
        }

        public INetworkExecutionChannel ExecutionChannel
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.ExecutionChannel;
                }
                return null;
            }
        }

        public INetworkDataListChannel DataListChannel
        {
            get
            {
                if (EnvironmentConnection != null)
                {
                    return EnvironmentConnection.DataListChannel;
                }
                return null;
            }
        }

        #endregion Properties

        #region Methods

        public void Connect()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            if (!Uri.IsWellFormedUriString(DsfAddress.ToString(), UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Address_Not_Valid));
            }

            EnvironmentConnection.Connect();
        }

        public void Disconnect()
        {
            if (EnvironmentConnection != null && EnvironmentConnection.IsConnected)
            {
                EnvironmentConnection.Disconnect();
            }
        }

        public void Connect(IEnvironmentModel model)
        {
            if (!model.IsConnected)
            {
                model.Connect();

                if (!model.IsConnected) throw new InvalidOperationException("Model failed to connect.");
            }

            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Name_Not_Provided));
            }

            if (!Uri.IsWellFormedUriString(DsfAddress.ToString(), UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(string.Format(StringResources.Error_Connect_Failed, StringResources.Error_DSF_Address_Not_Valid));
            }

            if (EnvironmentConnection is WrappedEnvironmentConnection)
            {
                EnvironmentConnection.Connect();
            }
            else
            {
                EnvironmentConnection = new WrappedEnvironmentConnection(model);
                EnvironmentConnection.Connect();
            }
        }

        // Not visible from the interface view
        public void Connect(string alias, Uri address)
        {
            Name = alias;
            DsfAddress = address;
            EnvironmentConnection.Connect();
            EventAggregator.Publish(new EnvironmentConnectedMessage(this));
        }

        public void LoadResources()
        {
            if (EnvironmentConnection == null)
            {
                throw new InvalidOperationException("No Environment connection available");
            }

            if (EnvironmentConnection.IsConnected)
            {

                if (DsfChannel != null)
                {
                    _resources = ResourceRepositoryFactory.CreateResourceRepository(this);
                    _resources.Load();
                }
            }
        }

        public string ToSourceDefinition()
        {
            var xml = new XElement("Source",
                new XAttribute("ID", ID),
                new XAttribute("Name", Name ?? ""),
                new XAttribute("Type", "Dev2Server"),
                new XAttribute("ConnectionString", string.Join(";",
                    string.Format("AppServerUri={0}", DsfAddress),
                    string.Format("WebServerPort={0}", WebServerPort)
                    )),
                new XElement("TypeOf", "Dev2Server"),
                new XElement("DisplayName", Name),
                new XElement("Category", Category ?? "") // BUG: 8786 - TWR - 2013.02.20 - Changed to use category
                );

            return xml.ToString();
        }

        #endregion Methods

        #region Event Handlers

        private void EnvironmentConnection_LoginStateChanged(object sender, LoginStateEventArgs e)
        {
            //
            // If application in shutdown do nothing
            //
            if (Application.Current == null)
            {
                return;
            }

            //
            // If auxilliry connection then do nothing
            //
            WrappedEnvironmentConnection connection = EnvironmentConnection as WrappedEnvironmentConnection;
            if (connection != null && connection.IsAuxiliry)
            {
                return;
            }

            if (e.LoggedIn)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => EventAggregator.Publish(new EnvironmentConnectedMessage(this))), null);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => EventAggregator.Publish(new EnvironmentDisconnectedMessage(this))), null);
            }
        }

        #endregion Event Handlers

        #region Private Class

        private sealed class WrappedEnvironmentConnection : IEnvironmentConnection
        {
            #region Class Members

            private TCPDispatchedClient _client;
            private string _alias;
            private IEnvironmentModel _underlying;

            #endregion Class Members

            #region Events

            #region NetworkStateChanged

            public event EventHandler<LoginStateEventArgs> LoginStateChanged;

            private void OnLoginStateChanged(LoginStateEventArgs args)
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

            public string Alias
            {
                get { return _alias; }
                set
                {
                    _alias = value;
                    DisplayName = value;
                }
            }

            public string DisplayName { get; set; }
            public IStudioEsbChannel DataChannel { get; set; }
            public INetworkExecutionChannel ExecutionChannel { get; set; }
            public INetworkDataListChannel DataListChannel { get; set; }
            public IFrameworkSecurityContext SecurityContext { get; set; }
            public bool IsConnected { get { return _client != null && _client.NetworkState == NetworkState.Online && _client.LoggedIn; } }
            public bool IsAuxiliry { get { return _client != null && _client.IsAuxiliary; } }

            #endregion Properties

            #region Constructor

            public WrappedEnvironmentConnection(IEnvironmentModel underlying)
            {
                _underlying = underlying;
                Address = underlying.EnvironmentConnection.Address;
                Alias = underlying.EnvironmentConnection.Alias;
                DisplayName = underlying.EnvironmentConnection.DisplayName;
                SecurityContext = underlying.EnvironmentConnection.SecurityContext;
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
                    if (_client != null)
                    {
                        _client.LoginStateChanged -= _client_LoginStateChanged;
                    }

                    FrameworkDataChannelWrapper wrapper = DataChannel as FrameworkDataChannelWrapper;
                    wrapper.Dispose();
                    wrapper = null;
                    _client = null;
                }
                DataChannel = null;

                if (!(_underlying.DsfChannel is IStudioClientContext)) throw new InvalidOperationException("Model DsfChannel is not a studio client context implementor.");
                IStudioClientContext context = (IStudioClientContext)_underlying.DsfChannel;
                Address = _underlying.EnvironmentConnection.Address;
                _client = context.AcquireAuxiliaryConnection();

                if (_client == null)
                {
                    if (!string.IsNullOrEmpty(Alias))
                    {
                        DisplayName = string.Format("{0} - (Unavailable) ", Alias);
                    }
                }
                else
                {
                    _client.LoginStateChanged += _client_LoginStateChanged;
                    DataChannel = new FrameworkDataChannelWrapper(this, _client);
                    ExecutionChannel = new ExecutionClientChannel(_client);
                    DataListChannel = new DataListClientChannel(_client);
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

            private sealed class FrameworkDataChannelWrapper : IStudioClientContext, IDisposable
            {
                #region Fields

                WrappedEnvironmentConnection _environment;
                TCPDispatchedClient _client;

                public Guid AccountID { get; set; }
                public Guid ServerID { get; set; }

                private ConcurrentDictionary<long, DispatcherFrameToken<INetworkMessage>> _pendingMessages = new ConcurrentDictionary<long, DispatcherFrameToken<INetworkMessage>>();
                private ServerMessaging _serverMessaging;
                private long _handles;

                private ConcurrentDictionary<Type, Guid> _messageSubscriptions = new ConcurrentDictionary<Type, Guid>();
                Guid _errorMessageSubscription;

                #endregion

                #region Constructors

                public FrameworkDataChannelWrapper(WrappedEnvironmentConnection environment, TCPDispatchedClient client)
                {
                    _environment = environment;
                    _client = client;

                    AccountID = client.AccountID;
                    ServerID = client.ServerID;

                    _serverMessaging = new ServerMessaging();
                }

                #endregion

                #region Methods

                public TCPDispatchedClient AcquireAuxiliaryConnection()
                {
                    if(_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                    if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                    return (_environment._underlying.DsfChannel is IStudioClientContext) ? ((IStudioClientContext)_environment._underlying.DsfChannel).AcquireAuxiliaryConnection() : null;
                }

                public void AddDebugWriter(IDebugWriter writer)
                {
                    if(_client == null) throw new ObjectDisposedException("FrameworkDataChannelWrapper");
                    if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                    if(_environment._underlying.DsfChannel is IStudioClientContext) ((IStudioClientContext)_environment._underlying.DsfChannel).AddDebugWriter(writer);
                }

                public void RemoveDebugWriter(IDebugWriter writer)
                {
                    if(_client == null) return;
                    if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                    if(_environment._underlying.DsfChannel is IStudioClientContext) ((IStudioClientContext)_environment._underlying.DsfChannel).RemoveDebugWriter(writer);
                }

                public void RemoveDebugWriter(Guid writerID)
                {
                    if(_client == null) return;
                    if(!EnsureConnected()) throw new InvalidOperationException("Connection to server could not be established.");
                    if(_environment._underlying.DsfChannel is IStudioClientContext) ((IStudioClientContext)_environment._underlying.DsfChannel).RemoveDebugWriter(writerID);
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
                    DispatcherFrameToken<INetworkMessage> messageToken;
                    if (_pendingMessages.TryRemove(message.Handle, out messageToken))
                    {
                        messageToken.SetResponse(message);
                    }
                }

                private bool EnsureConnected()
                {
                    if(_client != null)
                    {
                        if(!_client.WaitForClientDetails()) //Bug 8796, After logging in wait for client details to come through before proceeding
                        {
                            throw new Exception("Retrieving client details from the server timed out.");
                        }
                    }

                    if(_client == null || !_client.LoggedIn || _client.NetworkState != NetworkState.Online)
                    {
                        if(_client == null || _client.NetworkState == NetworkState.Offline)
                        {
                            if(_client != null)
                            {
                                _client.Dispose();
                                _client = null;
                            }

                            _client = (_environment._underlying.DsfChannel is IStudioClientContext) ? ((IStudioClientContext)_environment._underlying.DsfChannel).AcquireAuxiliaryConnection() : null;

                            if(_client != null)
                            {
                                AccountID = _client.AccountID;
                                ServerID = _client.ServerID;
                            }

                            return _client != null;
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

                #endregion
            }

            #endregion Private Classes
        }

        #endregion Private Class

        #region IEquatable

        public bool Equals(IEnvironmentModel other)
        {
            if (other == null)
            {
                return false;
            }
            return ID == other.ID;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var item = obj as IEnvironmentModel;
            return item != null && Equals(item);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        #endregion
    }
}
