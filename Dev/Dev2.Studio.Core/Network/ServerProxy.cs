
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
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Network;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Communication;
using Dev2.ConnectionHelpers;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Explorer;
using Dev2.ExtMethods;
using Dev2.Messages;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.AspNet.SignalR.Client;
using ServiceStack.Messaging.Rcon;

namespace Dev2.Network
{
    public class ServerProxy : IDisposable, IEnvironmentConnection
    {
        System.Timers.Timer _reconnectHeartbeat;
        private const int MillisecondsTimeout = 10000;
        readonly IAsyncWorker _asyncWorker;
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

        public ServerProxy(Uri serverUri)
            : this(serverUri.ToString(), CredentialCache.DefaultNetworkCredentials, new AsyncWorker())
        {
            AuthenticationType = AuthenticationType.Windows;
            Principal = ClaimsPrincipal.Current;
        }

        public static bool IsShuttingDown { get; private set; }

        public ServerProxy(string serverUri, ICredentials credentials, IAsyncWorker worker)
        {

            IsAuthorized = true;
            VerifyArgument.IsNotNull("serverUri", serverUri);
            ServerEvents = EventPublishers.Studio;

            var uriString = serverUri;
            if(!serverUri.EndsWith("dsf"))
            {
                uriString = serverUri + (serverUri.EndsWith("/") ? "" : "/") + "dsf";
            }
            AppServerUri = new Uri(uriString);
            WebServerUri = new Uri(uriString.Replace("/dsf", ""));


            Dev2Logger.Log.Debug("***** Attempting Server Hub : " + uriString + " -> " + CredentialCache.DefaultNetworkCredentials.Domain + @"\" + CredentialCache.DefaultNetworkCredentials.UserName);
            HubConnection = new HubConnection(uriString) { Credentials = credentials };
            HubConnection.Error += OnHubConnectionError;
            HubConnection.Closed += HubConnectionOnClosed;
            HubConnection.StateChanged += HubConnectionStateChanged;
            //HubConnection.TraceLevel = TraceLevels.All;
            //HubConnection.TraceWriter = new Dev2LoggingTextWriter();
            InitializeEsbProxy();
            _asyncWorker = worker;

        }




        public IPrincipal Principal { get; set; }

        public ServerProxy(string webAddress, string userName, string password)
            : this(webAddress, new NetworkCredential(userName, password), new AsyncWorker())
        {
            UserName = userName;
            Password = password;
            AuthenticationType = userName == "\\" ? AuthenticationType.Public : AuthenticationType.User;
            if(AuthenticationType == AuthenticationType.Public)
            {
                Principal = null;
            }           
        }

        public bool IsLocalHost { get { return DisplayName == "localhost"; } }

        protected void InitializeEsbProxy()
        {
            if(EsbProxy == null)
            {
                EsbProxy = HubConnection.CreateHubProxy("esb");
                EsbProxy.On<string>("SendMemo", OnMemoReceived);
                EsbProxy.On<string>("ReceiveResourcesAffectedMemo", OnReceiveResourcesAffectedMemo);
                EsbProxy.On<string>("SendPermissionsMemo", OnPermissionsMemoReceived);
                EsbProxy.On<string>("SendDebugState", OnDebugStateReceived);
                EsbProxy.On<Guid>("SendWorkspaceID", OnWorkspaceIdReceived);
                EsbProxy.On<Guid>("SendServerID", OnServerIdReceived);
                EsbProxy.On<string>("ItemUpdatedMessage", OnItemUpdatedMessageReceived);
                EsbProxy.On<string>("ItemDeletedMessage", OnItemDeletedMessageReceived);
                EsbProxy.On<string>("ItemAddedMessage", OnItemAddedMessageReceived);
            }
        }

        public Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage {get;set;}
        void OnReceiveResourcesAffectedMemo(string objString)
        {
            var obj = _serializer.Deserialize<CompileMessageList>(objString);
            if (ReceivedResourceAffectedMessage != null)
            {
                ReceivedResourceAffectedMessage(obj.ServiceID,obj);
            }
        }

        void HubConnectionOnClosed()
        {
            HasDisconnected();
        }

        void HasDisconnected()
        {
            Dev2Logger.Log.Debug("*********** Hub connection down");
            IsConnected = false;
            if(IsShuttingDown)
            {
                return;
            }
            StartReconnectTimer();
            OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Online, NetworkState.Offline));
        }

        void OnWorkspaceIdReceived(Guid obj)
        {
            AddDebugWriter(obj);
            WorkspaceID = obj;
        }

        void OnServerIdReceived(Guid obj)
        {
            ServerID = obj;
        }

        void OnDebugStateReceived(string objString)
        {

            var obj = _serializer.Deserialize<DebugState>(objString);
            Dev2Logger.Log.Info(string.Format("Debug Item Received ID {0}" + Environment.NewLine + "Parent ID:{1}" + "Name: {2}", obj.EnvironmentID, obj.ParentID, obj.Name));
            ServerEvents.Publish(new DebugWriterWriteMessage { DebugState = obj });
        }

        protected void HubConnectionStateChanged(StateChange stateChange)
        {
            switch(stateChange.NewState)
            {
                case ConnectionState.Connected:
                    IsConnected = true;
                    UpdateIsAuthorized(true);
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
                    break;
                case ConnectionState.Connecting:
                case ConnectionState.Reconnecting:
                    IsConnected = false;
                    UpdateIsAuthorized(false);
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

        public void Connect(Guid id)
        {
            ID = id;
            try
            {
                if(!IsLocalHost)
                {
                    if(HubConnection.State == ConnectionState.Reconnecting)
                    {
                        HubConnection.Stop(new TimeSpan(0, 0, 0, 1));
                    }
                }

                if(HubConnection.State == ConnectionState.Disconnected)
                {
                    ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                    if(!HubConnection.Start().Wait(5000))
                    {
                        if(!IsLocalHost)
                        {
                            ConnectionRetry();
                        }
                    }
                }
            }
            catch(AggregateException aex)
            {
                aex.Flatten();
                aex.Handle(ex =>
                {
                    Dev2Logger.Log.Error(this, aex);
                    var hex = ex as HttpClientException;
                    if(hex != null)
                    {
                        switch(hex.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new NotConnectedException();
                        }
                    }
                    throw new NotConnectedException();
                });
            }
            catch(NotConnectedException)
            {
                throw;
            }
            catch(Exception e)
            {
                HandleConnectError(e);
            }
        }

        private void ConnectionRetry()
        {
            HubConnection.Stop(new TimeSpan(0, 0, 0, 1));
            IPopupController popup = CustomContainer.Get<IPopupController>();

            var application = Application.Current;
            MessageBoxResult res = MessageBoxResult.No;
            if(application != null && application.Dispatcher != null)
            {
                application.Dispatcher.Invoke(() =>
                    {
                        res = popup.ShowConnectionTimeoutConfirmation(DisplayName);
                    });
            }

            if(res == MessageBoxResult.Yes)
            {
                if(!HubConnection.Start().Wait(5000))
                {
                    ConnectionRetry();
                }
            }
            else
            {
                throw new NotConnectedException();
            }
        }

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        void HandleConnectError(Exception e)
        {
            Dev2Logger.Log.Error(this, e);
            StartReconnectTimer();
        }

        protected virtual void StartReconnectTimer()
        {
            if(IsLocalHost)
            {
                if(_reconnectHeartbeat == null)
                {
                    _reconnectHeartbeat = new System.Timers.Timer();
                    _reconnectHeartbeat.Elapsed += OnReconnectHeartbeatElapsed;
                    _reconnectHeartbeat.Interval = 3000;
                    _reconnectHeartbeat.AutoReset = true;
                    _reconnectHeartbeat.Start();
                }
            }
        }

        public virtual void StopReconnectHeartbeat()
        {
            Dev2Logger.Log.Info("");
            if(_reconnectHeartbeat != null)
            {
                _reconnectHeartbeat.Stop();
                _reconnectHeartbeat.Dispose();
                _reconnectHeartbeat = null;
            }
        }


        void OnReconnectHeartbeatElapsed(object sender, ElapsedEventArgs args)
        {
            Dev2Logger.Log.Info("");
            Connect(ID);
            if(IsConnected)
            {
                StopReconnectHeartbeat();
                ConnectControlSingleton.Instance.Refresh(Guid.Empty);
            }
        }

        public void Disconnect()
        {
            // It can take some time to shutdown when permissions have changed ;(
            // Give 5 seconds, then force a dispose ;)
            try
            {
                IsShuttingDown = true;
                IsConnected = false;
                Task.Run(() => HubConnection.Stop(new TimeSpan(0, 0, 1))).RunSynchronously();
            }
            catch(AggregateException aex)
            {
                aex.Flatten();
                aex.Handle(ex =>
                {
                    Dev2Logger.Log.Error(this, aex);
                    var hex = ex as HttpClientException;
                    if(hex != null)
                    {
                        switch(hex.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new NotConnectedException();
                        }
                    }
                    throw new NotConnectedException();
                });
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(this, e);
            }
        }

        public void Verify(Action<ConnectResult> callback, bool wait = true)
        {
            if(IsConnected)
            {
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            if(wait)
            {
                HubConnection.Start().Wait(MillisecondsTimeout);
                callback(HubConnection.State == ConnectionState.Connected
                             ? ConnectResult.Success
                             : ConnectResult.ConnectFailed);
            }
            else
            {
                HubConnection.Start();
                AsyncWorker.Start(() => Thread.Sleep(MillisecondsTimeout), () => callback(HubConnection.State == ConnectionState.Connected
                                     ? ConnectResult.Success
                                     : ConnectResult.ConnectFailed));
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

        public IEventPublisher ServerEvents { get; set; }

        public IHubProxy EsbProxy { get; protected set; }

        public HubConnection HubConnection { get; private set; }

        void OnHubConnectionError(Exception exception)
        {
            Dev2Logger.Log.Error(this, exception);
        }

        void OnMemoReceived(string objString)
        {
            // DO NOT use publish as memo is of type object 
            // and hence won't find the correct subscriptions
            Dev2Logger.Log.Info("Memo Received [ " + objString + " ]");

            var obj = _serializer.Deserialize<DesignValidationMemo>(objString);
            ServerEvents.PublishObject(obj);
        }

        void OnPermissionsMemoReceived(string objString)
        {
            // DO NOT use publish as memo is of type object 
            // and hence won't find the correct subscriptions
            Dev2Logger.Log.Info("PERMISSIONS MEMO OBJECT [ " + objString + " ]");
            var obj = _serializer.Deserialize<PermissionsModifiedMemo>(objString);
            try
            {
                // When we connect against group A with Administrator perms, and we remove all permissions, a 403 will be thrown. 
                // Handle it more gracefully ;)
                RaisePermissionsModified(obj.ModifiedPermissions);
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(this, e);
            }
            RaisePermissionsChanged();
        }

        public Action<IExplorerItem> ItemAddedMessageAction { get; set; }

        void OnItemAddedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            serverExplorerItem.ServerId = ID;
            if(ItemAddedMessageAction != null)
            {
                ItemAddedMessageAction(serverExplorerItem);
            }
        }

        public Action<IExplorerItem> ItemItemDeletedMessageAction { get; set; }
        void OnItemDeletedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            serverExplorerItem.ServerId = ID;
            if(ItemItemDeletedMessageAction != null)
            {
                ItemItemDeletedMessageAction(serverExplorerItem);
            }
            //Logger.TraceInfo(string.Format("Debug Item Received ID {0}" + Environment.NewLine + "Parent ID:{1}" + "Name: {2}", obj.ID, obj.ParentID, obj.Name));
        }

        public Action<IExplorerItem> ItemItemUpdatedMessageAction { get; set; }
        void OnItemUpdatedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            if(ItemItemUpdatedMessageAction != null)
            {
                ItemItemUpdatedMessageAction(serverExplorerItem);
            }
        }

        public Guid ServerID { get; protected set; }
        public Guid WorkspaceID { get; private set; }
        public Uri AppServerUri { get; private set; }
        public Uri WebServerUri { get; private set; }
        public AuthenticationType AuthenticationType { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        /// <summary>
        /// <code>True</code> unless server returns Unauthorized or Forbidden status.
        /// </summary>
        public bool IsAuthorized { get; private set; }
        public IAsyncWorker AsyncWorker
        {
            get
            {
                return _asyncWorker;
            }
        }

        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        public event EventHandler PermissionsChanged;

        void RaisePermissionsChanged()
        {
            if(PermissionsChanged != null)
            {
                PermissionsChanged(this, EventArgs.Empty);
            }
        } 
        
        public event EventHandler<List<WindowsGroupPermission>> PermissionsModified;

        void RaisePermissionsModified(List<WindowsGroupPermission> args)
        {
            if (PermissionsModified != null)
            {
                PermissionsModified(this, args);
            }
        }

        void UpdateIsAuthorized(bool isAuthorized)
        {
            Dev2Logger.Log.Debug("UpdateIsAuthorized [ " + isAuthorized + " ]");
            if(IsAuthorized != isAuthorized)
            {
                IsAuthorized = isAuthorized;
                RaisePermissionsChanged();
            }
        }

        protected virtual void OnNetworkStateChanged(NetworkStateEventArgs e)
        {
            var handler = NetworkStateChanged;
            if(handler != null)
            {
                handler(this, e);
            }
        }

        public StringBuilder ExecuteCommand(StringBuilder payload, Guid workspaceId, Guid dataListId)
        {
            if(payload == null || payload.Length == 0)
            {
                throw new ArgumentNullException("payload");
            }

            Dev2Logger.Log.Debug("Execute Command Payload [ " + payload + " ]");

            // build up payload 
            var length = payload.Length;
            var startIdx = 0;
            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

            var messageId = Guid.NewGuid();
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
                Task<Receipt> invoke = EsbProxy.Invoke<Receipt>("ExecuteCommand", mailToSend[i], isEnd, workspaceId, dataListId, messageId);
                Wait(invoke, result);
                if(invoke.IsFaulted)
                {
                    break;
                }
                // now build up the result in fragments ;)
                if(isEnd)
                {
                    var totalToFetch = invoke.Result.ResultParts;
                    for(int q = 0; q < totalToFetch; q++)
                    {
                        Task<string> fragmentInvoke = EsbProxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = q, RequestID = messageId });
                        Wait(fragmentInvoke, result);
                        if(!fragmentInvoke.IsFaulted && fragmentInvoke.Result != null)
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
                        // we can return the trimmed payload instead
                        start += (GlobalConstants.ManagementServicePayload.Length + 2);
                        return new StringBuilder(result.Substring(start, (end - start)));
                    }
                }
            }

            return result;

        }

        protected virtual T Wait<T>(Task<T> task, StringBuilder result)
        {
            return Wait(task, result, GlobalConstants.NetworkTimeOut);
        }

        protected T Wait<T>(Task<T> task, StringBuilder result, int millisecondsTimeout)
        {
            try
            {
                task.WaitWithPumping(millisecondsTimeout);
                return task.Result;
            }
            catch(AggregateException aex)
            {
                var hasDisconnected = false;
                aex.Handle(ex =>
                {
                    result.AppendFormat("<Error>{0}</Error>", ex.Message);
                    var hex = ex as HttpRequestException;
                    if(hex != null)
                    {
                        hasDisconnected = true;
                        return true; // This we know how to handle this
                    }
                    var ioex = ex as InvalidOperationException;
                    if(ioex != null && ioex.Message.Contains(@"Connection started reconnecting before invocation result was received"))
                    {
                        Dev2Logger.Log.Debug("Connection is reconnecting");
                        return true; // This we know how to handle this
                    }
                    // handle 403 errors when permissions have been removed ;)
                    var hce = ex as HttpClientException;
                    if(hce != null && hce.Message.Contains("StatusCode: 403"))
                    {
                        Dev2Logger.Log.Debug("Forbidden - Most Likely Permissions Changed.");
                        // Signal not-authorized anymore ;)
                        UpdateIsAuthorized(false);
                        return true;
                    }

                    // handle generic errors ;)                   
                    Dev2Logger.Log.Error(this, ex);
                    return true; // Let anything else stop the application.
                });
                if(hasDisconnected)
                {
                    HasDisconnected();
                }
            }
            return default(T);
        }

        public void AddDebugWriter(Guid workspaceId)
        {
            var t = EsbProxy.Invoke("AddDebugWriter", workspaceId);
            Wait(t);
        }

        protected virtual void Wait(Task task)
        {
            task.Wait(100);
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        public Guid ID { get; private set; }

    }
}
