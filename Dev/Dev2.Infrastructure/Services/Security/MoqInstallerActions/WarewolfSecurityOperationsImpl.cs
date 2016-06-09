
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;

namespace Dev2.Services.Security.MoqInstallerActions
{
    /// <summary>
    /// This is the group operations class used in the installer
    /// </summary>
    internal class WarewolfSecurityOperationsImpl : IWarewolfSecurityOperations
    {
        private const string WarewolfGroup = "Warewolf Administrators";
        private const string AdministratorsGroup = "Administrators";
        private const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";

        // http://ss64.com/nt/syntax-security_groups.html

        public void AddWarewolfGroup()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                DirectoryEntry newGroup = ad.Children.Add(WarewolfGroup, "Group");
                newGroup.Invoke("Put", "Description", WarewolfGroupDesc);
                newGroup.CommitChanges();
            }
        }

        public bool DoesWarewolfGroupExist()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                if(ad.Children.Cast<DirectoryEntry>().Any(dChildEntry => dChildEntry.Name == WarewolfGroup))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddAdministratorsGroupToWarewolf()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        const string Entry = "WinNT://./" + AdministratorsGroup;
                        dChildEntry.Invoke("Add", Entry);
                    }
                }
            }
            var systemContext = new PrincipalContext(ContextType.Machine, null);
            var warewolfGroupPrincipal = GroupPrincipal.FindByIdentity(systemContext, WarewolfGroup);
            if (warewolfGroupPrincipal != null)
            {
                var adminGroupPrincipal = GroupPrincipal.FindByIdentity(systemContext, "Administrators");
                if(adminGroupPrincipal != null)
                {
                    if (!warewolfGroupPrincipal.Members.Contains(systemContext, IdentityType.SamAccountName, adminGroupPrincipal.SamAccountName))
                    {
                        warewolfGroupPrincipal.Members.Add(adminGroupPrincipal);
                        warewolfGroupPrincipal.Save();
                    }
                }
            }
        }

        public void DeleteWarewolfGroup()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        ad.Children.Remove(dChildEntry);
                    }
                }
            }
        }
    }
}
