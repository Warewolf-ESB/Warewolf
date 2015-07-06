
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Network;
using System.Security.Principal;
using System.Text;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.Events;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.SignalR.Wrappers;
using Dev2.Threading;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Studio.Core.Interfaces
{
    public interface IEnvironmentConnection:IDisposable
    {
        // PBI 6690 - 2013.07.04 - TWR : added
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
        bool IsAuthorized { get; set; }

        StringBuilder ExecuteCommand(StringBuilder xmlRequest, Guid workspaceId, Guid dataListId);

        IHubProxyWrapper EsbProxy { get; }

        bool IsConnected { get; }
        string Alias { get; set; }
        string DisplayName { get; set; }

        void Connect(Guid id);
        void Disconnect();
        Guid ID { get; }
        // BUG 9634 - 2013.07.17 - TWR : added
        void Verify(Action<ConnectResult> callback, bool wait = true);

        // BUG 10106 - 2013.08.13 - TWR - added
        void StartAutoConnect();

        bool IsLocalHost { get; }

        Action<IExplorerItem> ItemAddedMessageAction { get; set; }
        IAsyncWorker AsyncWorker { get; }
        IPrincipal Principal { get; }
        event EventHandler<List<WindowsGroupPermission>> PermissionsModified;
        Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage { get; set; }
        IHubConnectionWrapper HubConnection { get; }
    }
}
