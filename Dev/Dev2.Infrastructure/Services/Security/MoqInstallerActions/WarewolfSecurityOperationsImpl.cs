
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
using System.DirectoryServices;
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
                newGroup.Invoke("Put", new object[] { "Description", WarewolfGroupDesc });
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

        public bool IsUserInGroup(string username)
        {

            if(string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException("username");
            }

            var theUser = username;
            var domainChar = username.IndexOf("\\", StringComparison.Ordinal);
            if(domainChar >= 0)
            {
                theUser = username.Substring((domainChar + 1));
            }

            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        // Now check group membership ;)
                        var members = dChildEntry.Invoke("Members");

                        if(members != null)
                        {
                            foreach(var member in (IEnumerable)members)
                            {
                                using(var memberEntry = new DirectoryEntry(member))
                                {
                                    if(memberEntry.Name == theUser)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void AddUserToWarewolf(string currentUser)
        {
            if(string.IsNullOrEmpty(currentUser))
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("Null or Empty User");
                // ReSharper restore NotResolvedInText
            }

            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {

                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        dChildEntry.Invoke("Add", new object[] { currentUser });
                    }
                }
            }
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
                        dChildEntry.Invoke("Add", new object[] { Entry });
                    }
                }
            }
        }

        public bool IsAdminMemberOfWarewolf()
        {

            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        // Now check group membership ;)
                        var members = dChildEntry.Invoke("Members");

                        if(members != null)
                        {
                            foreach(var member in (IEnumerable)members)
                            {
                                using(var memberEntry = new DirectoryEntry(member))
                                {
                                    if(memberEntry.Name == AdministratorsGroup)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
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

        public string FormatUserForInsert(string currentUser, string machineName)
        {
            if(string.IsNullOrEmpty(currentUser))
            {
                throw new ArgumentNullException("currentUser");
            }

            if(string.IsNullOrEmpty(machineName))
            {
                throw new ArgumentNullException("machineName");
            }

            // Guest, Dev2\IntegrationTester
            var domainChar = currentUser.IndexOf("\\", StringComparison.Ordinal);
            string user;
            string userPath;


            // ,user
            if(domainChar >= 0)
            {
                string domain = currentUser.Substring(0, domainChar);
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
