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
    public class AppTests
    {
        // Fixed by Michael RE Broken Integration Tests (12th Feb 2013)
        // Fixed by Brendon.Page, the test now uses a mutex check to decide if a first process needs to be started.
        [TestMethod]
        public void PrepareApplication_With_ExistingApplication_Expect_OnlyOneApplication()
        {
            List<Process> processesToTryKill = new List<Process>();
            string studioPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Warewolf Studio.exe");

            // Check if there is already a studio running, in debug or otherwise
            bool studioAlreadyRunning = false;
            try
            {
                Mutex.OpenExisting("Warewolf Studio");
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
                Thread.Sleep(20000);
            }

            // Start a second studio, this should hit the logic that checks for a duplicate and exit
            Process secondProcess = Process.Start(studioPath);
            processesToTryKill.Add(secondProcess);

            // Gather actual
            bool actual = secondProcess.WaitForExit(60000);
            
            // Clean up and processes that were started for this test
            foreach (Process p in processesToTryKill)
            {
                try
                {
                    if (!p.HasExited)
                    {
                        p.Kill();
                    }
                }
                catch
                {
                    // Just be a good boy ;)
                }
            }

            Assert.AreEqual(true, actual, "Failed to kill second studio!");
        }
    }
}
