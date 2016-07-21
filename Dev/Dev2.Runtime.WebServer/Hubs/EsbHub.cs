/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using System.Threading.Tasks;
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

        public void SendResourcesAffectedMemo(Guid resourceId, IList<ICompileMessageTO> messages)
        {
            var msgs = new CompileMessageList { Dependants = new List<string>() };
            messages.ToList().ForEach(s => msgs.Dependants.Add(s.ServiceName));
            msgs.MessageList = messages;
            msgs.ServiceID = resourceId;
            var serializedMemo = _serializer.Serialize(msgs);
            var hubCallerConnectionContext = Clients;

            hubCallerConnectionContext.All.ReceiveResourcesAffectedMemo(serializedMemo);
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