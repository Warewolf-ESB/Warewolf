using Dev2.Studio.Feedback;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Dev2.Integration.Tests.Dev2.Studio.Core.Tests.Feedback
{
    [TestClass]
    public class FeedbackRecorderTests
    {
        #region Class Members

        private static readonly object _testLock = new object();
        private static string _tempTestFolder;
        private TestContext testContextInstance;

        #endregion Class Members

        #region Properties

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #endregion Properties

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext) 
        {
            _tempTestFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempTestFolder);
        }
        
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup]
        public static void MyClassCleanup() 
        {
            EnsureProcessIsntRunning(false);
            DeleteTempTestFolder();
        }
        
        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize() 
        {
            Monitor.Enter(_testLock);
            EnsureProcessIsntRunning(true);
        }
        
        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup() 
        {
            Monitor.Exit(_testLock);
        }
        
        #endregion

        #region Support Methods

        private static void EnsureProcessIsntRunning(bool waitForExit)
        {
            Process[] processes = Process.GetProcessesByName("psr");

            if (processes.Length == 0)
            {
                return;
            }

            foreach (Process process in processes)
            {
                if (!process.HasExited)
                {
                    process.Kill();

                    if (waitForExit)
                    {
                        process.WaitForExit(10000);
                        if (!process.HasExited)
                        {
                            throw new Exception(
                                "Couldn't exit all pse.exe processes. This step is necessary in order to properly run this test.");
                        }
                    }
                }
            }
        }

        private static string GetUniqueOutputPath()
        {
            return GetUniqueOutputPath(".zip");
        }

        private static string GetUniqueOutputPath(string extension)
        {
            return Path.Combine(_tempTestFolder, Guid.NewGuid().ToString() + extension);
        }

        private static bool CheckIfProcessIsRunning()
        {
            Process[] processes = Process.GetProcessesByName("psr");
            return processes.Length > 0;
        }

        private static void DeleteTempTestFolder()
        {
            try
            {
                Directory.Delete(_tempTestFolder, true);
            }
            catch (Exception)
            {
                //Fail silently if folder couldn't be deleted.
            }
        }

        #endregion Support Methods

        #region Test Methods

        // Brendon.Page, 2013-01-10 - This test needs to be a medium\large because unit test agents don't run in interactive mode, this prevents the
        //                            Problem Steps Recorder from ever becoming ready and in a state were it will record, this in turn
        //                            means that the 'psr.exe /stop' command will never be observed.
        [TestMethod]
        public void StopRecording_Where_PsrIsRunning_Expected_ProcessStops()
        {
            FeedbackRecorder recorder = new FeedbackRecorder();
            string outputPath = GetUniqueOutputPath();

            recorder.StartRecording(outputPath);
            recorder.StopRecording();

            if (CheckIfProcessIsRunning())
            {
                Assert.Fail("Recording process failed to stop!");
            }
        }

        #endregion Test Methods
    }
}

