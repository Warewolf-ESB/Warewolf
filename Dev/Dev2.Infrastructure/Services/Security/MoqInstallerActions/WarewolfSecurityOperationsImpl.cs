#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using Dev2.Common;
using Warewolf.Resource.Errors;

namespace Dev2.Services.Security.MoqInstallerActions
{
    /// <summary>
    /// This is the group operations class used in the installer
    /// </summary>
    class WarewolfSecurityOperationsImpl : IWarewolfSecurityOperations
    {
        const string WarewolfGroup = "Warewolf Administrators";
        const string AdministratorsGroup = "Administrators";
        const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";

        // http://ss64.com/nt/syntax-security_groups.html

        public void AddWarewolfGroup()
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                var newGroup = ad.Children.Add(WarewolfGroup, "Group");
                newGroup.Invoke("Put", "Description", WarewolfGroupDesc);
                newGroup.CommitChanges();
            }
        }

        public bool DoesWarewolfGroupExist()
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                if (ad.Children.Cast<DirectoryEntry>().Any(dChildEntry => string.Equals(dChildEntry.Name, WarewolfGroup, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsUserInGroup(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            var theUser = username;
            var domainChar = username.IndexOf("\\", StringComparison.Ordinal);
            if (domainChar >= 0)
            {
                theUser = username.Substring(domainChar + 1);
            }

            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == WarewolfGroup)
                    {
                        return IsMemberOfGroup(theUser, dChildEntry);
                    }
                }
            }

            return false;
        }

        private static bool IsMemberOfGroup(string theUser, DirectoryEntry dChildEntry)
        {
            bool isMember = false;
            var members = dChildEntry.Invoke("Members");

            if (members != null)
            {
                foreach (var member in (IEnumerable)members)
                {
                    using (var memberEntry = new DirectoryEntry(member))
                    {
                        isMember = memberEntry.Name == theUser;
                    }
                }
            }

            return isMember;
        }

        public void AddUserToWarewolf(string currentUser)
        {
            if (string.IsNullOrEmpty(currentUser))
            {                
                throw new ArgumentNullException("Null or Empty User");                
            }

            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == WarewolfGroup)
                    {
                        try
                        {
                            dChildEntry.Invoke("Add", currentUser);
                        }
                        catch (Exception)
                        {
                            Dev2Logger.Error(string.Format(ErrorResource.UserDoesNotExistOnTheMachine, currentUser), GlobalConstants.WarewolfError);
                        }
                    }
                }
            }
        }

        public void AddAdministratorsGroupToWarewolf()
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == WarewolfGroup)
                    {
                        const string Entry = "WinNT://./" + AdministratorsGroup;
                        try
                        {
                            dChildEntry.Invoke("Add", Entry);
                        }
                        catch(Exception)
                        {
                            //Already part of the group
                        }
                    }
                }
            }
            var systemContext = new PrincipalContext(ContextType.Machine, null);
            var warewolfGroupPrincipal = GroupPrincipal.FindByIdentity(systemContext, WarewolfGroup);
            if (warewolfGroupPrincipal != null)
            {
                var adminGroupPrincipal = GroupPrincipal.FindByIdentity(systemContext, "Administrators");
                if (adminGroupPrincipal != null && !warewolfGroupPrincipal.Members.Contains(systemContext, IdentityType.SamAccountName, adminGroupPrincipal.SamAccountName))
                {
                    warewolfGroupPrincipal.Members.Add(adminGroupPrincipal);
                    warewolfGroupPrincipal.Save();
                }

            }
        }

        public bool IsAdminMemberOfWarewolf()
        {
            bool result = false;
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == WarewolfGroup)
                    {
                        result = IsAdminMemberOfDirectoryEntry(dChildEntry);
                    }
                }
            }

            return result;
        }

        private static bool IsAdminMemberOfDirectoryEntry(DirectoryEntry dChildEntry)
        {
            bool result = false;
            var members = dChildEntry.Invoke("Members");

            if (members != null)
            {
                foreach (var member in (IEnumerable)members)
                {
                    using (var memberEntry = new DirectoryEntry(member))
                    {
                        result = memberEntry.Name == AdministratorsGroup;
                    }
                }
            }

            return result;
        }

        public void DeleteWarewolfGroup()
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == WarewolfGroup)
                    {
                        ad.Children.Remove(dChildEntry);
                    }
                }
            }
        }

        public string FormatUserForInsert(string currentUser, string machineName)
        {
            if (string.IsNullOrEmpty(currentUser))
            {
                throw new ArgumentNullException("currentUser");
            }

            if (string.IsNullOrEmpty(machineName))
            {
                throw new ArgumentNullException("machineName");
            }

            // Guest, Dev2\IntegrationTester
            var domainChar = currentUser.IndexOf("\\", StringComparison.Ordinal);
            string user;
            string userPath;


            // ,user
            if (domainChar >= 0)
            {
                var domain = currentUser.Substring(0, domainChar);
                user = currentUser.Substring(domainChar + 1);
                userPath = string.Format("WinNT://{0}/{1},user", domain, user);
            }
            else
            {
                user = currentUser;
                userPath = string.Format("WinNT://{0}/{1},user", machineName, user);
            }

            return userPath;
        }
    }
}
