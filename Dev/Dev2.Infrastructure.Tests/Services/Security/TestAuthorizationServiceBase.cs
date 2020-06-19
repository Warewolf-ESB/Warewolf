/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Security.Principal;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Services.Security;
using Warewolf.Data;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    public class TestAuthorizationServiceBase : AuthorizationServiceBase
    {
        private readonly bool _overrideIsAdminMember;
        public TestAuthorizationServiceBase(ISecurityService securityService, bool isLocalConnection = true, bool areAdminsWarewolfMembers = true, bool overrideAreAdminsFn = false)
            : base(securityService, isLocalConnection)
        {
            if (!overrideAreAdminsFn)
            {
                AreAdministratorsMembersOfWarewolfAdministrators = () => areAdminsWarewolfMembers;
            }
        }
        public TestAuthorizationServiceBase(IDirectoryEntryFactory directoryEntryFactory, ISecurityService securityService, bool isLocalConnection = true, bool areAdminsWarewolfMembers = true, bool overrideAreAdminsFn = false, bool overrideIsAdminMember = true)
            : base(directoryEntryFactory, securityService, isLocalConnection)
        {
            _overrideIsAdminMember = overrideIsAdminMember;
            if (overrideAreAdminsFn)
            {
                AreAdministratorsMembersOfWarewolfAdministrators = () => areAdminsWarewolfMembers;
            }
        }
        protected override bool IsGroupNameAdministrators<T>(T member, string adGroup)
        {
            if(!_overrideIsAdminMember)
            {
                return base.IsGroupNameAdministrators(member, adGroup);
            }
            return MemberOfAdminOverride;
        }

        public bool MemberOfAdminOverride { get; set; }
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

        // public override bool IsAuthorized(AuthorizationContext context, string resource)
        // {
        //     return IsAuthorized(User, context, resource);
        // }
        public override bool IsAuthorized(AuthorizationContext context, Guid resource)
        {
            return IsAuthorized(User, context, resource);
        }
        public override bool IsAuthorized(AuthorizationContext context, IWarewolfResource resource)
        {
            return IsAuthorized(User, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            return IsAuthorized(request.User, AuthorizationContext.Any, new WebNameSimple(request.QueryString["rid"]));
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
