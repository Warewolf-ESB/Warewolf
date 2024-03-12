#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Nest;
using Warewolf.Resource.Errors;
#if NETFRAMEWORK
using Microsoft.AspNet.SignalR.Hubs;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Dev2.Common.Interfaces.Infrastructure.Communication;
#endif

// Interface between the Studio and Server. Commands sent from the Studio come here to get processed, this is why methods are unused or only used in tests.

namespace Dev2.Runtime.WebServer.Hubs
{
    /**
     * SignalR hub primarily used by Warewolf Studio, if one wanted to use SignalR from a web browser to interact with the Warewolf Server
     * one would use this hub by connecting using the JS SignalR client library with url "/esb".
     */
#if NETFRAMEWORK
    [AuthorizeHub]
    [HubName("esb")]
#endif
    public class EsbHub : ServerHub, IDebugWriter, IExplorerRepositorySync
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();
        static readonly Dictionary<Guid, string> ResourceAffectedMessagesCache = new Dictionary<Guid, string>();
#if NETFRAMEWORK
        public EsbHub()
#else
        private IHubContext<EsbHub> _hubContext;

        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public EsbHub(IHubContext<EsbHub> hubContext = null, IHttpContextAccessor httpContextAccessor = null)
#endif
        {
#if !NETFRAMEWORK
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
#endif
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

#if NETFRAMEWORK
        public EsbHub(Server server)
#else
        public EsbHub(Server server, IHubContext<EsbHub> hubContext, IHttpContextAccessor httpContextAccessor = null)
#endif
            : base(server)
        {
#if !NETFRAMEWORK
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
#endif
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

        #region Implementation of IDebugWriter

#if NETFRAMEWORK
        public void Write(IDebugState debugState)
#else
        public void WriteDebugState(IDebugState debugState)
#endif
        {
            SendDebugState(debugState as DebugState);
        }

        public void Write(string serializeObject)
        {
            var debugState = _serializer.Deserialize<DebugState>(serializeObject);
            SendDebugState(debugState);
        }

        #endregion

        #region Implementation of IExplorerRepositorySync

        public void AddItemMessage(IExplorerItem addedItem)
        {
            if (addedItem != null)
            {
                addedItem.ServerId = HostSecurityProvider.Instance.ServerID;
                var item = _serializer.Serialize(addedItem);
#if NETFRAMEWORK
                var hubCallerConnectionContext = Clients;
                hubCallerConnectionContext.All.ItemAddedMessage(item);
#else
                _hubContext.Clients.All.SendAsync("ItemAddedMessage", item);
#endif
            }
        }

#if !NETFRAMEWORK
        public IEsbMessage IsMessagePublished(IExplorerItem addedItem, IEsbMessage esbMessage)
        {
            if (addedItem != null)
            {
                esbMessage = new EsbMessage();
                esbMessage.MessagePublished = true;
            }
            return esbMessage;
        }
#endif

        #endregion

        void ResourceSaved(IResource resource)
        {
            if (ServerExplorerRepository.Instance != null)
            {
                var resourceItem = ServerExplorerRepository.Instance.UpdateItem(resource);
#if !NETFRAMEWORK
                var esbMessage = new EsbMessage();
#endif
                AddItemMessage(resourceItem);
            }

        }



#if NETFRAMEWORK

        void PermissionsHaveBeenModified(object sender, PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            if (Context == null)
            {
                return;
            }

            try
            {
                var user = Context.User;
                var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
                var serializedMemo = _serializer.Serialize(permissionsMemo);
                Clients.Caller.SendPermissionsMemo(serializedMemo);
            }
            catch (Exception e)
            {
                Dev2Logger.Warn($"unable to notify remote client with PermissionsMemo, error: {e.Message}", GlobalConstants.WarewolfWarn);
            }
        }
#else
        void NotifyPermissionsHaveBeenModified(IClientProxy clientCaller, System.Security.Claims.ClaimsPrincipal user)
        {
            if (null == user && _httpContextAccessor.HttpContext != null)
            {
                user = _httpContextAccessor.HttpContext.User;
                if (null == user) return;
            }

            try
            {
                var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
                var serializedMemo = _serializer.Serialize(permissionsMemo);

                if (clientCaller != null)
                    clientCaller.SendAsync("SendPermissionsMemo", serializedMemo);
                else if(null != user)
                    _hubContext.Clients.Group($"user_{user.Identity.Name}").SendAsync("SendPermissionsMemo", serializedMemo);

            }
            catch (Exception e)
            {
                Dev2Logger.Warn($"unable to notify remote client with PermissionsMemo, error: {e.Message}", GlobalConstants.WarewolfWarn);
            }
        }


        void PermissionsHaveBeenModified(object sender, PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            if (_httpContextAccessor.HttpContext == null)
                return;

            try
            {
                var user = _httpContextAccessor.HttpContext.User;
                var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
                var serializedMemo = _serializer.Serialize(permissionsMemo);
                //Clients.Caller.SendPermissionsMemo(serializedMemo);
                _hubContext.Clients.Group($"user_{_httpContextAccessor.HttpContext.User.Identity.Name}").SendAsync("SendPermissionsMemo", serializedMemo);

            }
            catch (Exception e)
            {
                Dev2Logger.Warn($"unable to notify remote client with PermissionsMemo, error: {e.Message}", GlobalConstants.WarewolfWarn);
            }
        }
#endif

        void SendResourceMessages(Guid resourceId, IList<ICompileMessageTO> compileMessageTos)
        {
            SendResourcesAffectedMemo(resourceId, compileMessageTos);
        }

        protected void OnCompilerMessageReceived(IList<ICompileMessageTO> messages)
        {
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.MappingChange || m.MessageType == CompileMessageType.MappingIsRequiredChanged), CoalesceMappingChangedErrors);
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.ResourceSaved), CoalesceResourceSavedErrors);

        }

        #region CoalesceMappingChangedErrors

        static void CoalesceMappingChangedErrors(DesignValidationMemo memo, ICompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = false;
            memo.Errors.Add(compilerMessage.ToErrorInfo());
        }

        #endregion

        #region CoalesceResourceSavedErrors

        static void CoalesceResourceSavedErrors(DesignValidationMemo memo, ICompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = true;
        }

        #endregion

        void SendResourcesAffectedMemo(Guid resourceId, IList<ICompileMessageTO> messages)
        {

            var msgs = new CompileMessageList { Dependants = new List<string>() };
            messages.ToList().ForEach(s => msgs.Dependants.Add(s.ServiceName));
            msgs.MessageList = messages;
            msgs.ServiceID = resourceId;
            var serializedMemo = _serializer.Serialize(msgs);
            if (!ResourceAffectedMessagesCache.ContainsKey(resourceId))
            {
                ResourceAffectedMessagesCache.Add(resourceId, serializedMemo);
            }
        }

        public async Task<string> FetchResourcesAffectedMemo(Guid resourceId)
        {
            try
            {
                if (ResourceAffectedMessagesCache.TryGetValue(resourceId, out string value))
                {
                    var task = new Task<string>(() => value);
                    ResourceAffectedMessagesCache.Remove(resourceId);
                    task.Start();
                    return await task.ConfigureAwait(true);
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(this, e, GlobalConstants.WarewolfError);
            }

            return null;
        }

        public void SendDebugState(DebugState debugState)
        {
            var debugSerializated = _serializer.Serialize(debugState);

#if NETFRAMEWORK
            var hubCallerConnectionContext = Clients;
#else

            var hubCallerConnectionContext = _hubContext.Clients;
#endif


            try
            {
#if NETFRAMEWORK
                var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
                user.SendDebugState(debugSerializated);
#else
                hubCallerConnectionContext.Group($"user_{_httpContextAccessor.HttpContext.User.Identity.Name}").SendAsync("SendDebugState", debugSerializated).Wait();
#endif

            }
            catch (Exception ex)
            {
#if NETFRAMEWORK
                var user = hubCallerConnectionContext.Caller;
                user.SendDebugState(debugSerializated);
#else
                Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
#endif
            }

        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<ICompileMessageTO> messages, Action<TMemo, ICompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            var messageArray = messages.ToArray();
            if (messageArray.Length == 0)
            {
                return;
            }

            // we need to broadcast per service and per unique ID messages
            var serviceGroupings = messageArray.GroupBy(to => to.ServiceID).ToList();
            WriteEventProviderClientMessage(serviceGroupings, coalesceErrors);

            var instanceGroupings = messageArray.Where(to => to.UniqueID != Guid.Empty).GroupBy(to => to.UniqueID).ToList();
            WriteEventProviderClientMessage(instanceGroupings, coalesceErrors);
        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<IGrouping<Guid, ICompileMessageTO>> groupings, Action<TMemo, ICompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            var memo = new TMemo { ServerID = HostSecurityProvider.Instance.ServerID };

            foreach (var grouping in groupings)
            {
                if (grouping.Key == Guid.Empty)
                {
                    continue;
                }
                memo.InstanceID = grouping.Key;
                foreach (var message in grouping)
                {
                    memo.WorkspaceID = message.WorkspaceID;
                    coalesceErrors?.Invoke(memo, message);
                }
            }
        }

        public async Task AddDebugWriter(Guid workspaceId)
        {
            var task = new Task(() => DebugDispatcher.Instance.Add(workspaceId, this));
            task.Start();
            await task.ConfigureAwait(true);
        }

        public async Task<string> FetchExecutePayloadFragment(FutureReceipt receipt)
        {
#if NETFRAMEWORK
            if (Context.User.Identity.Name != null)
            {
                receipt.User = Context.User.Identity.Name;
            }
#else
            if (_httpContextAccessor.HttpContext.User.Identity.Name != null)
            {
                receipt.User = _httpContextAccessor.HttpContext.User.Identity.Name;
            }
#endif

            try
            {
                var value = ResultsCache.Instance.FetchResult(receipt);
                var task = new Task<string>(() => value);

                task.Start();
                return await task.ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(this, e, GlobalConstants.WarewolfError);
            }

            return null;
        }

        public async Task<Receipt> ExecuteCommand(Envelope envelope, bool endOfStream, Guid workspaceId, Guid dataListId, Guid messageId)
        {
#if NETFRAMEWORK
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = Context.User };
#else
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = _httpContextAccessor.HttpContext.User };
#endif
            try
            {
                var task = new Task<Receipt>(() =>
                {
                    try
                    {
                        if (!MessageCache.TryGetValue(messageId, out StringBuilder sb))
                        {
                            sb = new StringBuilder();
                            MessageCache.TryAdd(messageId, sb);
                        }
                        sb.Append(envelope.Content);

                        MessageCache.TryRemove(messageId, out sb);
                        var request = _serializer.Deserialize<EsbExecuteRequest>(sb);

                        var user = string.Empty;

#if NETFRAMEWORK
                        var userPrinciple = Context.User;
                        if (Context.User.Identity != null)
                        
                        {
                            user = Context.User.Identity.Name;
                            userPrinciple = Context.User;
                            Thread.CurrentPrincipal = userPrinciple;
                            Dev2Logger.Debug("Execute Command Invoked For [ " + user + " : "+userPrinciple?.Identity?.AuthenticationType+" : "+userPrinciple?.Identity?.IsAuthenticated+" ] For Service [ " + request.ServiceName + " ]", GlobalConstants.WarewolfDebug);
                        }
#else
                        var userPrinciple = _httpContextAccessor.HttpContext.User;
                        if (_httpContextAccessor.HttpContext.User.Identity != null)

                        {
                            user = _httpContextAccessor.HttpContext.User.Identity.Name;
                            userPrinciple = _httpContextAccessor.HttpContext.User;
                            Thread.CurrentPrincipal = userPrinciple;
                            Dev2Logger.Debug("Execute Command Invoked For [ " + user + " : " + userPrinciple?.Identity?.AuthenticationType + " : " + userPrinciple?.Identity?.IsAuthenticated + " ] For Service [ " + request.ServiceName + " ]", GlobalConstants.WarewolfDebug);
                        }
#endif
                        StringBuilder processRequest = null;
                        Common.Utilities.PerformActionInsideImpersonatedContext(userPrinciple, () => { processRequest = internalServiceRequestHandler.ProcessRequest(request, workspaceId, dataListId, Context.ConnectionId); });
                        var future = new FutureReceipt
                        {
                            PartID = 0,
                            RequestID = messageId,
                            User = user
                        };

                        var value = processRequest?.ToString();
                        if (!string.IsNullOrEmpty(value) && !ResultsCache.Instance.AddResult(future, value))
                        {
                            Dev2Logger.Error(new Exception(string.Format(ErrorResource.FailedToBuildFutureReceipt, Context.ConnectionId, value)), GlobalConstants.WarewolfError);
                        }

                        return new Receipt { PartID = envelope.PartID, ResultParts = 1 };

                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                    }
                    return null;
                });
                task.Start();
                return await task.ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                Dev2Logger.Info("Is End of Stream:" + endOfStream, GlobalConstants.WarewolfInfo);
            }
            return null;
        }

        #region Overrides of Hub

#if NETFRAMEWORK
        public override Task OnConnected()
        {
            
            ConnectionActions();
            return base.OnConnected();
        }
#else
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{_httpContextAccessor.HttpContext.User.Identity.Name}");
            ConnectionActions();
            await base.OnConnectedAsync();
        }
#endif

#if NETFRAMEWORK
        /// <summary>
        ///     Called when the connection reconnects to this hub instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
        public override Task OnReconnected()
        {
            ConnectionActions();
            return base.OnReconnected();
        }
#endif

        void ConnectionActions()
        {

            SetupEvents();
#if NETFRAMEWORK
            var t = new Task(() =>
            {
                var workspaceId = Server.GetWorkspaceID(Context.User.Identity);
                //ResourceCatalog.Instance.LoadServerActivityCache();
                var hubCallerConnectionContext = Clients;
                var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
                user.SendWorkspaceID(workspaceId);
                user.SendServerID(HostSecurityProvider.Instance.ServerID);
                PermissionsHaveBeenModified(null, null);
            });
#else
            var connectionId = Context.ConnectionId;
            var t = new Task(() =>
            {
                var workspaceId = Server.GetWorkspaceID(_httpContextAccessor.HttpContext.User.Identity);
                ResourceCatalog.Instance.LoadServerActivityCache();

                var clientCaller = _hubContext.Clients.Client(connectionId);

                clientCaller.SendAsync("SendWorkspaceID", workspaceId);//clientCaller.SendWorkspaceID(workspaceId);
                clientCaller.SendAsync("SendServerID", HostSecurityProvider.Instance.ServerID);//clientCaller.SendServerID(HostSecurityProvider.Instance.ServerID);

                NotifyPermissionsHaveBeenModified(clientCaller, _httpContextAccessor.HttpContext == null ? null : _httpContextAccessor.HttpContext.User);//PermissionsHaveBeenModified(null, null);
            });
#endif
            t.Start();
        }

        protected void SetupEvents()
        {
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
            ServerAuthorizationService.Instance.PermissionsModified += PermissionsHaveBeenModified;
            ServerExplorerRepository.Instance.MessageSubscription(this);
            if (ResourceCatalog.Instance.ResourceSaved == null)
            {
                ResourceCatalog.Instance.ResourceSaved += ResourceSaved;
            }
            if (ResourceCatalog.Instance.SendResourceMessages == null)
            {
                ResourceCatalog.Instance.SendResourceMessages += SendResourceMessages;
            }
        }

        #endregion
    }
}
