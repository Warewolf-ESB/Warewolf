
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Security.Principal;
using Dev2.Services.Security;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestAuthorizationServiceBase : AuthorizationServiceBase
    {
        public TestAuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection = true, bool areAdminsWarewolfMembers = true, bool overrideAreAdminsFn = false)
            : base(securityService, isLocalConnection)
        {
            if(!overrideAreAdminsFn)
            {
                AreAdministratorsMembersOfWarewolfAdministrators = () => areAdminsWarewolfMembers;
            }
        }

        public int RaisePermissionsChangedHitCount { get; private set; }
        public int RaisePermissionsModifiedHitCount { get; private set; }

        public IPrincipal User { get; set; }

        protected override void RaisePermissionsChanged()
        {
            RaisePermissionsChangedHitCount++;
            base.RaisePermissionsChanged();
        }

        protected override void OnPermissionsModified(PermissionsModifiedEventArgs e)
        {
            RaisePermissionsModifiedHitCount++;
            base.OnPermissionsModified(e);
        }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(User, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            return IsAuthorized(request.User, AuthorizationContext.Any, request.QueryString["rid"]);
        }

        public bool TestIsAuthorizedToConnect(IPrincipal principal)
        {
            return IsAuthorizedToConnect(principal);
        }

        protected override void OnDisposed()
        {
        }
    }
}
