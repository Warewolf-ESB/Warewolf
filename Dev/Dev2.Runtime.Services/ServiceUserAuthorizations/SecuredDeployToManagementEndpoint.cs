using System;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ServiceUserAuthorizations
{
    public class SecuredDeployToManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredDeployToManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        // ReSharper disable once UnusedMember.Global
        public SecuredDeployToManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployToException);
            }
        }

        #endregion
    }
}