using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            /*string processPath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location).LocalPath) + "\\Dev2.Studio.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo(processPath);
            startInfo.WorkingDirectory = Path.GetDirectoryName(processPath);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            
            Process process1 = Process.Start(startInfo);
            */
            Process.Start("Dev2.Studio.exe");
            Process.Start("Dev2.Studio.exe");
            Process[] processes = Process.GetProcessesByName("Dev2.Studio");

            int actual = processes.Length;
            int expected = 1;

            foreach (Process process in processes)
            {
                process.Kill();
            }

            Assert.AreEqual(expected, actual);
        }
    }
}
