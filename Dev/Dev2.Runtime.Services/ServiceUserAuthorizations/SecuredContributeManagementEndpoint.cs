using System;
using Dev2.Common;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ServiceUserAuthorizations
{
    public class SecuredContributeManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredContributeManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public SecuredContributeManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            _authorizationService.ClearCaches();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Contribute, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException);
            }
        }

        #endregion
    }
}