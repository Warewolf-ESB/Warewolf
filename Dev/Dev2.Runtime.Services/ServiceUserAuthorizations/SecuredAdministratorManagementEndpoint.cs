using System;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ServiceUserAuthorizations
{
    public class SecuredAdministratorManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredAdministratorManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public SecuredAdministratorManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Administrator, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToAdministratorException);
            }
        }

        #endregion
    }
}