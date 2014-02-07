using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.Enums;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Diagnostics;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Security;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Security;
using Dev2.Services.Security;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;

namespace Dev2.Runtime.WebServer.Hubs
{
    [AuthorizeHub]
    [HubName("esb")]
    public class EsbHub : ServerHub, IDebugWriter
    {
        static readonly ConcurrentDictionary<Guid, StringBuilder> MessageCache = new ConcurrentDictionary<Guid, StringBuilder>();
        static readonly ConcurrentDictionary<string, string> ResultCache = new ConcurrentDictionary<string, string>();

        public EsbHub()
        {
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
            ServerAuthorizationService.Instance.PermissionsModified += PermissionsHaveBeenModified;
        }

        void PermissionsHaveBeenModified(object sender, PermissionsModifiedEventArgs permissionsModifiedEventArgs)
        {
            var permissionsMemo = new PermissionsModifiedMemo { ModifiedPermissions = permissionsModifiedEventArgs.ModifiedWindowsGroupPermissions };
            var serializedMemo = JsonConvert.SerializeObject(permissionsMemo);
            var hubCallerConnectionContext = Clients;
            hubCallerConnectionContext.All.SendPermissionsMemo(serializedMemo);
        }

        public EsbHub(Server server)
            : base(server)
        {
        }

        public async Task AddDebugWriter(Guid workspaceID)
        {
            var task = new Task(() => DebugDispatcher.Instance.Add(workspaceID, this));
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
            var workspaceID = Server.GetWorkspaceID(Context.User.Identity);
            Server.SendWorkspaceID(workspaceID, Context.ConnectionId);
            Server.SendServerID(HostSecurityProvider.Instance.ServerID, Context.ConnectionId);
            return base.OnConnected();
        }

        #endregion

        /// <summary>
        /// Fetches the execute payload fragment.
        /// </summary>
        /// <param name="receipt">The receipt.</param>
        /// <returns></returns>
        public async Task<string> FetchExecutePayloadFragment(FutureReceipt receipt)
        {
            string value;
            if(ResultCache.TryRemove(receipt.ToKey(), out value))
            {
                var task = new Task<string>(() => value);

                task.Start();
                return await task;
            }

            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        /// <param name="endOfStream">if set to <c>true</c> [end of stream].</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="dataListID">The data list unique identifier.</param>
        /// <param name="messageID">The message unique identifier.</param>
        /// <returns></returns>
        public async Task<Receipt> ExecuteCommand(Envelope envelope, bool endOfStream, Guid workspaceID, Guid dataListID, Guid messageID)
        {
            var internalServiceRequestHandler = new InternalServiceRequestHandler { ExecutingUser = Context.User };
            try
            {
                var task = new Task<Receipt>(() =>
                {
                    try
                    {
                        StringBuilder sb;
                        if(!MessageCache.TryGetValue(messageID, out sb))
                        {
                            sb = new StringBuilder();
                            MessageCache.TryAdd(messageID, sb);
                        }
                        sb.Append(envelope.Content);

                        if(endOfStream)
                        {
                            MessageCache.TryRemove(messageID, out sb);
                            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                            EsbExecuteRequest request = serializer.Deserialize<EsbExecuteRequest>(sb);
                            var processRequest = internalServiceRequestHandler.ProcessRequest(request, workspaceID, dataListID, Context.ConnectionId);
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

                                var future = new FutureReceipt
                                            {
                                                PartID = q,
                                                RequestID = messageID
                                            };

                                var value = processRequest.Substring(startIdx, len);

                                if(!ResultCache.TryAdd(future.ToKey(), value))
                                {
                                    this.LogError(new Exception("Failed to build future receipt for [ " + Context.ConnectionId + " ] Value [ " + value + " ]"));
                                }

                                startIdx += len;
                            }

                            return new Receipt { PartID = envelope.PartID, ResultParts = rounds };
                        }

                        return new Receipt { PartID = envelope.PartID, ResultParts = -1 };
                    }
                    catch(Exception e)
                    {
                        this.LogError(e);
                    }
                    return null;
                });
                task.Start();

                return await task;
            }
            catch(Exception e)
            {
                this.LogError(e);
            }
            return null;
        }

        protected void OnCompilerMessageReceived(IList<CompileMessageTO> messages)
        {
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.MappingChange || m.MessageType == CompileMessageType.MappingIsRequiredChanged), CoalesceMappingChangedErrors);
            WriteEventProviderClientMessage<DesignValidationMemo>(messages.Where(m => m.MessageType == CompileMessageType.ResourceSaved), CoalesceResourceSavedErrors);

        }

        #region CoalesceMappingChangedErrors

        static void CoalesceMappingChangedErrors(DesignValidationMemo memo, CompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = false;
            memo.Errors.Add(compilerMessage.ToErrorInfo());
        }

        #endregion

        #region CoalesceResourceSavedErrors

        static void CoalesceResourceSavedErrors(DesignValidationMemo memo, CompileMessageTO compilerMessage)
        {
            memo.ServiceID = compilerMessage.ServiceID;
            memo.IsValid = true;
        }

        #endregion

        public void SendMemo(Memo memo)
        {
            var serializedMemo = JsonConvert.SerializeObject(memo);
            var hubCallerConnectionContext = Clients;
            hubCallerConnectionContext.All.SendMemo(serializedMemo);
            CompileMessageRepo.Instance.ClearObservable();
            CompileMessageRepo.Instance.AllMessages.Subscribe(OnCompilerMessageReceived);
        }

        public void SendDebugState(DebugState debugState)
        {
            var debugSerializated = JsonConvert.SerializeObject(debugState);
            var hubCallerConnectionContext = Clients;
            var user = hubCallerConnectionContext.User(Context.User.Identity.Name);
            user.SendDebugState(debugSerializated);
        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<CompileMessageTO> messages, Action<TMemo, CompileMessageTO> coalesceErrors)
           where TMemo : IMemo, new()
        {
            var messageArray = messages.ToArray();
            if(messageArray.Length == 0)
            {
                return;
            }

            // we need to broadcast per service and per unique ID messages
            var serviceGroupings = messageArray.GroupBy(to => to.ServiceID);
            WriteEventProviderClientMessage(serviceGroupings, coalesceErrors);

            var instanceGroupings = messageArray.GroupBy(to => to.UniqueID);
            WriteEventProviderClientMessage(instanceGroupings, coalesceErrors);
        }

        void WriteEventProviderClientMessage<TMemo>(IEnumerable<IGrouping<Guid, CompileMessageTO>> groupings, Action<TMemo, CompileMessageTO> coalesceErrors)
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
    }
}