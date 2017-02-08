/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Controller;
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

        public IEnvironmentConnection EnvironmentConnection => _environmentConnection;

        void OnNetworkStateChanged(object sender, NetworkStateEventArgs args)
        {
            if(args.ToState == NetworkState.Online)
            {
                Read();
            }
        }

        public override async void Read()
        {
            Dev2Logger.Debug("Reading Permissions from Server");
            await ReadAsync();
        }

        public virtual async Task ReadAsync()
        {
            if (EnvironmentConnection.IsConnected)
            {
                var communicationController = new CommunicationController
                {
                    ServiceName = "SecurityReadService"
                };
                Dev2Logger.Debug("Getting Permissions from Server");

                SecuritySettingsTO securitySettingsTo = await communicationController.ExecuteCommandAsync<SecuritySettingsTO>(EnvironmentConnection,EnvironmentConnection.WorkspaceID);
                List<WindowsGroupPermission> newPermissions = null;
                if (securitySettingsTo != null)
                {
                    Permissions = securitySettingsTo.WindowsGroupPermissions;
                    newPermissions = securitySettingsTo.WindowsGroupPermissions;
                    Dev2Logger.Debug("Permissions from Server:" + Permissions);
                }
                if (newPermissions != null)
                {
                    RaisePermissionsModified(new PermissionsModifiedEventArgs(newPermissions));
                }
                RaisePermissionsChanged();
            }
        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
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
