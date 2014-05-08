
using System;
using System.DirectoryServices;

namespace Gui.Utility
{
    public class WarewolfGroupOps
    {

        public void AddWarewolfGroup()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                DirectoryEntry newGroup = ad.Children.Add("Warewolf", "Group");
                newGroup.Invoke("Put", new object[] { "Description", "Warewolf Group" });
                newGroup.CommitChanges();
            }
        }

        public bool DoesWarewolfGroupExist()
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == "Warewolf")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsUserInGroup(string username)
        {
            using(var ad = new DirectoryEntry("WinNT://" + Environment.MachineName + ",computer"))
            {
                ad.Children.SchemaFilter.Add("group");
                foreach(DirectoryEntry dChildEntry in ad.Children)
                {
                    if(dChildEntry.Name == "Warewolf")
                    {

                        return true;
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
                    if(dChildEntry.Name == "Warewolf")
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
                    if(dChildEntry.Name == "Warewolf")
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
