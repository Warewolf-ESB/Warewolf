using System;
using System.Collections.Generic;
using System.Linq;
using System.Network;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dev2.Communication;
using Dev2.Controller;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Security
{
    public class ClientSecurityService : SecurityServiceBase
    {
        readonly IEnvironmentConnection _environmentConnection;
        readonly PermissionsModifiedService _permissionsModifiedService;

        public ClientSecurityService(IEnvironmentConnection environmentConnection)
        {
            VerifyArgument.IsNotNull("environmentConnection", environmentConnection);
            _environmentConnection = environmentConnection;
            _environmentConnection.NetworkStateChanged += OnNetworkStateChanged;
            _permissionsModifiedService = new PermissionsModifiedService(environmentConnection.ServerEvents);
            _permissionsModifiedService.Subscribe(Guid.Empty, ReceivePermissionsModified);
        }

        void ReceivePermissionsModified(PermissionsModifiedMemo obj)
        {
            if(Permissions == null || Permissions.Count == 0)
            {
                return;
            }
            var modifiedPermissions = obj.ModifiedPermissions;
            foreach(var modifiedPermission in modifiedPermissions)
            {
                var foundPermission = Permissions.FirstOrDefault(perm =>
                    modifiedPermission.IsServer == perm.IsServer
                    && modifiedPermission.ResourceID == perm.ResourceID
                    && modifiedPermission.WindowsGroup == perm.WindowsGroup);

                if(foundPermission != null)
                {
                    foundPermission.Permissions = modifiedPermission.Permissions;
                }
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
            // ReSharper disable once UnusedVariable
            var task = ReadAsync();
        }

        public virtual async Task ReadAsync()
        {
            await Task.Factory.StartNew(() => base.Read());
        }

        protected override List<WindowsGroupPermission> ReadPermissions()
        {
            var communicationController = new CommunicationController
            {
                ServiceName = "SecurityReadService"
            };
            SecuritySettingsTO securitySettingsTO = communicationController.ExecuteCommand<SecuritySettingsTO>(_environmentConnection, _environmentConnection.WorkspaceID);
            return securitySettingsTO.WindowsGroupPermissions;
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
            if(_permissionsModifiedService != null)
            {
                _permissionsModifiedService.Dispose();
            }
        }
    }
}