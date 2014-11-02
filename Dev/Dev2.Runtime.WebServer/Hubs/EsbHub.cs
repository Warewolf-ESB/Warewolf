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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Communication;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Wrappers;
using Dev2.Communication;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics.Debug;
using Dev2.Explorer;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hubs;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("esb")]
    public class EsbHub : ServerHub, IDebugWriter, IExplorerRepositorySync
    {
        private static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache =
            new ConcurrentDictionary<Guid, StringBuilder>();

        private readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();

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

        #endregion

        #region Implementation of IExplorerRepositorySync

        public void AddItemMessage(IExplorerItem addedItem)
        {
            if (addedItem != null)
            {
                addedItem.ServerId = HostSecurityProvider.Instance.ServerID;
                string item = _serializer.Serialize(addedItem);
                IHubCallerConnectionContext hubCallerConnectionContext = Clients;
                hubCallerConnectionContext.All.ItemAddedMessage(item);
            }
        }

        #endregion

        private void ResourceSaved(IResource resource)
        {
            var factory = new ExplorerItemFactory(ResourceCatalog.Instance, new DirectoryWrapper(),
                ServerAuthorizationService.Instance);
            ServerExplorerItem resourceItem = factory.CreateResourceItem(resource);
            AddItemMessage(resourceItem);
        }

        private void PermissionsHaveBeenModified(object sender,
            PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            IPrincipal user = Context.User;
            var permissionsMemo = new PermissionsModifiedMemo
            {
                ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                ServerID = HostSecurityProvider.Instance.ServerID
            };
            string serializedMemo = _serializer.Serialize(permissionsMemo);
            Clients.Caller.SendPermissionsMemo(serializedMemo);
        }

        private void SendResourceMessages(Guid resourceId, IList<ICompileMessageTO> compileMessageTos)
        {
            SendResourcesAffectedMemo(resourceId, compileMessageTos);
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
                string value = ResultsCache.Instance.FetchResult(receipt);
                var task = new Task<string>(() => value);

                task.Start();
                return await task;
            }
            catch (Exception e)
            {
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Log.Error(this, e);
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
        public async Task<Receipt> ExecuteCommand(Envelope envelope, bool endOfStream, Guid workspaceId, Guid dataListId,
            Guid messageId)
        {
            var internalServiceRequestHandler = new InternalServiceRequestHandler {ExecutingUser = Context.User};
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

                        if (endOfStream)
                        {
                            MessageCache.TryRemove(messageId, out sb);
                            var request = _serializer.Deserialize<EsbExecuteRequest>(sb);

                            string user = string.Empty;
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (Context.User.Identity != null)
                                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                            {
                                user = Context.User.Identity.Name;
                                // set correct principle ;)
                                Thread.CurrentPrincipal = Context.User;
                                Dev2Logger.Log.Debug("Execute Command Invoked For [ " + user + " ] For Service [ " +
                                                     request.ServiceName + " ]");
                            }

                            StringBuilder processRequest = internalServiceRequestHandler.ProcessRequest(request,
                                workspaceId, dataListId, Context.ConnectionId);
                            // Convert to chunked msg store for fetch ;)
                            int length = processRequest.Length;
                            int startIdx = 0;
                            var rounds = (int) Math.Ceiling(length/GlobalConstants.MAX_SIZE_FOR_STRING);

                            for (int q = 0; q < rounds; q++)
                            {
                                var len = (int) GlobalConstants.MAX_SIZE_FOR_STRING;
                                if (len > (length - startIdx))
                                {
                                    len = (length - startIdx);
                                }

                                // always place requesting user in here ;)
                                var future = new FutureReceipt
                                {
                                    PartID = q,
                                    RequestID = messageId,
                                    User = user
                                };

                                string value = processRequest.Substring(startIdx, len);

                                if (!ResultsCache.Instance.AddResult(future, value))
                                {
                                    Dev2Logger.Log.Error(
                                        new Exception("Failed to build future receipt for [ " + Context.ConnectionId +
                                                      " ] Value [ " + value + " ]"));
                                }

                                startIdx += len;
                            }

                            return new Receipt {PartID = envelope.PartID, ResultParts = rounds};
                        }

                        return new Receipt {PartID = envelope.PartID, ResultParts = -1};
                    }
                    catch (Exception e)
                    {
                        Dev2Logger.Log.Error(e);
                    }
                    return null;
                });
                task.Start();
                return await task;
            }
            catch (Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
            return null;
        }

        protected void OnCompilerMessageReceived(IList<ICompileMessageTO> messages)
        {
            WriteEventProviderClientMessage<DesignValidationMemo>(
                messages.Where(
                    m =>
                        (m.MessageType == CompileMessageType.MappingChange ||
                         m.MessageType == CompileMessageType.MappingIsRequiredChanged)), CoalesceMappingChangedErrors);
            WriteEventProviderClientMessage<DesignValidationMemo>(
                messages.Where(m => m.MessageType == CompileMessageType.ResourceSaved), CoalesceResourceSavedErrors);
        }

        public void SendMemo(Memo memo)
        {
            string serializedMemo = _serializer.Serialize(memo);
            IHubCallerConnectionContext hubCallerConnectionContext = Clients;

            hubCallerConnectionContext.All.SendMemo(serializedMemo);

            CompileMessageRepo.Instance.ClearObservable();
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
        }

        public void SendResourcesAffectedMemo(Guid resourceId, IList<ICompileMessageTO> messages)
        {
            var msgs = new CompileMessageList {Dependants = new List<string>()};
            messages.ToList().ForEach(s => msgs.Dependants.Add(s.ServiceName));
            msgs.MessageList = messages;
            msgs.ServiceID = resourceId;
            string serializedMemo = _serializer.Serialize(msgs);
            IHubCallerConnectionContext hubCallerConnectionContext = Clients;

            hubCallerConnectionContext.All.ReceiveResourcesAffectedMemo(serializedMemo);
        }

        public void SendDebugState(DebugState debugState)
        {
            string debugSerializated = _serializer.Serialize(debugState);

            IHubCallerConnectionContext hubCallerConnectionContext = Clients;
            dynamic user = hubCallerConnectionContext.User(Context.User.Identity.Name);
            //var user = hubCallerConnectionContext.All;
            user.SendDebugState(debugSerializated);
        }

        private void WriteEventProviderClientMessage<TMemo>(IEnumerable<ICompileMessageTO> messages,
            Action<TMemo, ICompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            ICompileMessageTO[] messageArray = messages.ToArray();
            if (messageArray.Length == 0)
            {
                return;
            }

            // we need to broadcast per service and per unique ID messages
            List<IGrouping<Guid, ICompileMessageTO>> serviceGroupings =
                messageArray.GroupBy(to => to.ServiceID).ToList();
            WriteEventProviderClientMessage(serviceGroupings, coalesceErrors);

            List<IGrouping<Guid, ICompileMessageTO>> instanceGroupings =
                messageArray.Where(to => to.UniqueID != Guid.Empty).GroupBy(to => to.UniqueID).ToList();
            WriteEventProviderClientMessage(instanceGroupings, coalesceErrors);
        }

        private void WriteEventProviderClientMessage<TMemo>(IEnumerable<IGrouping<Guid, ICompileMessageTO>> groupings,
            Action<TMemo, ICompileMessageTO> coalesceErrors)
            where TMemo : IMemo, new()
        {
            var memo = new TMemo {ServerID = HostSecurityProvider.Instance.ServerID};

            foreach (var grouping in groupings)
            {
                if (grouping.Key == Guid.Empty)
                {
                    continue;
                }
                memo.InstanceID = grouping.Key;
                foreach (ICompileMessageTO message in grouping)
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

        public override Task OnDisconnected()
        {
            ServerAuthorizationService.Instance.PermissionsModified -= PermissionsHaveBeenModified;
            var authorizationServiceBase = ServerAuthorizationService.Instance as AuthorizationServiceBase;
            if (authorizationServiceBase != null)
            {
                authorizationServiceBase.Dispose();
            }

            if (ResourceCatalog.Instance.ResourceSaved == null)
            {
                ResourceCatalog.Instance.ResourceSaved = null;
            }
            if (ResourceCatalog.Instance.SendResourceMessages == null)
            {
                ResourceCatalog.Instance.SendResourceMessages = null;
            }
            ResourceCatalog.Instance.Dispose();
            return base.OnDisconnected();
        }

        private void ConnectionActions()
        {
            SetupEvents();

            Guid workspaceId = Server.GetWorkspaceID(Context.User.Identity);
            IHubCallerConnectionContext hubCallerConnectionContext = Clients;
            dynamic user = hubCallerConnectionContext.User(Context.User.Identity.Name);
            user.SendWorkspaceID(workspaceId);
            user.SendServerID(HostSecurityProvider.Instance.ServerID);
            PermissionsHaveBeenModified(null, null);
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

        #region CoalesceMappingChangedErrors

        private static void CoalesceMappingChangedErrors(DesignValidationMemo memo, ICompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = false;
            memo.Errors.Add(compilerMessage.ToErrorInfo());
        }

        #endregion

        #region CoalesceResourceSavedErrors

        private static void CoalesceResourceSavedErrors(DesignValidationMemo memo, ICompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = true;
        }

        #endregion
    }
}