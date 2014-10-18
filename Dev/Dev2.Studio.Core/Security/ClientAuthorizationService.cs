
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Dev2.Common.Interfaces.Security;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Security
{
    public class ClientAuthorizationService : AuthorizationServiceBase
    {
        readonly IEnvironmentConnection _environmentConnection;

        public ClientAuthorizationService(ISecurityService securityService, bool isLocalConnection)
            : base(securityService, isLocalConnection)
        {
            var clientSecurityService = securityService as ClientSecurityService;
            if(clientSecurityService != null)
            {
                _environmentConnection = clientSecurityService.EnvironmentConnection;
            }
        }

        public override Permissions GetResourcePermissions(Guid resourceId)
        {
            var principal = _environmentConnection == null ? ClaimsPrincipal.Current : _environmentConnection.Principal;
            var groupPermissions = GetGroupPermissions(principal, resourceId.ToString()).ToList();
            var result = groupPermissions.Aggregate(Permissions.None, (current, gp) => current | gp.Permissions);
            return result;
        }


        protected override IEnumerable<WindowsGroupPermission> GetGroupPermissions(IPrincipal principal, string resource)
        {
            var serverPermissions = _securityService.Permissions;

            Guid resourceId;
            if (Guid.TryParse(resource, out resourceId))
            {
                if (resourceId == Guid.Empty)
                {
                    return serverPermissions.Where(permission => permission.IsServer);
                }
                return serverPermissions.Where(p => p.Matches(resource) && !p.IsServer).ToList();
            }
            return serverPermissions;
        }
//
//        protected override bool IsInRole(IPrincipal principal, WindowsGroupPermission p)
//        {
//            var isInRole = false;
//
//            try
//            {
//                // If its our admin group ( Warewolf ), we need to check membership differently 
//                // Prefixing with computer name does not work with Warewolf and IsInRole
//                // Plain does not work with IsInRole
//                // Hence this conditional check and divert
//                var windowsGroup = p.WindowsGroup;
//                if(windowsGroup == WindowsGroupPermission.BuiltInAdministratorsText && _environmentConnection.AuthenticationType != AuthenticationType.Public)
//                {
//                    // We need to get the group as it is local then look for principle's membership
//                    var principleName = principal.Identity.Name;
//                    if(!string.IsNullOrEmpty(principleName))
//                    {
//                        // Examine if BuiltIn\Administrators is still present as a Member
//                        // Then inspect BuiltIn\Administrators
//
//                        // Examine group for this member ;)  
//                        isInRole = principal.IsInRole(windowsGroup);
//
//                        // if that fails, check Administrators group membership in the Warewolf Administrators group
//                        if(!isInRole)
//                        {
//                            SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
//                            var windowsPrincipal = principal as WindowsPrincipal;
//                            if(windowsPrincipal != null)
//                            {
//                                isInRole = (windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator) || windowsPrincipal.IsInRole(sid)) && _environmentConnection.IsLocalHost;
//                                if(!isInRole)
//                                {
//                                    var adGroup = FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
//                                    using(var ad = new DirectoryEntry("WinNT://" + _environmentConnection.AppServerUri.Host.ToLower() + ",computer"))
//                                    {
//                                        ad.Children.SchemaFilter.Add("group");
//                                        var groupIsPartOfWarewolfAdmins = false;
//                                        var userIsPartOfGroup = false;
//                                        foreach(DirectoryEntry dChildEntry in ad.Children)
//                                        {
//                                            if(dChildEntry.Name == "Warewolf Administrators")
//                                            {
//                                                // Now check group membership ;)
//                                                var members = dChildEntry.Invoke("Members");
//
//                                                if(members != null)
//                                                {
//                                                    foreach(var member in (IEnumerable)members)
//                                                    {
//                                                        using(DirectoryEntry memberEntry = new DirectoryEntry(member))
//                                                        {
//                                                            if(memberEntry.Name == adGroup)
//                                                            {
//                                                                groupIsPartOfWarewolfAdmins = true;
//                                                            }
//                                                        }
//                                                    }
//                                                }
//                                            }
//                                            if(dChildEntry.Name == adGroup)
//                                            {
//                                                var members = dChildEntry.Invoke("Members");
//                                                foreach(var member in (IEnumerable)members)
//                                                {
//                                                    using(DirectoryEntry memberEntry = new DirectoryEntry(member))
//                                                    {
//                                                        if(memberEntry.Path.ToLower().Contains(principleName.Replace('\\', '/').ToLower()))
//                                                        {
//                                                            userIsPartOfGroup = true;
//                                                        }
//                                                    }
//                                                }
//                                            }
//                                        }
//                                        return userIsPartOfGroup && groupIsPartOfWarewolfAdmins;
//                                    }
//                                }
//                            }
//                            else
//                            {
//                                if(AreAdministratorsMembersOfWarewolfAdministrators.Invoke())
//                                {
//                                    // Check user's administrator membership
//                                    isInRole = principal.IsInRole(sid.Value);
//                                }
//
//                                //Check regardless. Not installing the software can create a situation where the "Administrators" group is not part of Warewolf
//                                isInRole = principal.IsInRole(sid.Value);
//                            }
//                        }
//
//                        return isInRole;
//                    }
//                }
//                else
//                {
//                    // THIS TRY-CATCH IS HERE TO AVOID THE EXPLORER NOT LOADING ANYTHING WHEN THE DOMAIN CANNOT BE CONTACTED!
//                    if(windowsGroup != WindowsGroupPermission.BuiltInGuestsText)
//                    {
//                        isInRole = principal.IsInRole(windowsGroup);
//                    }
//                }
//            }
//            // ReSharper disable EmptyGeneralCatchClause
//            catch { }
//            // ReSharper restore EmptyGeneralCatchClause
//
//            return isInRole || p.IsBuiltInGuestsForExecution;
//        }
//

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            return IsAuthorized(_environmentConnection.Principal, context, resource);
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            return false;
        }

        protected override void OnDisposed()
        {
        }
    }
}
