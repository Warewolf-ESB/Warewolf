using Dev2.Studio.AppResources.Exceptions;
using Dev2.Studio.Feedback;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Dev2.Core.Tests.Feedback
{
    [TestClass]
    [Ignore]
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
            EnsureProcessIsntRunning(true);
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
                        if (!process.HasExited)
                        {
                            process.WaitForExit(5000);
                        }

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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Construction_Where_FileExistsFunctionIsNull_Expected_ArgumentNullException()
        {
            FeedbackRecorder recorder = new FeedbackRecorder(null);
        }

        [TestMethod]
        public void StartRecording_Where_PsrIsntRunningAndOutputPathValid_Expected_ProcessStarts()
        {
            var recorder = new FeedbackRecorder();
            string outputPath = GetUniqueOutputPath();

            recorder.StartRecording(outputPath);

            if (!CheckIfProcessIsRunning())
            {
                Assert.Fail("Recording process failed to start!");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FeedbackRecordingInprogressException))]
        public void StartRecording_Where_PsrIsRunning_Expected_InvalidOperationException()
        {
            FeedbackRecorder recorder = new FeedbackRecorder();
            string outputPath = GetUniqueOutputPath();

            recorder.StartRecording(outputPath);
            recorder.StartRecording(outputPath);
        }

        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void StartRecording_Where_OutputPathAlreadyExists_Expected_FileIOException()
        {
            //Create file which is to conflict with the output path of the recorder
            FileInfo conflictingPath = new FileInfo(GetUniqueOutputPath());
            conflictingPath.Create().Close();

            FeedbackRecorder recorder = new FeedbackRecorder();
            string outputPath = conflictingPath.FullName;

            recorder.StartRecording(outputPath);
        }

        [TestMethod]
        public void StartRecording_Where_OutputPathDoesntExist_Expected_PathCreated()
        {
            FileInfo tmpOutputPath = new FileInfo(GetUniqueOutputPath());
            string newOutputFolder = Path.Combine(tmpOutputPath.Directory.FullName, Guid.NewGuid().ToString());
            string newOutputpath = Path.Combine(newOutputFolder, tmpOutputPath.Name);

            FeedbackRecorder recorder = new FeedbackRecorder();

            recorder.StartRecording(newOutputpath);
            Assert.AreEqual(Directory.Exists(newOutputFolder), true);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartRecording_Where_OutputPathIsntZipOrXml_Expected_InvalidOperationException()
        {
            string outputPath = GetUniqueOutputPath(".cake");

            FeedbackRecorder recorder = new FeedbackRecorder();
            recorder.StartRecording(outputPath);
        }

        // Brendon.Page, 2013-01-10 - This test needs to be a medium\large test either as a coded UI or in the integration test project.
        //                            The reason for this is because unit test agents don't run in interactive mode, this prevents the
        //                            Problem Steps Recorder from ever becoming ready and in a state were it will record, this in turn
        //                            means that the 'psr.exe /stop' command will never be observed.
        //*********** IF YOU CAN'T FIND AN EQUIVELANT TEST DURING THE REVIEW PLEASE CONTACT ME.
        //[TestMethod]
        //public void StopRecording_Where_PsrIsRunning_Expected_ProcessStops()
        //{
        //    FeedbackRecorder recorder = new FeedbackRecorder();
        //    string outputPath = GetUniqueOutputPath();

        //    recorder.StartRecording(outputPath);
        //    recorder.StopRecording();

        //    if (CheckIfProcessIsRunning())
        //    {
        //        Assert.Fail("Recording process failed to stop!");
        //    }
        //}

        [TestMethod]
        public void StopRecording_Where_PsrIsntRunning_Expected_NoException()
        {
            FeedbackRecorder recorder = new FeedbackRecorder();
            recorder.StopRecording();
        }

        [TestMethod]
        public void IsRecorderAvailable_Where_RecorderAvailable_Expected_True()
        {
            FeedbackRecorder recorder = new FeedbackRecorder(p => true);
            Assert.IsTrue(recorder.IsRecorderAvailable);
        }

        [TestMethod]
        public void IsRecorderAvailable_Where_RecorderAvailable_Expected_False()
        {
            FeedbackRecorder recorder = new FeedbackRecorder(p => false);
            Assert.IsFalse(recorder.IsRecorderAvailable);
        }

        [TestMethod]
        public void KillAllRecordingTasks_Expected_NoRecordinProcessesLeft()
        {
            FeedbackRecorder recorder = new FeedbackRecorder();
            string outputPath = GetUniqueOutputPath();

            recorder.StartRecording(outputPath);
            recorder.KillAllRecordingTasks();

            if (CheckIfProcessIsRunning())
            {
                Assert.Fail("Failed to kill all running recording processes!");
            }
        }

        #endregion Test Methods
    }
}
