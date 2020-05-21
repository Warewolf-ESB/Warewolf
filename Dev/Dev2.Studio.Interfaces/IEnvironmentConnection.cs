#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.SignalR.Wrappers;
using System.Network;
using Dev2.Common.Interfaces.Studio.Core;


namespace Dev2.Studio.Interfaces
{
    public interface IEnvironmentConnection
    {
        IEventPublisher ServerEvents { get; }

        Guid ServerID { get; set; }
        Guid WorkspaceID { get; }

        Uri AppServerUri { get; }
        Uri WebServerUri { get; }
        AuthenticationType AuthenticationType { get; }
        string UserName { get; }
        string Password { get; }
        event EventHandler<NetworkStateEventArgs> NetworkStateChanged;
        event EventHandler PermissionsChanged;
        event EventHandler AuthenticationChanged;
        bool IsAuthorized { get; set; }

        Task<StringBuilder> ExecuteCommandAsync(StringBuilder xmlRequest, Guid workspaceId);
        StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceId);

        IHubProxyWrapper EsbProxy { get; }

        bool IsConnected { get; }
        bool IsConnecting { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }

        void Connect(Guid id);
        Task<bool> ConnectAsync(Guid id);
        void Disconnect();
        Guid ID { get; }

        void Verify(Action<ConnectResult> callback);

        void Verify(Action<ConnectResult> callback, bool wait);
        
        void StartAutoConnect();

        bool IsLocalHost { get; }

        Action<IExplorerItem> ItemAddedMessageAction { get; set; }
        IAsyncWorker AsyncWorker { get; }
        IPrincipal Principal { get; }
        event EventHandler<List<WindowsGroupPermission>> PermissionsModified;
        event EventHandler<SecuritySettingsTO> AuthenticationModified;
        Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage { get; set; }
        IHubConnectionWrapper HubConnection { get; }

        void FetchResourcesAffectedMemo(Guid resourceId);
    }
}
