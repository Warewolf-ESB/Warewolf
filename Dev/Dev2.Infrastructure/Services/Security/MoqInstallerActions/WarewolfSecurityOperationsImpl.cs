
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using System;
using System.Collections;
#if NOTNANOSERVER
using System.DirectoryServices;
#endif
using System.Linq;
using System.Runtime.InteropServices;

namespace Dev2.Services.Security.MoqInstallerActions
{
    /// <summary>
    /// Provides Windows local group management operations used by the installer.
    /// This implementation manipulates the local machine's "Warewolf Administrators" group
    /// using the WinNT provider via <see cref="System.DirectoryServices.DirectoryEntry"/>.
    /// </summary>
    internal class WarewolfSecurityOperationsImpl : IWarewolfSecurityOperations
    {
        private const string WarewolfGroup = "Warewolf Administrators";
        private const string AdministratorsGroup = "Administrators";
        private const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";

        // http://ss64.com/nt/syntax-security_groups.html

        /// <summary>
        /// Adds the local "Warewolf Administrators" group to the machine.
        /// </summary>
        /// <remarks>
        /// Uses the WinNT provider to create a group entry with a description.
        /// </remarks>
        public void AddWarewolfGroup()
		{
#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
            { 
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    var newGroup = ad.Children.Add(WarewolfGroup, "Group");
                    newGroup.Invoke("Put", new object[] { "Description", WarewolfGroupDesc });
                    newGroup.CommitChanges();
                }
			}
#endif
		}

		/// <summary>
		/// Determines whether the "Warewolf Administrators" group exists on the local machine.
		/// </summary>
		/// <returns><c>true</c> if the group exists; otherwise <c>false</c>.</returns>
		public bool DoesWarewolfGroupExist()
		{
#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
			{
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    if (ad.Children.Cast<DirectoryEntry>().Any(dChildEntry => dChildEntry.Name == WarewolfGroup))
                    {
                        return true;
                    }
                }
			}
#endif
			return false;
        }

        /// <summary>
        /// Checks whether the specified user is a member of the "Warewolf Administrators" group.
        /// </summary>
        /// <param name="username">User name to check. May include domain (e.g. "DOMAIN\User").</param>
        /// <returns><c>true</c> if the user is a member of the group; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="username"/> is null or empty.</exception>
        public bool IsUserInGroup(string username)
        {
            if(string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            var theUser = username;
            var domainChar = username.IndexOf("\\", StringComparison.Ordinal);
            if(domainChar >= 0)
            {
                theUser = username.Substring((domainChar + 1));
            }

#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
            {
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach (DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name == WarewolfGroup)
                        {
                            // Now check group membership ;)
                            var members = dChildEntry.Invoke("Members");

                            if (members != null)
                            {
                                foreach (var member in (IEnumerable)members)
                                {
                                    using (var memberEntry = new DirectoryEntry(member))
                                    {
                                        if (memberEntry.Name == theUser)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
#endif

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

#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
            {
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {

                    ad.Children.SchemaFilter.Add("group");
                    foreach (DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name == WarewolfGroup)
                        {
                            dChildEntry.Invoke("Add", new object[] { currentUser });
                        }
                    }
                }
            }
#endif
        }

        public void AddAdministratorsGroupToWarewolf()
		{
#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
            {
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach (DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name == WarewolfGroup)
                        {
                            const string Entry = "WinNT://./" + AdministratorsGroup;
                            dChildEntry.Invoke("Add", new object[] { Entry });
                        }
                    }
                }
            }
#endif
        }

        public bool IsAdminMemberOfWarewolf()
        {
#if NOTNANOSERVER
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
            {
                using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
                {
                    ad.Children.SchemaFilter.Add("group");
                    foreach (DirectoryEntry dChildEntry in ad.Children)
                    {
                        if (dChildEntry.Name == WarewolfGroup)
                        {
                            // Now check group membership ;)
                            var members = dChildEntry.Invoke("Members");

                            if (members != null)
                            {
                                foreach (var member in (IEnumerable)members)
                                {
                                    using (var memberEntry = new DirectoryEntry(member))
                                    {
                                        if (memberEntry.Name == AdministratorsGroup)
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
#endif
            return false;
        }

        public void DeleteWarewolfGroup()
		{
#if NOTNANOSERVER
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !GlobalConstants.IsNanoServer())
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
#endif
        }

        /// <summary>
        /// Formats a user name into a WinNT path suitable for adding to a group (for example: "WinNT://DOMAIN/User,user").
        /// </summary>
        /// <param name="currentUser">The input user name, which may include a domain ("DOMAIN\User").</param>
        /// <param name="machineName">The local machine name to use when no domain is present.</param>
        /// <returns>A WinNT formatted user path string that can be passed to DirectoryEntry group membership methods.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="currentUser"/> or <paramref name="machineName"/> is null or empty.</exception>
        public string FormatUserForInsert(string currentUser, string machineName)
        {
            if(string.IsNullOrEmpty(currentUser))
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            if(string.IsNullOrEmpty(machineName))
            {
                throw new ArgumentNullException(nameof(machineName));
            }

            // Guest, Dev2\IntegrationTester
            var domainChar = currentUser.IndexOf("\\", StringComparison.Ordinal);
            string user;
            string userPath;


            // ,user
            if(domainChar >= 0)
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
