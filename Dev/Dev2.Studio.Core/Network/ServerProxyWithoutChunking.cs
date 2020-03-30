#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Explorer;
using Dev2.Messages;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Events;
using Dev2.Services.Security;
using Dev2.SignalR.Wrappers;
using Dev2.SignalR.Wrappers.New;
using Dev2.Studio.Interfaces;
using Dev2.Threading;
using Microsoft.AspNet.SignalR.Client;
using ServiceStack.Messaging.Rcon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Network;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Esb;

namespace Dev2.Network
{
    public class ServerProxyWithoutChunking : IEnvironmentConnection, IDisposable
    {
        const int MillisecondsTimeout = 10000;
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

        static ServerProxyWithoutChunking()
        {
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
        }

        public ServerProxyWithoutChunking(Uri serverUri)
            : this(serverUri.ToString(), CredentialCache.DefaultNetworkCredentials, new AsyncWorker())
        {
            AuthenticationType = AuthenticationType.Windows;
        }

        static bool IsShuttingDown { get; set; }

        public ServerProxyWithoutChunking(string serverUri, ICredentials credentials, IAsyncWorker worker)
        {
            IsAuthorized = true;
            VerifyArgument.IsNotNull("serverUri", serverUri);
            ServerEvents = EventPublishers.Studio;

            var uriString = serverUri;
            if (!serverUri.EndsWith("dsf"))
            {
                uriString = serverUri + (serverUri.EndsWith("/") ? "" : "/") + "dsf";
            }
            Principal = ClaimsPrincipal.Current;
            AppServerUri = new Uri(uriString);
            WebServerUri = new Uri(uriString.Replace("/dsf", ""));
            Dev2Logger.Debug(credentials, "Warewolf Debug");
            Dev2Logger.Debug("***** Attempting Server Hub : " + uriString + " -> " + CredentialCache.DefaultNetworkCredentials.Domain + @"\" + Principal.Identity.Name, "Warewolf Debug");
            HubConnection = new HubConnectionWrapper(uriString) { Credentials = credentials };
            HubConnection.Error += OnHubConnectionError;
            HubConnection.Closed += HubConnectionOnClosed;
            HubConnection.StateChanged += HubConnectionStateChanged;
            InitializeEsbProxy();
            AsyncWorker = worker;
        }

        public IPrincipal Principal { get; private set; }

        public ServerProxyWithoutChunking(string webAddress, string userName, string password)
            : this(webAddress, new NetworkCredential(userName, password), new AsyncWorker())
        {
            UserName = userName;
            Password = password;
            AuthenticationType = userName == "\\" ? AuthenticationType.Public : AuthenticationType.User;
            if (AuthenticationType == AuthenticationType.Public)
            {
                Principal = null;
            }
        }

        public bool IsLocalHost
        {
            get
            {
                if (string.IsNullOrEmpty(DisplayName))
                {
                    return false;
                }
                var displayName = DisplayName.ToLower();
                var isLocalHost = (displayName == "localhost") || (displayName == "localhost (connected)");
                return isLocalHost;
            }
        }

        void InitializeEsbProxy()
        {
            if (EsbProxy == null)
            {
                EsbProxy = HubConnection.CreateHubProxy("esb");
                EsbProxy.On<string>("SendMemo", OnMemoReceived);
                EsbProxy.On<string>("SendPermissionsMemo", OnPermissionsMemoReceived);
                EsbProxy.On<string>("SendDebugState", OnDebugStateReceived);
                EsbProxy.On<Guid>("SendWorkspaceID", OnWorkspaceIdReceived);
                EsbProxy.On<Guid>("SendServerID", OnServerIdReceived);
                EsbProxy.On<string>("ItemUpdatedMessage", OnItemUpdatedMessageReceived);
                EsbProxy.On<string>("ItemDeletedMessage", OnItemDeletedMessageReceived);
                EsbProxy.On<string>("ItemAddedMessage", OnItemAddedMessageReceived);
            }
        }

        public Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage { get; set; }

        public void FetchResourcesAffectedMemo(Guid resourceId)
        {
            if (ReceivedResourceAffectedMessage != null)
            {
                var result = Task.Run(async () => await EsbProxy.Invoke<string>("FetchResourcesAffectedMemo", resourceId).ConfigureAwait(true)).GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(result))
                {
                    FetchResourcesAffectedMemo(result);
                }
            }
        }

        void FetchResourcesAffectedMemo(string result)
        {
            var obj = _serializer.Deserialize<CompileMessageList>(result);
            if (obj != null)
            {
                ReceivedResourceAffectedMessage.Invoke(obj.ServiceID, obj);
                var shellViewModel = CustomContainer.Get<IShellViewModel>();
                if (shellViewModel != null)
                {
                    shellViewModel.ResourceCalled = false;
                }
            }
        }

        void HubConnectionOnClosed()
        {
            HasDisconnected();
        }

        void HasDisconnected()
        {
            Dev2Logger.Debug("*********** Hub connection down", "Warewolf Debug");
            IsConnected = false;
            IsConnecting = false;
            if (IsShuttingDown)
            {
                return;
            }

            if (HubConnection.State != ConnectionStateWrapped.Disconnected)
            {
                OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Online, NetworkState.Offline));
            }
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
            ServerEvents.Publish(new DebugWriterWriteMessage { DebugState = obj });
        }

        protected void HubConnectionStateChanged(IStateChangeWrapped stateChange)
        {
            switch (stateChange.NewState)
            {
                case ConnectionStateWrapped.Connected:
                    IsConnected = true;
                    IsConnecting = false;
                    UpdateIsAuthorized(true);
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Offline, NetworkState.Online));
                    break;

                case ConnectionStateWrapped.Connecting:
                case ConnectionStateWrapped.Reconnecting:
                    IsConnected = false;
                    IsConnecting = true;
                    UpdateIsAuthorized(false);
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Online, NetworkState.Offline));
                    break;

                default:
                case ConnectionStateWrapped.Disconnected:
                    OnNetworkStateChanged(new NetworkStateEventArgs(NetworkState.Online, NetworkState.Offline));
                    HasDisconnected();
                    break;
            }
        }

        public bool IsConnected { get; private set; }
        public bool IsConnecting { get; private set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }

        public void Connect(Guid id)
        {
            // bug: since Connect doesn't control connection parameters, Id should not be set from here.
            ID = id;
            try
            {
                if (!IsConnecting)
                {
                    ConnectAsync(id);
                }
                //ensureConnectedWaitTask.Wait(Config.Studio.ConnectTimeout);
            }
            catch (AggregateException aex)
            {
                aex.Flatten();
                aex.Handle(ex =>
                {
                    Dev2Logger.Error(this, aex, "Warewolf Error");
                    if (ex is HttpClientException hex && (hex.Response.StatusCode == HttpStatusCode.Unauthorized || hex.Response.StatusCode == HttpStatusCode.Forbidden))
                    {
                        UpdateIsAuthorized(false);
                        throw new UnauthorizedAccessException();
                    }
                    throw ex;
                });
            }
            catch (Exception e)
            {
                HandleConnectError(e);
            }
        }

        public Task<bool> ConnectAsync(Guid id)
        {
            ID = id;
            var ensureConnectedWaitTask = new Task<bool>(() =>
            {
                while (IsConnected == false)
                {
                    Task.Delay(300).Wait();
                    Task.Yield();
                }
                return true;
            });
            ensureConnectedWaitTask.Start();
            return ensureConnectedWaitTask;
        }

        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors) => true;

        void HandleConnectError(Exception e)
        {
            Dev2Logger.Error(this, e, "Warewolf Error");
        }

        public void Disconnect()
        {
            // It can take some time to shutdown when permissions have changed ;(
            // Give 5 seconds, then force a dispose ;)
            try
            {
                IsShuttingDown = true;
                HubConnection.Stop(new TimeSpan(0, 0, 0, 5));
            }
            catch (AggregateException aex)
            {
                aex.Flatten();
                aex.Handle(ex =>
                {
                    Dev2Logger.Error(this, aex, "Warewolf Error");
                    if (ex is HttpClientException hex)
                    {
                        switch (hex.Response.StatusCode)
                        {
                            case HttpStatusCode.Unauthorized:
                            case HttpStatusCode.Forbidden:
                                UpdateIsAuthorized(false);
                                throw new NotConnectedException();
                            case HttpStatusCode.Continue:
                            case HttpStatusCode.SwitchingProtocols:
                            case HttpStatusCode.OK:
                            case HttpStatusCode.Created:
                            case HttpStatusCode.Accepted:
                            case HttpStatusCode.NonAuthoritativeInformation:
                            case HttpStatusCode.NoContent:
                            case HttpStatusCode.ResetContent:
                            case HttpStatusCode.PartialContent:
                            case HttpStatusCode.MultipleChoices:
                            case HttpStatusCode.MovedPermanently:
                            case HttpStatusCode.Found:
                            case HttpStatusCode.SeeOther:
                            case HttpStatusCode.NotModified:
                            case HttpStatusCode.UseProxy:
                            case HttpStatusCode.Unused:
                            case HttpStatusCode.TemporaryRedirect:
                            case HttpStatusCode.BadRequest:
                            case HttpStatusCode.PaymentRequired:
                            case HttpStatusCode.NotFound:
                            case HttpStatusCode.MethodNotAllowed:
                            case HttpStatusCode.NotAcceptable:
                            case HttpStatusCode.ProxyAuthenticationRequired:
                            case HttpStatusCode.RequestTimeout:
                            case HttpStatusCode.Conflict:
                            case HttpStatusCode.Gone:
                            case HttpStatusCode.LengthRequired:
                            case HttpStatusCode.PreconditionFailed:
                            case HttpStatusCode.RequestEntityTooLarge:
                            case HttpStatusCode.RequestUriTooLong:
                            case HttpStatusCode.UnsupportedMediaType:
                            case HttpStatusCode.RequestedRangeNotSatisfiable:
                            case HttpStatusCode.ExpectationFailed:
                            case HttpStatusCode.UpgradeRequired:
                            case HttpStatusCode.InternalServerError:
                            case HttpStatusCode.NotImplemented:
                            case HttpStatusCode.BadGateway:
                            case HttpStatusCode.ServiceUnavailable:
                            case HttpStatusCode.GatewayTimeout:
                            case HttpStatusCode.HttpVersionNotSupported:
                            default:
                                throw new NotConnectedException();
                        }
                    }
                    return false;
                });
            }
            catch (Exception e)
            {
                Dev2Logger.Error(this, e, "Warewolf Error");
            }
        }

        public void Verify(Action<ConnectResult> callback) => Verify(callback, true);

        public void Verify(Action<ConnectResult> callback, bool wait)
        {
            if (IsConnected)
            {
                return;
            }
            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;

            if (wait)
            {
                callback?.Invoke(HubConnection.State == (ConnectionStateWrapped)ConnectionState.Connected
                             ? ConnectResult.Success
                             : ConnectResult.ConnectFailed);
            }
            else
            {
                AsyncWorker.Start(() => Thread.Sleep(MillisecondsTimeout), () => callback?.Invoke(HubConnection.State == (ConnectionStateWrapped)ConnectionState.Connected
                                     ? ConnectResult.Success
                                     : ConnectResult.ConnectFailed));
            }
        }

        public void StartAutoConnect()
        {

        }

        public IEventPublisher ServerEvents { get; }

        public IHubProxyWrapper EsbProxy { get; protected set; }

        public IHubConnectionWrapper HubConnection { get; }

        void OnHubConnectionError(Exception exception)
        {
            Dev2Logger.Error(this, exception, "Warewolf Error");
        }

        void OnMemoReceived(string objString)
        {
            var obj = _serializer.Deserialize<DesignValidationMemo>(objString);
            ServerEvents.PublishObject(obj);
        }

        void OnPermissionsMemoReceived(string objString)
        {
            var obj = _serializer.Deserialize<PermissionsModifiedMemo>(objString);
            try
            {
                RaisePermissionsModified(obj.ModifiedPermissions);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(this, e, "Warewolf Error");
            }
            RaisePermissionsChanged();
        }

        public Action<IExplorerItem> ItemAddedMessageAction { get; set; }

        void OnItemAddedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            serverExplorerItem.ServerId = ID;
            ItemAddedMessageAction?.Invoke(serverExplorerItem);
        }

        public Action<IExplorerItem> ItemItemDeletedMessageAction { get; set; }

        void OnItemDeletedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            serverExplorerItem.ServerId = ID;
            ItemItemDeletedMessageAction?.Invoke(serverExplorerItem);
        }

        public Action<IExplorerItem> ItemItemUpdatedMessageAction { get; set; }

        void OnItemUpdatedMessageReceived(string obj)
        {
            var serverExplorerItem = _serializer.Deserialize<ServerExplorerItem>(obj);
            ItemItemUpdatedMessageAction?.Invoke(serverExplorerItem);
        }

        public Guid ServerID { get; set; }
        public Guid WorkspaceID { get; private set; }
        public Uri AppServerUri { get; }
        public Uri WebServerUri { get; }
        public AuthenticationType AuthenticationType { get; }
        public string UserName { get; }
        public string Password { get; }

        public bool IsAuthorized { get; set; }
        public IAsyncWorker AsyncWorker { get; }

        public event EventHandler<NetworkStateEventArgs> NetworkStateChanged;

        public event EventHandler PermissionsChanged;

        void RaisePermissionsChanged()
        {
            PermissionsChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<List<WindowsGroupPermission>> PermissionsModified;

        void RaisePermissionsModified(List<WindowsGroupPermission> args)
        {
            PermissionsModified?.Invoke(this, args);
        }

        void UpdateIsAuthorized(bool isAuthorized)
        {
            if (IsAuthorized != isAuthorized)
            {
                IsAuthorized = isAuthorized;
                RaisePermissionsChanged();
            }
        }

        protected void OnNetworkStateChanged(NetworkStateEventArgs e)
        {
            var handler = NetworkStateChanged;
            handler?.Invoke(this, e);
        }

        public StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceId)
        {
            return ExecuteCommand(xmlRequest, workspaceId, 120000);
        }
        public StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceId, int timeout)
        {
            if (xmlRequest == null || xmlRequest.Length == 0)
            {
                throw new ArgumentNullException(nameof(xmlRequest));
            }
            
            var executeRequestAsync = Task.Run(async () => await ExecuteCommandAsync(xmlRequest, workspaceId).ConfigureAwait(true));
            if (executeRequestAsync.Wait(timeout))
            {
                return executeRequestAsync.Result;
            }
            return null;
        }

        public async Task<StringBuilder> ExecuteCommandAsync(StringBuilder xmlRequest, Guid workspaceId)
        {
            if (xmlRequest == null || xmlRequest.Length == 0)
            {
                throw new ArgumentNullException(nameof(xmlRequest));
            }

            Dev2Logger.Debug("Execute Command Payload [ " + xmlRequest + " ]", "Warewolf Debug");

            var messageId = Guid.NewGuid();
            var envelope = new Envelope
            {
                PartID = 0,
                Type = typeof(Envelope),
                Content = xmlRequest.ToString()
            };

            var result = new StringBuilder();
            try
            {
                await EsbProxy.Invoke<Receipt>("ExecuteCommand", envelope, true, workspaceId, Guid.Empty, messageId).ConfigureAwait(true);
                var fragmentInvoke = await EsbProxy.Invoke<string>("FetchExecutePayloadFragment", new FutureReceipt { PartID = 0, RequestID = messageId }).ConfigureAwait(false);
                result.Append(fragmentInvoke);

                if (result.Length > 0)
                {
                    var start = result.LastIndexOf("<" + GlobalConstants.ManagementServicePayload + ">", false);
                    var end = result.LastIndexOf("</" + GlobalConstants.ManagementServicePayload + ">", false);
                    if (start > 0 && start < end && end - start > 1)
                    {
                        start += GlobalConstants.ManagementServicePayload.Length + 2;
                        return new StringBuilder(result.Substring(start, end - start));
                    }

                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, "Warewolf Error");
            }
            return result;
        }

        public async Task<StringBuilder> ExecuteCommandAsync(ICatalogRequest request, Guid workspaceId)
        {
            var toSend = _serializer.SerializeToBuilder(request);
            return await ExecuteCommandAsync(toSend, workspaceId).ConfigureAwait(true);
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

        public Guid ID { get; private set; }

        bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }

    class ServerProxyPersistentConnection : ServerProxyWithoutChunking
    {
        public ServerProxyPersistentConnection(Uri serverUri)
            : base(serverUri)
        {
            HubConnection.Start();
            StartHubConnectionWatchdogThread();
        }

        public ServerProxyPersistentConnection(string webAddress, string userName, string password)
            : base(webAddress, userName, password)
        {
            HubConnection.Start();
            StartHubConnectionWatchdogThread();
        }

        public ServerProxyPersistentConnection(string serverUri, ICredentials credentials, IAsyncWorker worker) 
            : base(serverUri, credentials, worker)
        {
            HubConnection.Start();
            StartHubConnectionWatchdogThread();
        }

        private void StartHubConnectionWatchdogThread()
        {
            var t = new Thread(() =>
            {
                const int initialDelay = 1;
                const int multiplier = 2;
                const int maxDelay = 60;
                var delay = initialDelay;
                bool stopped = false;

                HubConnection.StateChanged += (stateChange) =>
                {
                    if (stateChange.NewState == ConnectionStateWrapped.Disconnected)
                    {
                        stopped = true;
                    }
                };
                while (true)
                {
                    Thread.Sleep(delay);
                    if (stopped)
                    {
                        stopped = false;
                        delay *= multiplier;
                        if (delay > maxDelay)
                        {
                            delay = initialDelay;
                        }
                        HubConnection.Start();
                    }
                }
            });
            t.IsBackground = true;
            t.Start();
        }
    }

    public class FallbackException : Exception
    {
    }
}
