using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Dev2.Diagnostics.Debug;
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
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        readonly Dev2JsonSerializer _serializer = new Dev2JsonSerializer();
        public EsbHub()
        {

        }

        void ResourceSaved(IResource resource)
        {
            var factory = new ExplorerItemFactory(ResourceCatalog.Instance, new DirectoryWrapper(), ServerAuthorizationService.Instance);
            var resourceItem = factory.CreateResourceItem(resource);
            AddItemMessage(resourceItem);
        }

        void PermissionsHaveBeenModified(object sender, PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = permissionsModifiedEventArgs.ModifiedWindowsGroupPermissions,
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
            var serializedMemo = _serializer.Serialize(permissionsMemo);
            var hubCallerConnectionContext = Clients;
            hubCallerConnectionContext.All.SendPermissionsMemo(serializedMemo);
        }

        public EsbHub(Server server)
            : base(server)
        {
        }

        public async Task AddDebugWriter(Guid workspaceId)
        {
            var task = new Task(() => DebugDispatcher.Instance.Add(workspaceId, this));
            task.Start();
            await task;
        }

        #region Overrides of Hub

        /// <summary>
        /// Called when the connection connects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
        /// </returns>
        public override Task OnConnected()
        {
            ConnectionActions();
            return base.OnConnected();
        }


        /// <summary>
        /// Called when the connection reconnects to this hub instance.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/>
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
            if(authorizationServiceBase != null)
            {
                authorizationServiceBase.Dispose();
            }
            if(ResourceCatalog.Instance.ResourceSaved == null)
            {
                ResourceCatalog.Instance.ResourceSaved = null;
            }
            ResourceCatalog.Instance.Dispose();
            return base.OnDisconnected();
        }


        void ConnectionActions()
        {
            SetupEvents();

            var workspaceId = Server.GetWorkspaceID(Context.User.Identity);
            var hubCallerConnectionContext = Clients;
            var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
            user.SendWorkspaceID(workspaceId);
            user.SendServerID(HostSecurityProvider.Instance.ServerID);
        }

        protected void SetupEvents()
        {
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
            ServerAuthorizationService.Instance.PermissionsModified += PermissionsHaveBeenModified;
            ServerExplorerRepository.Instance.MessageSubscription(this);
            if(ResourceCatalog.Instance.ResourceSaved == null)
            {
                ResourceCatalog.Instance.ResourceSaved += ResourceSaved;
            }
        }

        #endregion


        /// <summary>
        /// Fetches the execute payload fragment.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <returns></returns>
        public async Task<string> FetchExecutePayloadFragment(FutureReceipt receipt)
        {
            // Set Requesting User as per what is authorized ;)
            // Sneaky people may try to forge packets to get payload ;)
            if(Context.User.Identity.Name != null)
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
            catch(Exception e)
            {
                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Log.Error(this, e);
                // ReSharper restore InvokeAsExtensionMethod
            }

            return null;
        }

        /// <summary>
        /// Executes the command.
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
                        if(!MessageCache.TryGetValue(messageId, out sb))
                        {
                            sb = new StringBuilder();
                            MessageCache.TryAdd(messageId, sb);
                        }
                        sb.Append(envelope.Content);

                        if(endOfStream)
                        {
                            MessageCache.TryRemove(messageId, out sb);
                            EsbExecuteRequest request = _serializer.Deserialize<EsbExecuteRequest>(sb);

                            var user = string.Empty;
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if(Context.User.Identity != null)
                            // ReSharper restore ConditionIsAlwaysTrueOrFalse
                            {
                                user = Context.User.Identity.Name;
                                // set correct principle ;)
                                System.Threading.Thread.CurrentPrincipal = Context.User;
                                Dev2Logger.Log.Debug("Execute Command Invoked For [ " + user + " ] For Service [ " + request.ServiceName + " ]");
                            }

                            var processRequest = internalServiceRequestHandler.ProcessRequest(request, workspaceId, dataListId, Context.ConnectionId);
                            // Convert to chunked msg store for fetch ;)
                            var length = processRequest.Length;
                            var startIdx = 0;
                            var rounds = (int)Math.Ceiling(length / GlobalConstants.MAX_SIZE_FOR_STRING);

                            for(var q = 0; q < rounds; q++)
                            {
                                var len = (int)GlobalConstants.MAX_SIZE_FOR_STRING;
                                if(len > (length - startIdx))
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

                                var value = processRequest.Substring(startIdx, len);

                                if(!ResultsCache.Instance.AddResult(future, value))
                                {
                                    Dev2Logger.Log.Error(new Exception("Failed to build future receipt for [ " + Context.ConnectionId + " ] Value [ " + value + " ]"));
                                }

                                startIdx += len;
                            }

                            return new Receipt { PartID = envelope.PartID, ResultParts = rounds };
                        }

                        return new Receipt { PartID = envelope.PartID, ResultParts = -1 };
                    }
                    catch(Exception e)
                    {
                        Dev2Logger.Log.Error(e);
                    }
                    return null;
                });
                task.Start();
                return await task;
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error(e);
            }
            return null;
        }

        protected void OnCompilerMessageReceived(IList<ICompileMessageTO> messages)
        {
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => (m.MessageType == CompileMessageType.MappingChange || m.MessageType == CompileMessageType.MappingIsRequiredChanged)), CoalesceMappingChangedErrors);
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

        public void SendDebugState(DebugState debugState)
        {
            var debugSerializated = _serializer.Serialize(debugState);

            var hubCallerConnectionContext = Clients;
            var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
            //var user = hubCallerConnectionContext.All;
            user.SendDebugState(debugSerializated);
        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<ICompileMessageTO> messages, Action<TMemo, ICompileMessageTO> coalesceErrors)
           where TMemo : IMemo, new()
        {
            var messageArray = messages.ToArray();
            if(messageArray.Length == 0)
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

            foreach(var grouping in groupings)
            {
                if(grouping.Key == Guid.Empty)
                {
                    continue;
                }
                memo.InstanceID = grouping.Key;
                foreach(var message in grouping)
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

        #region Implementation of IDebugWriter

        /// <summary>
        /// Writes the given state.
        /// <remarks>
        /// This must implement the one-way (fire and forget) message exchange pattern.
        /// </remarks>
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
            if(addedItem != null)
            {
                addedItem.ServerId = HostSecurityProvider.Instance.ServerID;
                var item = _serializer.Serialize(addedItem);
                var hubCallerConnectionContext = Clients;
                hubCallerConnectionContext.All.ItemAddedMessage(item);
            }
        }

        #endregion
    }
}