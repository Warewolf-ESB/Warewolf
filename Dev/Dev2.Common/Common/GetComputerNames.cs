using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Management;
using System.Security.Principal;

namespace Dev2.Common.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class GetComputerNames
    {

        static List<string> CurrentComputerNames;

        public static void GetComputerNamesList()
        {
            CurrentComputerNames = StandardComputerNameQuery();
        }

        public static List<string> ComputerNames
        {
            get
            {
                if(CurrentComputerNames == null)
                {
                    GetComputerNamesList();
                }

                return CurrentComputerNames;
            }
        }

        /// <summary>
        /// Query for Network Computer Names
        /// </summary>
        /// <returns></returns>
        private static List<string> StandardComputerNameQuery()
        {
            WindowsIdentity wi = WindowsIdentity.GetCurrent();

            if(wi != null)
            {
                var serverUserName = wi.Name;

                var parts = serverUserName.Split('\\');

                var queryStr = "WinNT://";

                // query with domain appended ;)
                if(parts.Length == 2)
                {
                    queryStr += parts[0];
                }
                else
                {
                    // find the first workgroup and report on it ;)

                    try
                    {
                        SelectQuery query = new SelectQuery("Win32_ComputerSystem");
                        ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

                        ManagementObjectCollection tmp = searcher.Get();

                        var itr = tmp.GetEnumerator();

                        if(itr.MoveNext())
                        {
                            queryStr += itr.Current["Workgroup"] as string;
                        }
                    }
                    // ReSharper disable EmptyGeneralCatchClause
                    catch
                    // ReSharper restore EmptyGeneralCatchClause
                    {
                        // best effort ;)
                    }

                }

                var root = new DirectoryEntry(queryStr);

                var kids = root.Children;

                List<string> result = new List<string>();
                foreach(DirectoryEntry node in kids)
                {
                    if(node.SchemaClassName == "Computer")
                    {
                        result.Add(node.Name);
                    }
                }

                return result;
            }

            // big problems, add this computer and return
            return new List<string> { Environment.MachineName };
        }
    }
}
