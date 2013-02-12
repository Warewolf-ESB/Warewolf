using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class AppTests
    {
        // Fixed by Michael RE Broken Integration Tests (12th Feb 2013)
        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            string studioPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dev2.Studio.exe");
            Process alreadyRunningProcess = Process.GetProcesses().FirstOrDefault(c => c.ProcessName.Equals("Dev2.Studio", StringComparison.InvariantCultureIgnoreCase));

            if (alreadyRunningProcess == null)
            {
                Process.Start(studioPath);
            }

            Process.Start(studioPath);
            // Wait for Process to start, else it works in debug, not run
            Thread.Sleep(2000);

            var processes = Process.GetProcesses().Where(c => c.ProcessName == "Dev2.Studio").ToList();
            int actual = processes.Count;
            int expected = 1;

            foreach (Process p in processes)
            {
                p.Kill();
            }
            
            //int actual;
            //int expected;
            //if (process != null && !process.ProcessName.Contains("vshost"))
            //{
            //    // A Studio had been started before the test had run - We're probably running it on the Build Server
            //    Process.Start(process.MainModule.FileName);
            //    // Wait for Process to start, else it works in debug, not run
            //    System.Threading.Thread.Sleep(2000);
            //    actual = Process.GetProcesses().Count(c => c.ProcessName.Contains("Dev2.Studio"));
            //    expected = 1;
            //}
            //else
            //{
            //    string studioPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dev2.Studio.exe");
            //    // We're running local integration tests, so all Studios need to be killed after they've been opened
            //    Process.Start(studioPath);
            //    Process.Start(studioPath);
            //    // Wait for Process to start, else it works in debug, not run
            //    System.Threading.Thread.Sleep(2000);
            //    Process[] processes = Process.GetProcesses().Where(c => c.ProcessName == "Dev2.Studio").ToArray();
            //    expected = 1;
            //    actual = processes.Count(c => !c.ProcessName.Contains("vshost"));
            //    foreach (Process p in processes)
            //    {
            //        p.Kill();
            //    }
            //}

            Assert.AreEqual(expected, actual);
        }
    }
}
