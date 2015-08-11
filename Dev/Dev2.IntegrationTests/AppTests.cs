
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class AppTests
    {
        private static string deployDir;
        const string ServerProcessName = "Warewolf Server.exe";
        const string StudioProcessName = "Warewolf Studio";

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            deployDir = testContext.TestDeploymentDir;
        }

        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            bool actual = false;

            var msg = string.Empty;

            try
            {
                var studioPath = GetProcessPath(ServerProcessName).Replace(@"\Warewolf Server.exe", @"\Warewolf Studio.exe").Replace(@"Server\", @"Studio\").Replace(@"server\", @"studio\");

                //Pre-assert
                Assert.IsTrue(File.Exists(studioPath), "Studio not found at " + studioPath);

                Process firstProcess = Process.Start(studioPath);

                // Wait for Process to start, and get past the check for a duplicate process
                Thread.Sleep(7000);

                if(firstProcess == null)
                {
                    Assert.Fail("Cannot start first Studio instance!");
                }

                // Start a second studio, this should hit the logic that checks for a duplicate and exit
                Process secondProcess = Process.Start(studioPath);

                // Gather actual
                if(secondProcess != null)
                {
                    actual = secondProcess.WaitForExit(15000);
                }
                else
                {
                    Assert.Fail("Cannot start second Studio instance!");
                }

                const string wmiQueryString = "SELECT ProcessId FROM Win32_Process WHERE Name LIKE 'Warewolf Studio%'";
                using(var searcher = new ManagementObjectSearcher(wmiQueryString))
                {
                    using(var results = searcher.Get())
                    {
                        ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();

                        if(mo != null)
                        {
                            var id = mo.Properties["ProcessId"].Value.ToString();

                            int myID;
                            Int32.TryParse(id, out myID);

                            var proc = Process.GetProcessById(myID);

                            proc.Kill();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                msg = e.Message;
                msg += Environment.NewLine;
                msg += e.StackTrace;
            }

            Assert.IsTrue(actual, "Failed to kill second studio! [ " + msg + " ]");
        }

        private static string GetProcessPath(string processName)
        {
            var query = new SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + "%'");
            ManagementObjectCollection processes;
            //initialize the searcher with the query it is
            //supposed to execute
            using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                //execute the query
                processes = searcher.Get();
                if(processes.Count <= 0)
                {
                    return null;
                }
            }
            if(processes == null || processes.Count == 0)
            {
                return null;
            }
            return (from ManagementObject process in processes select (process.Properties["ExecutablePath"].Value ?? string.Empty).ToString()).FirstOrDefault();
        }

        #region Server Lifecycle Manager Test Utils

        private static string outputData = string.Empty;
        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            var data = outLine.Data;
            if(!String.IsNullOrEmpty(data))
            {
                outputData += data;
            }
        }

        private int FetchRunningServerID()
        {
            const string wmiQueryString = "SELECT ProcessId FROM Win32_Process WHERE Name LIKE 'Warewolf Server%'";
            using(var searcher = new ManagementObjectSearcher(wmiQueryString))
            {
                using(var results = searcher.Get())
                {
                    ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();

                    if(mo != null)
                    {
                        var id = mo.Properties["ProcessId"].Value.ToString();

                        int myID;
                        Int32.TryParse(id, out myID);

                        return myID;
                    }
                }
            }

            return 0;
        }

        #endregion
    }
}
