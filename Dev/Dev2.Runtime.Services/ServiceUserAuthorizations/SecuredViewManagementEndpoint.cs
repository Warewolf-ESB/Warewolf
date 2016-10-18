using System;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.Security;
using Dev2.Services.Security;

namespace Dev2.Runtime.ServiceUserAuthorizations
{
    public class SecuredViewManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredViewManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public SecuredViewManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.View, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(string.Format(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToViewException, Environment.NewLine, resourceId));
            }
        }

        #endregion
    }
}