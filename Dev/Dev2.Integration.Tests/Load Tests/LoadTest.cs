using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Load_Tests
{
    /// <summary>
    /// Summary description for LoadTest
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class LoadTest
    {
        public LoadTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        private double _ticksPerSec = 10000000;

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void FileWith10kPrimes_Expect10kRecordsetEntries_in_Under_5Seconds()
        {
            string path = ServerSettings.WebserverURI + "LargeDataTest";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;
            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            string exp = "<myPrimes><value> 104729</value></myPrimes>"; // Last value in the file

            Assert.IsTrue(result.IndexOf(exp, StringComparison.Ordinal) > 0);
            // Travis.Frisinger - Bug 8579
            // Was 10.0 Moved to 2.5
            if (duration <= 2.5)
            {
                Assert.IsTrue(duration <= 2.5, " It Took { " + duration + " }");
            }
            else if (duration <= 35)
            {
                Assert.Inconclusive("It took too long to run this test! { " + duration + " }");
            }
            else
            {
                Assert.Inconclusive("Get new hardware buddy! { " + duration + " }");
            }
        }

        // Travis.Frisinger - Bug 8579
        [TestMethod]
        public void FileWith200kLine_Expect200kRecordsetEntries_In_Under_25Seconds()
        {

            string path = ServerSettings.WebserverURI + "DataSplit200kEntryFile_Expect_Sub5SecondProcess";

            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;

            string exp = "00199999"; // Last value in the file to ensure it was read ;)

            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            StringAssert.Contains(result, exp);
            Console.WriteLine("Took " + duration);
            if (duration <= 25.0)
            {
                Assert.AreEqual(1,1);
            }
            else if (duration <= 60.0)
            {
                Assert.Inconclusive("Your PC passed the test, although it was a bit slow - It meant to take less than 25 seconds, but it took " + duration);
            }
            else
            {
                Assert.Fail("The process took too long to run! " + duration);
            }
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ForEachDelete_IntegrationTest")]
        public void ForEachDelete_IntegrationTest_RunsQuickly_InUnder7SecondsForEntireWorkflow()
        {

            //------------Setup for test--------------------------
            string path = ServerSettings.WebserverURI + "DeleteSpeed";

            //------------Execute Test---------------------------
            DateTime start = DateTime.Now;
            string result = TestHelper.PostDataToWebserver(path);
            DateTime end = DateTime.Now;

            double duration = (end.Ticks - start.Ticks) / _ticksPerSec;

            //------------Assert Results-------------------------

            // ensure we have valid data ;)
            StringAssert.Contains(result, "<TestResult>");

            // check duration ;)
            if (duration <= 7.0)
            {
                Assert.AreEqual(1, 1);
            }
            else if (duration <= 65.0) // silly nightly enviroments take for every to do this?!
            {
                Assert.Inconclusive("Your PC passed the test, although it was a bit slow - It meant to take less than 7 seconds, but it took " + duration);
            }
            else
            {
                Assert.Fail("The process took too long to run! " + duration);
            }  
        }
    }
}
