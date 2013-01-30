using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Integration.Tests.Load_Tests
{
    /// <summary>
    /// Summary description for LoadTest
    /// </summary>
    [TestClass]
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

            Assert.IsTrue(result.IndexOf(exp) > 0);
            Assert.IsTrue(duration <= 10.0);
        }
    }
}
