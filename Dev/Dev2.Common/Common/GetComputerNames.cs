using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Common.Common
{
    public class GetComputerNames
    {
        static List<string> CurrentComputerNames;

        public static void GetComputerNamesList()
        {
            var root = new DirectoryEntry("WinNT:");
            CurrentComputerNames = root.Children.Cast<DirectoryEntry>()
                              .SelectMany(dom => dom.Children.Cast<DirectoryEntry>()
                                                    .Where(entry => entry.SchemaClassName == "Computer"))
                              .Select(entry => entry.Name)
                              .ToList();
        }

        public static List<string> ComputerNames
        {
            get
            {
                if (CurrentComputerNames == null)
                {
                    GetComputerNamesList();
                }
                return CurrentComputerNames;
            }
        }
    }
}
