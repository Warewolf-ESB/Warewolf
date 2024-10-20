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
using Microsoft.AspNet.SignalR.Hubs;
using Elastic.Clients.Elasticsearch;
using Warewolf.Resource.Errors;



// Interface between the Studio and Server. Commands sent from the Studio come here to get processed, this is why methods are unused or only used in tests.

namespace Dev2.Runtime.WebServer.Hubs
{
    /**
     * SignalR hub primarily used by Warewolf Studio, if one wanted to use SignalR from a web browser to interact with the Warewolf Server
     * one would use this hub by connecting using the JS SignalR client library with url "/esb".
     */
    [AuthorizeHub]
    [HubName("esb")]
    public class EsbHub : ServerHub, IDebugWriter, IExplorerRepositorySync
    {
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();
        static readonly Dictionary<Guid, string>  ResourceAffectedMessagesCache = new Dictionary<Guid, string>();
        public EsbHub()
        {
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

        public EsbHub(Server server)
            : base(server)
        {
            DebugDispatcher.Instance.Add(GlobalConstants.ServerWorkspaceID, this);
        }

        #region Implementation of IDebugWriter
        
        public void Write(IDebugState debugState)
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
                hubCallerConnectionContext.All.ItemAddedMessage(item);
            }
        }

        #endregion

        void ResourceSaved(IResource resource)
        {
            if (ServerExplorerRepository.Instance != null)
            {
                var resourceItem = ServerExplorerRepository.Instance.UpdateItem(resource);
                AddItemMessage(resourceItem);
            }
            
        }

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

            var hubCallerConnectionContext = Clients;
            try
            {
                var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
                user.SendDebugState(debugSerializated);
            }
            catch (Exception ex)
            {
                var user = hubCallerConnectionContext.Caller;
                user.SendDebugState(debugSerializated);
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
            if (Context.User.Identity.Name != null)
            {
                receipt.User = Context.User.Identity.Name;
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
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = Context.User };
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
                        
                        var userPrinciple = Context.User;
                        if (Context.User.Identity != null)
                        
                        {
                            user = Context.User.Identity.Name;
                            userPrinciple = Context.User;
                            Thread.CurrentPrincipal = userPrinciple;
                            Dev2Logger.Debug("Execute Command Invoked For [ " + user + " : "+userPrinciple?.Identity?.AuthenticationType+" : "+userPrinciple?.Identity?.IsAuthenticated+" ] For Service [ " + request.ServiceName + " ]", GlobalConstants.WarewolfDebug);
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
        
        public override Task OnConnected()
        {
            
            ConnectionActions();
            return base.OnConnected();
        }

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

        void ConnectionActions()
        {
            
            SetupEvents();

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
