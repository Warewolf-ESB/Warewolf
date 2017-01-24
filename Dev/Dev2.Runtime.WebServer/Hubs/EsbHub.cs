/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Warewolf.Resource.Errors;
// ReSharper disable UnusedMember.Global


// Interface between the Studio and Server. Commands sent from the Studio come here to get processed, this is why methods are unused or only used in tests.

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("esb")]
    public class EsbHub : ServerHub, IDebugWriter, IExplorerRepositorySync
    {
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();
        static readonly Dictionary<Guid, string>  ResourceAffectedMessagesCache = new Dictionary<Guid, string>();
        public EsbHub()
        {
        }

        public EsbHub(Server server)
            : base(server)
        {
        }

        #region Implementation of IDebugWriter

        /// <summary>
        ///     Writes the given state.
        ///     <remarks>
        ///         This must implement the one-way (fire and forget) message exchange pattern.
        ///     </remarks>
        /// </summary>
        /// <param name="debugState">The state to be written.</param>
        public void Write(IDebugState debugState)
        {
            SendDebugState(debugState as DebugState);
        }
        
        /// <summary>
        ///     Writes the given state.
        ///     <remarks>
        ///         This must implement the one-way (fire and forget) message exchange pattern.
        ///     </remarks>
        /// </summary>
        /// <param name="serializedState">The state to be written.</param>
        public void Write(string serializedState)
        {
            var debugState = _serializer.Deserialize<DebugState>(serializedState);
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
            var user = Context.User;
            var permissionsMemo = new PermissionsModifiedMemo
            {
                ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                ServerID = HostSecurityProvider.Instance.ServerID
            };
            var serializedMemo = _serializer.Serialize(permissionsMemo);
            Clients.Caller.SendPermissionsMemo(serializedMemo);
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

        public void SendMemo(Memo memo)
        {
            var serializedMemo = _serializer.Serialize(memo);
            var hubCallerConnectionContext = Clients;

            hubCallerConnectionContext.All.SendMemo(serializedMemo);

            CompileMessageRepo.Instance.ClearObservable();
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
        }

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
                string value;
                if (ResourceAffectedMessagesCache.TryGetValue(resourceId, out value))
                {
                    var task = new Task<string>(() => value);
                    ResourceAffectedMessagesCache.Remove(resourceId);
                    task.Start();
                    return await task;
                }
            }
            catch (Exception e)
            {
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
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
            catch
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
                    coalesceErrors(memo, message);
                }

                WriteEventProviderClientMessage(memo);
            }
        }

        protected virtual void WriteEventProviderClientMessage(IMemo memo)
        {
            SendMemo(memo as Memo);
        }

        public async Task AddDebugWriter(Guid workspaceId)
        {
            var task = new Task(() => DebugDispatcher.Instance.Add(workspaceId, this));
            task.Start();
            await task;
        }

        /// <summary>
        ///     Fetches the execute payload fragment.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <returns></returns>
        public async Task<string> FetchExecutePayloadFragment(FutureReceipt receipt)
        {
            // Set Requesting User as per what is authorized ;)
            // Sneaky people may try to forge packets to get payload ;)
            if (Context.User.Identity.Name != null)
            {
                receipt.User = Context.User.Identity.Name;
            }

            try
            {
                var value = ResultsCache.Instance.FetchResult(receipt);
                var task = new Task<string>(() => value);

                task.Start();
                return await task;
            }
            catch (Exception e)
            {
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
            }

            return null;
        }

        /// <summary>
        ///     Executes the command.
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        /// <param name="endOfStream">if set to <c>true</c> [end of stream].</param>
        /// <param name="workspaceId">The workspace unique identifier.</param>
        /// <param name="dataListId">The data list unique identifier.</param>
        /// <param name="messageId">The message unique identifier.</param>
        /// <returns></returns>
        public async Task<Receipt> ExecuteCommand(Envelope envelope, bool endOfStream, Guid workspaceId, Guid dataListId, Guid messageId)
        {
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = Context.User };
            try
            {
                var task = new Task<Receipt>(() =>
                {
                    try
                    {
                        StringBuilder sb;
                        if (!MessageCache.TryGetValue(messageId, out sb))
                        {
                            sb = new StringBuilder();
                            MessageCache.TryAdd(messageId, sb);
                        }
                        sb.Append(envelope.Content);

                        MessageCache.TryRemove(messageId, out sb);
                        var request = _serializer.Deserialize<EsbExecuteRequest>(sb);

                        var user = string.Empty;
                        // ReSharper disable ConditionIsAlwaysTrueOrFalse
                        var userPrinciple = Context.User;
                        if (Context.User.Identity != null)
                        // ReSharper restore ConditionIsAlwaysTrueOrFalse
                        {
                            user = Context.User.Identity.Name;
                            userPrinciple = Context.User;
                            Thread.CurrentPrincipal = userPrinciple;
                            Dev2Logger.Debug("Execute Command Invoked For [ " + user + " : "+userPrinciple?.Identity?.AuthenticationType+" : "+userPrinciple?.Identity?.IsAuthenticated+" ] For Service [ " + request.ServiceName + " ]");
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
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (!ResultsCache.Instance.AddResult(future, value))
                            {
                                Dev2Logger.Error(new Exception(string.Format(ErrorResource.FailedToBuildFutureReceipt, Context.ConnectionId, value)));
                            }
                        }
                        return new Receipt { PartID = envelope.PartID, ResultParts = 1 };

                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Error(e);
                    }
                    return null;
                });
                task.Start();
                return await task;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
                Dev2Logger.Info("Is End of Stream:" + endOfStream);
            }
            return null;
        }

        #region Overrides of Hub

        /// <summary>
        ///     Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task" />
        /// </returns>
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

            Task t = new Task(() =>
            {
                var workspaceId = Server.GetWorkspaceID(Context.User.Identity);
                ResourceCatalog.Instance.LoadResourceActivityCache(workspaceId);
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