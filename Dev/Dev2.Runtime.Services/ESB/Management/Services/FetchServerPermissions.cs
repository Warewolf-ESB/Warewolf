using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Find Server Username
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FetchServerPermissions : IEsbManagementEndpoint
    {
        private readonly IAuthorizer _authorizer;
        // ReSharper disable once MemberCanBePrivate.Global
        public FetchServerPermissions(IAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        // ReSharper disable once MemberCanBeInternal
        public FetchServerPermissions()
            :this(new SecuredAdministratorManagementEndpoint())
        {

        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                Dev2Logger.Info("Find Server User Name");
                _authorizer.RunPermissions(GlobalConstants.ServerWorkspaceID);
                var user = Thread.CurrentPrincipal;
                var permissionsMemo = new PermissionsModifiedMemo
                {
                    ModifiedPermissions = ServerAuthorizationService.Instance.GetPermissions(user),
                    ServerID = HostSecurityProvider.Instance.ServerID
                };
                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(permissionsMemo);
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService findServerUsernameService = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            ServiceAction findServerUsernameServiceAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceName = HandlesType(), SourceMethod = HandlesType() };

            findServerUsernameService.Actions.Add(findServerUsernameServiceAction);

            return findServerUsernameService;
        }

        public string HandlesType()
        {
            return "FetchServerPermissions";
        }
    }
}