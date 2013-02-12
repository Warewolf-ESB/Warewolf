using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class AppTests
    {
        // Fixed by Michael RE Broken Integration Tests (12th Feb 2013)
        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            
            Process process = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Contains("Dev2.Studio"));
            
            int actual = 0;
            int expected = 1;
            string studioPath = string.Empty;
            if (process != null && !process.ProcessName.Contains("vshost"))
            {
                // A Studio had been started before the test had run - We're probably running it on the Build Server
                Process.Start(process.MainModule.FileName);
                // Wait for Process to start, else it works in debug, not run
                System.Threading.Thread.Sleep(2000);
                actual = Process.GetProcesses().Count(c => c.ProcessName.Contains("Dev2.Studio"));
                expected = 1;
            }
            else
            {
                // We're running local integration tests, so all Studios need to be killed after they've been opened
                Process.Start("Dev2.Studio.exe");
                Process.Start("Dev2.Studio.exe");
                // Wait for Process to start, else it works in debug, not run
                System.Threading.Thread.Sleep(2000);
                Process[] processes = Process.GetProcesses().Where(c => c.ProcessName == "Dev2.Studio").ToArray();
                expected = 1;
                actual = processes.Count(c => !c.ProcessName.Contains("vshost"));
                foreach (Process p in processes)
                {
                    p.Kill();
                }
            }

            Assert.AreEqual(expected, actual);
        }
    }
}
