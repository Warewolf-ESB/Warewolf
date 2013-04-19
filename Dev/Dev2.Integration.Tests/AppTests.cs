using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Dev2.Integration.Tests
{
    [TestClass]
    [Ignore]
    public class AppTests
    {
        // Fixed by Michael RE Broken Integration Tests (12th Feb 2013)
        // Fixed by Brendon.Page, the test now uses a mutex check to decide if a first process needs to be started.
        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            List<Process> processesToTryKill = new List<Process>();
            string studioPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dev2.Studio.exe");

            // Check if there is already a studio running, in debug or otherwise
            bool studioAlreadyRunning = false;
            try
            {
                Mutex.OpenExisting("Dev2.Studio");
                studioAlreadyRunning = true;
            }
            catch (Exception)
            {
            }

            // If there isn't a studio running start one
            if (!studioAlreadyRunning)
            {
                Process firstProcess = Process.Start(studioPath);
                processesToTryKill.Add(firstProcess);

                // Wait for Process to start, and get past the check for a duplicate process
                Thread.Sleep(2000);
            }

            // Start a second studio, this should hit the logic that checks for a duplicate and exit
            Process secondProcess = Process.Start(studioPath);
            processesToTryKill.Add(secondProcess);

            // Wait for Process to start, check for a duplicate process and exit
            Thread.Sleep(2000);

            // Gather actual
            bool actual = secondProcess.HasExited;

            // Clean up and processes that were started for this test
            foreach (Process p in processesToTryKill)
            {
                if (!p.HasExited)
                {
                    p.Kill();
                }
            }

            //Assert.AreEqual(true, actual);

            Assert.Inconclusive("Not a sane Integration Test!!!");
        }
    }
}
