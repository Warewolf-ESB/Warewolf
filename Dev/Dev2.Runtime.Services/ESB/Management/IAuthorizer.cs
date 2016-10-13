﻿using System;
using Dev2.Common;
using Dev2.Runtime.Security;
using Dev2.Services.Security;
// ReSharper disable MemberCanBeInternal

namespace Dev2.Runtime.ESB.Management
{
    public interface IAuthorizer
    {
        void RunPermissions(Guid resourceId);
    }

    public class SecuredCreateEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredCreateEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public SecuredCreateEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Contribute, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToCreateException);
            }
        }

        #endregion
    }

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
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.View, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToViewException);
            }
        }

        #endregion
    }

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
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Contribute, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException);
            }
        }

        #endregion
    }

    public class SecuredExecuteManagementEndpoint : IAuthorizer
    {
        private readonly IAuthorizationService _authorizationService;

        public SecuredExecuteManagementEndpoint(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        // ReSharper disable once UnusedMember.Global
        public SecuredExecuteManagementEndpoint()
            : this(ServerAuthorizationService.Instance)
        {

        }
        #region Implementation of ISecuredCreateManagementEndpoint

        public void RunPermissions(Guid resourceId)
        {
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Execute, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToExecuteException);
            }
        }

        #endregion
    }
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
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.DeployFrom, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployFromException);
            }
        }

        #endregion
    }

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
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.DeployTo, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToDeployToException);
            }
        }

        #endregion
    }

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
            ((ServerAuthorizationService)_authorizationService).RefreshPermission();
            var isAuthorized = _authorizationService.IsAuthorized(AuthorizationContext.Administrator, resourceId.ToString());
            if (!isAuthorized)
            {
                throw new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToAdministratorException);
            }
        }

        #endregion
    }
}
