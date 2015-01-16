
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Network;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Security
{
    public class ClientSecurityService : SecurityServiceBase
    {
        readonly IEnvironmentConnection _environmentConnection;

        public ClientSecurityService(IEnvironmentConnection environmentConnection)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            _environmentConnection = environmentConnection;
            EnvironmentConnection.NetworkStateChanged += OnNetworkStateChanged;
            EnvironmentConnection.PermissionsModified+=EnvironmentConnectionOnPermissionsModified;
        }

        void EnvironmentConnectionOnPermissionsModified(object sender, List<WindowsGroupPermission> windowsGroupPermissions)
        {
            Permissions = windowsGroupPermissions;
        }

        public IEnvironmentConnection EnvironmentConnection
        {
            get
            {
                return _environmentConnection;
            }
        }        

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs args)
        {
            if(args.ToState == NetworkState.Online)
            {
                Read();
            }
        }

        public override void Read()
        {
            // ReSharper disable UnusedVariable
#pragma warning disable 168
            var task = ReadAsync();
#pragma warning restore 168
        }

        public virtual async Task ReadAsync()
        {
            await Task.Factory.StartNew(() => base.Read());
        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
//            var communicationController = new CommunicationController
//            {
//                ServiceName = "SecurityReadService"
//            };
//            SecuritySettingsTO securitySettingsTo = communicationController.ExecuteCommand<SecuritySettingsTO>(EnvironmentConnection, EnvironmentConnection.WorkspaceID);
//            if(securitySettingsTo != null)
//            {
//                return securitySettingsTo.WindowsGroupPermissions;
//            }
            return null;
        }

        protected override void WritePermissions(List<WindowsGroupPermission> permissions)
        {
        }

        protected override void LogStart([CallerMemberName]string methodName = null)
        {
        }

        protected override void LogEnd([CallerMemberName]string methodName = null)
        {
        }

        protected override void OnDisposed()
        {
            
        }
    }
}
