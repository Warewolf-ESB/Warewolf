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
using System.Diagnostics;
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
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
//using Microsoft.AspNet.SignalR;
//using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Nest;
using Warewolf.Resource.Errors;

// Interface between the Studio and Server. Commands sent from the Studio come here to get processed, this is why methods are unused or only used in tests.

namespace Dev2.Runtime.WebServer.Hubs
{
    /**
     * SignalR hub primarily used by Warewolf Studio, if one wanted to use SignalR from a web browser to interact with the Warewolf Server
     * one would use this hub by connecting using the JS SignalR client library with url "/esb".
     */
    //[AuthorizeHub]
    //[HubName("esb")]
    public class EsbHub : ServerHub, IDebugWriter, IExplorerRepositorySync
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();
        static readonly Dictionary<Guid, string> ResourceAffectedMessagesCache = new Dictionary<Guid, string>();
        private IHubContext<EsbHub> _hubContext;

        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public EsbHub(IHubContext<EsbHub> hubContext = null, IHttpContextAccessor httpContextAccessor = null)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

        public EsbHub(Server server, IHubContext<EsbHub> hubContext, IHttpContextAccessor httpContextAccessor = null)
            : base(server)
        {
            _hubContext = hubContext;
            _httpContextAccessor = httpContextAccessor;
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

        #region Implementation of IDebugWriter

        public void WriteDebugState(IDebugState debugState)
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
                var hubCallerConnectionContext = Clients;
                hubCallerConnectionContext.All.SendAsync("ItemAddedMessage", item);
            }
        }

        public IEsbMessage IsMessagePublished(IExplorerItem addedItem, IEsbMessage esbMessage)
        {
            if (addedItem != null)
            {
                esbMessage = new EsbMessage();
                esbMessage.MessagePublished = true;
            }
            return esbMessage;
        }

        #endregion

        void ResourceSaved(IResource resource)
        {
            if (ServerExplorerRepository.Instance != null)
            {
                var resourceItem = ServerExplorerRepository.Instance.UpdateItem(resource);
                var esbMessage = new EsbMessage();
                AddItemMessage(resourceItem);
            }

        }



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
                //Clients.Caller.SendPermissionsMemo(serializedMemo);

                if (clientCaller != null)
                    clientCaller.SendAsync("SendPermissionsMemo", serializedMemo);
                else
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

            //var hubCallerConnectionContext = Clients;
            var hubCallerConnectionContext = _hubContext.Clients;


            try
            {
                //var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
                //user.SendAsync("SendDebugState", debugSerializated);//user.SendDebugState(debugSerializated);
                hubCallerConnectionContext.Group($"user_{_httpContextAccessor.HttpContext.User.Identity.Name}").SendAsync("SendDebugState", debugSerializated).Wait();

            }
            catch (Exception ex)
            {
                Dev2Logger.Error(this, ex, GlobalConstants.WarewolfError);
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
            if (_httpContextAccessor.HttpContext.User.Identity.Name != null)
            {
                receipt.User = _httpContextAccessor.HttpContext.User.Identity.Name;
            }

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
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = _httpContextAccessor.HttpContext.User };
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

                        var userPrinciple = _httpContextAccessor.HttpContext.User;
                        if (_httpContextAccessor.HttpContext.User.Identity != null)

                        {
                            user = _httpContextAccessor.HttpContext.User.Identity.Name;
                            userPrinciple = _httpContextAccessor.HttpContext.User;
                            Thread.CurrentPrincipal = userPrinciple;
                            Dev2Logger.Debug("Execute Command Invoked For [ " + user + " : " + userPrinciple?.Identity?.AuthenticationType + " : " + userPrinciple?.Identity?.IsAuthenticated + " ] For Service [ " + request.ServiceName + " ]", GlobalConstants.WarewolfDebug);
                        }
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

        //public override Task OnConnected()
        //{

        //    ConnectionActions();
        //    return base.OnConnected();
        //}

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{_httpContextAccessor.HttpContext.User.Identity.Name}");
            ConnectionActions();
            await base.OnConnectedAsync();
        }

        ///// <summary>
        /////     Called when the connection reconnects to this hub instance.
        ///// </summary>
        ///// <returns>
        /////     A <see cref="T:System.Threading.Tasks.Task" />
        ///// </returns>
        //public override Task OnReconnected()
        //{
        //    ConnectionActions();
        //    return base.OnReconnected();
        //}

        void ConnectionActions()
        {

            SetupEvents();
            var connectionId = Context.ConnectionId;
            var t = new Task(() =>
            {
                var workspaceId = Server.GetWorkspaceID(_httpContextAccessor.HttpContext.User.Identity);
                ResourceCatalog.Instance.LoadServerActivityCache();

                var clientCaller = _hubContext.Clients.Client(connectionId);

                clientCaller.SendAsync("SendWorkspaceID", workspaceId);//clientCaller.SendWorkspaceID(workspaceId);
                clientCaller.SendAsync("SendServerID", HostSecurityProvider.Instance.ServerID);//clientCaller.SendServerID(HostSecurityProvider.Instance.ServerID);

                NotifyPermissionsHaveBeenModified(clientCaller, _httpContextAccessor.HttpContext.User);//PermissionsHaveBeenModified(null, null);
            });
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
