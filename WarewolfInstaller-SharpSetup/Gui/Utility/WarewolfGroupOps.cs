
using System;
using System.Collections;
using System.DirectoryServices;
using System.Security.Principal;

namespace Gui.Utility
{
    public class WarewolfGroupOps
    {
        public const string WarewolfGroup = "Warewolf Administrators";
        public const string AdministratorsGroup = "Administrators";
        public const string WarewolfGroupDesc = "Warewolf Administrators have complete and unrestricted access to Warewolf";

        public void AddWarewolfGroup()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                DirectoryEntry newGroup = ad.Children.Add(WarewolfGroup, "Group");
                newGroup.Invoke("Put", new object[] { "Description", WarewolfGroupDesc });
                newGroup.CommitChanges();
            }
        }

        public void AddAdministratorsGroupToWarewolf()
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == "Warewolf Administrators")
                    {
                        string Entry = "WinNT://./" + FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
                        dChildEntry.Invoke("Add", new object[] { Entry });
                    }
                }
            }
        }

        public bool IsAdminMemberOfWarewolf()
        {

            var adGroup = FindGroup(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    if (dChildEntry.Name == "Warewolf Administrators")
                    {
                        // Now check group membership ;)
                        var members = dChildEntry.Invoke("Members");

                        if (members != null)
                        {
                            foreach (var member in (IEnumerable)members)
                            {
                                using (DirectoryEntry memberEntry = new DirectoryEntry(member))
                                {
                                    if (memberEntry.Name == adGroup)
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

        public bool DoesWarewolfGroupExist()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == WarewolfGroup)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsUserInGroup(string username)
        {
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
                                using(DirectoryEntry memberEntry = new DirectoryEntry(member))
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
                throw new Exception("Null or Empty User");
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

        public static string FindGroup(SecurityIdentifier searchSid)
        {
            using (var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach (DirectoryEntry dChildEntry in ad.Children)
                {
                    var bytes = (byte[])dChildEntry.Properties["objectSid"].Value;
                    var sid = new SecurityIdentifier(bytes, 0).ToString();

                    if (sid == searchSid.ToString())
                    {
                        return dChildEntry.Name;
                    }
                }
            }
            throw new Exception("Cannot fid group");
        }

        public string FormatUserForInsert(string currentUser, string machineName)
        {
            if(string.IsNullOrEmpty(currentUser))
            {
                throw new ArgumentNullException("currentUser");
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
