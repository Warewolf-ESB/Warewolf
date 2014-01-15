using System;
using System.Collections.Generic;
using System.Net;
using System.Network;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Diagnostics;
using Dev2.ExtMethods;
using Dev2.Providers.Events;
using Dev2.Providers.Logs;
using Dev2.Security;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace Dev2.Network
{
    public class ServerProxy : IDisposable, IEnvironmentConnection
    {
        Timer _reconnectHeartbeat;

        public ServerProxy(Uri serverUri)
            : this(serverUri.ToString())
        {
        }

        public ServerProxy(string serverUri)
        {
            VerifyArgument.IsNotNull("serverUri", serverUri);
            ServerEvents = EventPublishers.Studio;

            AppServerUri = new Uri(serverUri);
            WebServerUri = new Uri(serverUri.Replace("/dsf", ""));

            HubConnection = new HubConnection(serverUri) { Credentials = CredentialCache.DefaultNetworkCredentials };
            HubConnection.Error += OnHubConnectionError;
            HubConnection.Closed += HubConnectionOnClosed;
            HubConnection.StateChanged += HubConnectionStateChanged;
            //HubConnection.TraceLevel = TraceLevels.All;
            //HubConnection.TraceWriter = Console.Out;

            InitializeEsbProxy();

            AuthorizationService = new ClientAuthorizationService(new ClientSecurityService(this));
        }

        public bool IsLocalHost
        {
            get
            {
                return DisplayName == "localhost";
            }
        }

        protected void InitializeEsbProxy()
        {
            EsbProxy = HubConnection.CreateHubProxy("esb");
            EsbProxy.On<string>("SendMemo", OnMemoReceived);
            EsbProxy.On<string>("SendDebugState", OnDebugStateReceived);
            EsbProxy.On<Guid>("SendWorkspaceID", OnWorkspaceIDReceived);
        }

        protected void LoadPermissions()
        {
            AuthorizationService.Load();
        }

        void HubConnectionOnClosed()
        {
            HasDisconnected();
        }

        void HasDisconnected()
        {
            IsConnected = false;
            StartReconnectTimer();
            OnLoginStateChanged(new LoginStateEventArgs(AuthenticationResponse.Logout, false, false, "Logged Out"));
            OnServerStateChanged(new ServerStateEventArgs(ServerState.Offline));
            OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Online, NetworkState.Offline));
        }

        void OnWorkspaceIDReceived(Guid obj)
        {
            AddDebugWriter(obj);
            WorkspaceID = obj;
        }

        void OnDebugStateReceived(string objString)
        {
            var obj = JsonConvert.DeserializeObject<DebugState>(objString);
            ServerEvents.Publish(new DebugWriterWriteMessage { DebugState = obj });
        }

        void HubConnectionStateChanged(StateChange stateChange)
        {
            switch(stateChange.NewState)
            {
                case ConnectionState.Connected:
                    IsConnected = true;
                    OnLoginStateChanged(new LoginStateEventArgs(AuthenticationResponse.Success, true, false, "Logged In"));
                    OnServerStateChanged(new ServerStateEventArgs(ServerState.Online));
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
                    LoadPermissions();
                    break;
                case ConnectionState.Connecting:
                case ConnectionState.Reconnecting:
                    IsConnected = false;
                    OnLoginStateChanged(new LoginStateEventArgs(AuthenticationResponse.Success, true, false, "Logged In"));
                    OnServerStateChanged(new ServerStateEventArgs(ServerState.Offline));
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Connecting));
                    break;
                case ConnectionState.Disconnected:
                    HasDisconnected();
                    break;
            }


        }

        public bool IsConnected { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public void Connect()
        {
            try
            {
                if(HubConnection.State == ConnectionState.Disconnected)
                {
                    //TODO: take the waiting off the UI thread 
                    //var t = HubConnection.Start();
                    //Wait(t);
                    HubConnection.Start().Wait();
                }
            }
            catch(Exception e)
            {
                Logger.Error(e);
                StartReconnectTimer();
            }
        }

        protected virtual void StartReconnectTimer()
        {
            if(IsLocalHost)
            {
                if(_reconnectHeartbeat == null)
                {
                    _reconnectHeartbeat = new Timer();
                    _reconnectHeartbeat.Elapsed += OnReconnectHeartbeatElapsed;
                    _reconnectHeartbeat.Interval = 10000;
                    _reconnectHeartbeat.AutoReset = true;
                    _reconnectHeartbeat.Start();
                }
            }
        }

        public virtual void StopReconnectHeartbeat()
        {
            this.TraceInfo();
            if(_reconnectHeartbeat != null)
            {
                _reconnectHeartbeat.Stop();
                _reconnectHeartbeat.Dispose();
                _reconnectHeartbeat = null;
            }
        }


        void OnReconnectHeartbeatElapsed(object sender, ElapsedEventArgs args)
        {
            this.TraceInfo();
            Connect();
            if(IsConnected)
            {
                StopReconnectHeartbeat();
            }
        }

        public void Disconnect()
        {
            HubConnection.Stop();
        }

        public void Verify(Action<ConnectResult> callback)
        {
            if(IsConnected)
            {
                return;
            }
            try
            {
                Connect();
                callback(ConnectResult.Success);
            }
            catch(Exception e)
            {
                Logger.Error(e);
                callback(ConnectResult.ConnectFailed);
            }
        }

        public void StartAutoConnect()
        {
            if(IsConnected)
            {
                return;
            }
            StartReconnectTimer();
        }

        public IAuthorizationService AuthorizationService { get; private set; }

        public IEventPublisher ServerEvents { get; set; }

        public IHubProxy EsbProxy { get; protected set; }

        public HubConnection HubConnection { get; private set; }

        void OnHubConnectionError(Exception exception)
        {
            Logger.Error(exception);
        }

        void OnMemoReceived(string objString)
        {
            var obj = JsonConvert.DeserializeObject<DesignValidationMemo>(objString);
            this.TraceInfo("Publish message of type - " + typeof(Memo));
            ServerEvents.PublishObject(obj);
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Guid ServerID { get; private set; }
        public Guid WorkspaceID { get; private set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public IDebugWriter DebugWriter { get; private set; }
        public Uri AppServerUri { get; private set; }
        public Uri WebServerUri { get; private set; }
        public event EventHandler<LoginStateEventArgs> LoginStateChanged;

        protected virtual void OnLoginStateChanged(LoginStateEventArgs e)
        {
            var handler = LoginStateChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;

        protected virtual void OnNetworkStateChanged(NetworkStateEventArgs e)
        {
            var handler = NetworkStateChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<ServerStateEventArgs> ServerStateChanged;

        protected virtual void OnServerStateChanged(ServerStateEventArgs e)
        {
            var handler = ServerStateChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }


        public StringBuilder ExecuteCommand(StringBuilder payload, Guid workspaceID, Guid dataListID)
        {
            if(payload == null || payload.Length == 0)
            {
                throw new ArgumentNullException("payload");
            }

            // build up payload 
            var length = payload.Length;
            var startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

            var messageID = Guid.NewGuid();
            List<Envelope> mailToSend = new List<Envelope>();
            for(int i = 0; i < rounds; i++)
            {
                var envelope = new Envelope { PartID = i, Type = typeof(Envelope) };

                var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                if(len > (payload.Length - startIdx))
                {
                    len = (payload.Length - startIdx);
                }

                envelope.Content = payload.Substring(startIdx, len);
                startIdx += len;

                mailToSend.Add(envelope);
            }

            // Send and receive chunks from the server ;)
            var result = new StringBuilder();
            for(int i = 0; i < mailToSend.Count; i++)
            {
                bool isEnd = (i + 1 == mailToSend.Count);
                Task<Receipt> invoke = EsbProxy.Invoke<Receipt>("ExecuteCommand", mailToSend[i], isEnd, workspaceID, dataListID, messageID);
                Wait(invoke);

                // now build up the result in fragements ;)
                if(isEnd)
                {
                    var totalToFetch = invoke.Result.ResultParts;
                    for(int q = 0; q < totalToFetch; q++)
                    {
                        Task<string> fragmentInvoke = EsbProxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = q, RequestID = messageID });
                        Wait(fragmentInvoke);
                        if(fragmentInvoke.Result != null)
                        {
                            result.Append(fragmentInvoke.Result);
                        }
                    }
                }
            }

            // prune any result for old datalist junk ;)
            if(result.Length > 0)
            {
                // Only return Dev2System.ManagmentServicePayload if present ;)
                var start = result.LastIndexOf("<" + GlobalConstants.ManagementServicePayload + ">", false);
                if(start > 0)
                {
                    var end = result.LastIndexOf("</" + GlobalConstants.ManagementServicePayload + ">", false);
                    if(start < end && (end - start) > 1)
                    {
                        // we can return the trimed payload instead
                        start += (GlobalConstants.ManagementServicePayload.Length + 2);
                        return new StringBuilder(result.Substring(start, (end - start)));
                    }
                }
            }

            return result;

        }

        protected virtual T Wait<T>(Task<T> task)
        {
            try
            {
                task.WaitWithPumping(GlobalConstants.NetworkTimeOut);
                return task.Result;
            }
            catch(Exception e)
            {
                Logger.Error(e);
            }
            return default(T);
        }

        protected virtual void Wait(Task task)
        {
            task.Wait(100);
        }

        public void AddDebugWriter(Guid workspaceID)
        {
            var t = EsbProxy.Invoke("AddDebugWriter", workspaceID);
            Wait(t);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion
    }
}
