/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Management;
using System.Security.Principal;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Dev2.Common.Common
{
    /// <summary>
    /// </summary>
    public class GetComputerNames
    {
        private static List<string> _currentComputerNames;

        public static List<string> ComputerNames
        {
            get
            {
                if (_currentComputerNames == null)
                {
                    GetComputerNamesList();
                }

                return _currentComputerNames;
            }
        }

        public static void GetComputerNamesList()
        {
            _currentComputerNames = StandardComputerNameQuery();
        }

        /// <summary>
        ///     Query for Network Computer Names
        /// </summary>
        /// <returns></returns>
        private static List<string> StandardComputerNameQuery()
        {
            WindowsIdentity wi = WindowsIdentity.GetCurrent();

            if (wi != null)
            {
                string serverUserName = wi.Name;

                string[] parts = serverUserName.Split('\\');

                string queryStr = "WinNT://";

                // query with domain appended ;)
                if (parts.Length == 2)
                {
                    queryStr += parts[0];
                }
                else
                {
                    // find the first workgroup and report on it ;)

                    try
                    {
                        var query = new SelectQuery("Win32_ComputerSystem");
                        var searcher = new ManagementObjectSearcher(query);

                        ManagementObjectCollection tmp = searcher.Get();

                        ManagementObjectCollection.ManagementObjectEnumerator itr = tmp.GetEnumerator();

                        if (itr.MoveNext())
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

                DirectoryEntries kids = root.Children;

                return (from DirectoryEntry node in kids where node.SchemaClassName == "Computer" select node.Name).ToList();
            }

            // big problems, add this computer and return
            return new List<string> { Environment.MachineName };
        }
    }
}