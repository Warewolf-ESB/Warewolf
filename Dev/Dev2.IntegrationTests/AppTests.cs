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
        const string ServerProcessName = "Warewolf Server";
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
                var studioPath = GetProcessPath(ServerProcessName).Replace(@"\Warewolf Server.exe", @"\Warewolf Studio.exe").Replace("ServerbinDebug", "StudiobinDebug");

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

        // NOTE : This test assumes that there is a server running as part of the integration test suite ;)
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("LifecycleManager_StartServer")]
        public void LifecycleManager_StartServer_WhenAServerIsRunning_ExpectSecondServerToCrash()
        {
            int runningServerID = FetchRunningServerID();

            try
            {
                var serverPath = GetProcessPath(ServerProcessName);

                //Pre-assert
                Assert.IsTrue(File.Exists(serverPath), "Server not found at " + serverPath);

                // fire off process 
                Process p = new Process { StartInfo = { FileName = serverPath, RedirectStandardOutput = true, UseShellExecute = false } };
                p.OutputDataReceived += OutputHandler; // ensure we can grap the output ;)
                p.Start();
                p.BeginOutputReadLine();

                // Wait for Process to start, and get past the check for a duplicate process
                Thread.Sleep(10000);

                // kill any hanging instances ;)
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

                            if(myID != runningServerID)
                            {
                                var proc = Process.GetProcessById(myID);

                                proc.Kill();
                            }
                        }
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
            }

            const string expected = "Critical Failure: Webserver failed to startException has been thrown by the target of an invocation.";

            StringAssert.Contains(outputData, expected);
        }

        private static string GetProcessPath(string processName)
        {
            var query = new System.Management.SelectQuery(@"SELECT * FROM Win32_Process where Name LIKE '%" + processName + "%'");
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
