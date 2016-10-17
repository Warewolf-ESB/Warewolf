using System;
using Dev2.Common;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ServiceUserAuthorizations
{
    public class SecuredDeployFromManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredDeployFromManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        // ReSharper disable once UnusedMember.Global
        public SecuredDeployFromManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployFromException);
            }
        }

        #endregion
    }
}