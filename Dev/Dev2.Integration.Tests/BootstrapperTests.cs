using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class BootstrapperTests
    {
        [TestMethod]
        public void PrepareApplication_With_ExistingApplicationProcessRunning_Expected_ApplicationShutDown()
        {
            string processPath = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location).LocalPath) + "\\Dev2.Studio.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo(processPath);
            startInfo.WorkingDirectory = Path.GetDirectoryName(processPath);
            startInfo.WindowStyle = ProcessWindowStyle.Normal;

            Process process1 = Process.Start(startInfo);
            Process.Start(startInfo);
            Process[] processes = Process.GetProcessesByName(process1.ProcessName);

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
