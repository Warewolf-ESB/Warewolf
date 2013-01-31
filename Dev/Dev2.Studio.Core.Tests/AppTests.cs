using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        public void AppConstructor_With_AppAlreadyRunning_Expected_OneAppInProcesses()
        {
            Process.Start(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location).LocalPath) + "\\Dev2.Studio.exe");
            Process.Start(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().Location).LocalPath) + "\\Dev2.Studio.exe");
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1) Assert.Fail();
            Process.GetProcessesByName("Dev2.Studio")[0].Kill();
        }
    }
}
