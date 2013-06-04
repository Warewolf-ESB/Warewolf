using System;
using System.Network;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.DataList.Contract.Network;
using Dev2.Diagnostics;
using Dev2.ExtMethods;
using Dev2.Network;
using Dev2.Network.Execution;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;
using Dev2.Studio.Core.Diagnostics;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network.Channels;

namespace Dev2.Studio.Core.Network
{
    public abstract class TcpConnectionBase : IDisposable, IEnvironmentConnection
    {
        protected ITcpClientHost TCPHost;

        bool _isDisposed;
        Guid _networkContextDetachedSubscription;

        #region CTOR

        protected TcpConnectionBase(IFrameworkSecurityContext securityContext, Uri appServerUri, int webServerPort, IEventAggregator eventAggregator, bool isAuxiliary = false)
        {
            if(securityContext == null)
            {
                throw new ArgumentNullException("securityContext");
            }
            if(appServerUri == null)
            {
                throw new ArgumentNullException("appServerUri");
            }
            if(eventAggregator == null)
            {
                throw new ArgumentNullException("eventAggregator");
            }
            if(!Uri.IsWellFormedUriString(appServerUri.ToString(), UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException(@"URI is not well formed", "appServerUri");
            }

            EventAggregator = eventAggregator;
            SecurityContext = securityContext;
            AppServerUri = appServerUri;
            IsAuxiliary = isAuxiliary;

            var builder = new UriBuilder(appServerUri.Scheme, appServerUri.Host, webServerPort);
            WebServerUri = builder.Uri;
        }

        #endregion

        public IEventAggregator EventAggregator { get; private set; }

        public IStudioNetworkMessageAggregator MessageAggregator { get { return TCPHost == null ? null : TCPHost.MessageAggregator; } }

        public INetworkMessageBroker MessageBroker { get { return TCPHost == null ? null : TCPHost.MessageBroker; } }
        
        public IDebugWriter DebugWriter { get { return TCPHost.DebugWriter; } }

        #region Implementation of IEnvironmentConnection

        public event EventHandler<LoginStateEventArgs> LoginStateChanged;
        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        public event EventHandler<ServerStateEventArgs> ServerStateChanged;

        #region Properties

        public IFrameworkSecurityContext SecurityContext { get; private set; }

        public Uri AppServerUri { get; set; }

        public Uri WebServerUri { get; private set; }

        public string Alias { get; set; }

        public string DisplayName { get; set; }

        public bool IsAuxiliary { get; private set; }

        public IStudioEsbChannel DataChannel { get; private set; }

        public INetworkExecutionChannel ExecutionChannel { get; private set; }

        public INetworkDataListChannel DataListChannel { get; private set; }

        #endregion

        #region SendNetworkMessage

        public void SendNetworkMessage(INetworkMessage message)
        {
            Connect();
            TCPHost.SendNetworkMessage(message);
        }

        #endregion

        #region Add/RemoveDebugWriter

        public void AddDebugWriter()
        {
            if (!IsAuxiliary)
            {
                TCPHost.AddDebugWriter();
            }
        }

        public void RemoveDebugWriter()
        {
            if (!IsAuxiliary)
            {
                TCPHost.RemoveDebugWriter();
            }
        }

        #endregion

        #region RecieveNetworkMessage

        public INetworkMessage RecieveNetworkMessage(IByteReaderBase reader)
        {
            Connect();
            return TCPHost.RecieveNetworkMessage(reader);
        }

        #endregion

        #region SendReceiveNetworkMessage

        public INetworkMessage SendReceiveNetworkMessage(INetworkMessage message)
        {
            Connect();
            if (TCPHost != null)
            {
                return TCPHost.SendReceiveNetworkMessage(message);
            }
            return null;
        }

        #endregion

        #region ExecuteCommand

        public string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID)
        {
            Connect();
            var payload = TCPHost.ExecuteCommand(xmlRequest, workspaceID, dataListID);
            if(!string.IsNullOrEmpty(payload))
            {
                // Only return Dev2System.ManagmentServicePayload if present ;)
                var start = payload.IndexOf("<" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);

                if(start > 0)
                {
                    var end = payload.IndexOf("</" + GlobalConstants.ManagementServicePayload + ">", StringComparison.Ordinal);
                    if(start < end && (end - start) > 1)
                    {
                        // we can return the trimed payload instead

                        start += (GlobalConstants.ManagementServicePayload.Length + 2);

                        return payload.Substring(start, (end - start));
                    }
                }
            }
            return payload;
        }

        #endregion

        #region ConnectImpl

        protected void ConnectImpl(bool isAuxiliary)
        {
            if(IsConnected)
            {
                return;
            }

            TCPHost = CreateHost(isAuxiliary);
            InitializeHost();

            var connection = TCPHost
                .ConnectAsync(AppServerUri.DnsSafeHost, AppServerUri.Port)
                .ContinueWith(t =>
                {
                    if(t.Result)
                    {
                        var loginTask = TCPHost.LoginAsync(SecurityContext.UserIdentity);
                        return loginTask.Result;
                    }
                    return false;
                })
                .ContinueWith(t =>
                {
                    if(t.Result)
                    {
                        if (!isAuxiliary)
                        {
                            AddDebugWriter();
                        }
                        //InitializeHost();
                    }
                    else
                    {
                        FinalizeHost();
                        //TCPHost.Dispose();
                        //TCPHost = null;
                        //throw new Exception("Connection to server failed.");
                    }
                    return t.Result;
                });

            // DO NOT block the UI thread by using Wait()!!
            if(!connection.WaitWithPumping(GlobalConstants.NetworkTimeOut))
            {
                throw new Exception("Connection to server timed out.");
            }
        }

        #endregion

        #region Disconnect

        public void Disconnect()
        {
            FinalizeHost();
        }

        #endregion

        #endregion

        #region Initialize/FinalizeHost

        protected void InitializeHost()
        {
            if(TCPHost != null)
            {
                DataChannel = new DataChannel(this);
                DataListChannel = new DataListClientChannel(this);
                ExecutionChannel = new ExecutionClientChannel(this);
                TCPHost.LoginStateChanged += OnClientHostLoginStateChanged;
                TCPHost.NetworkStateChanged += OnClientHostOnNetworkStateChanged;
                TCPHost.ServerStateChanged += OnClientHostServerStateChanged;
                _networkContextDetachedSubscription = TCPHost.MessageAggregator
                    .Subscribe(new Action<NetworkContextDetachedMessage, IStudioNetworkChannelContext>(OnNetworkContextDetachedMessageReceived));
            }
        }

        void FinalizeHost()
        {
            if(TCPHost != null)
            {
                if (!IsAuxiliary)
                {
                    RemoveDebugWriter();
                }

                DataChannel = null;
                DataListChannel = null;
                ExecutionChannel = null;

                if(TCPHost.MessageAggregator != null)
                {
                    TCPHost.MessageAggregator.Unsubscibe(_networkContextDetachedSubscription);
                }

                TCPHost.Disconnect();
                TCPHost.LoginStateChanged -= OnClientHostLoginStateChanged;
                TCPHost.NetworkStateChanged -= OnClientHostOnNetworkStateChanged;
                TCPHost.ServerStateChanged -= OnClientHostServerStateChanged;
                TCPHost.Dispose();
                TCPHost = null;
            }
        }

        #endregion

        #region OnClientHostLogin/NetworkStateChanged

        void OnClientHostServerStateChanged(object sender, ServerStateEventArgs args)
        {
            if (ServerStateChanged != null)
            {
                ServerStateChanged(this, args);
            }
        }

        void OnClientHostLoginStateChanged(object sender, LoginStateEventArgs args)
        {
            if(LoginStateChanged != null)
            {
                LoginStateChanged(this, args);
            }
        }

        void OnClientHostOnNetworkStateChanged(object sender, NetworkStateEventArgs args)
        {
            if(NetworkStateChanged != null)
            {
                NetworkStateChanged(this, args);
            }
        }
        #endregion

        #region OnNetworkContextDetachedMessageReceived

        void OnNetworkContextDetachedMessageReceived(NetworkContextDetachedMessage message, IStudioNetworkChannelContext context)
        {
            if(ServerID != context.Server)
            {
                return;
            }

            Disconnect();
        }

        #endregion

        #region Implementation of IDisposable

        ~TcpConnectionBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    FinalizeHost();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion

        public abstract Guid ServerID { get; }
        public abstract Guid WorkspaceID { get; }
        public abstract bool IsConnected { get; }

        public abstract void Connect(bool isAuxiliary = false);
        protected abstract ITcpClientHost CreateHost(bool isAuxiliary);

    }
}
