#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
        Action<Guid, CompileMessageList> ReceivedResourceAffectedMessage { get; set; }
        IHubConnectionWrapper HubConnection { get; }

        void FetchResourcesAffectedMemo(Guid resourceId);
    }
}
