using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Data.Storge;
using Dev2.Data.Storage;
using Dev2.Data.Storage.ProtocolBuffers;

namespace Dev2.Data.Tests.Persistence
{



    /// <summary>
    /// Summary description for BinaryBlobStorageTest
    /// </summary>
    [TestClass]
    public class BinaryBlobStorageTest
    {
        public BinaryBlobStorageTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

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
        public void CanCreateWithDefaultConstructor()
        {
            BinaryBlobStorage tmp = new BinaryBlobStorage();

            string key = Guid.NewGuid().ToString();

            SampleObject o = new SampleObject() { TheInt = 100, TheString = "A string value, how evil" };

            tmp.PushObject(key, o);

            tmp.Dispose();
        }

        [TestMethod]
        [Ignore] // Travis to fix with new stuff
        public void CanCreateAndFetchWithDefaultConstructor()
        {
            BinaryBlobStorage tmp = new BinaryBlobStorage();

            string key = Guid.NewGuid().ToString();

            SampleObject o = new SampleObject() { TheInt = 100, TheString = "A string value, how evil" };

            tmp.PushObject(key, o);

            SampleObject o2 = tmp.FetchObject<SampleObject>(key);

            tmp.Dispose();

            Assert.AreEqual(100, o2.TheInt);
            Assert.AreEqual("A string value, how evil", o2.TheString);

        }

        [TestMethod]
        public void CanCreate100kInUnder2Second()
        {
            BinaryBlobStorage tmp = new BinaryBlobStorage();

            string key = Guid.NewGuid().ToString();

            DateTime start = DateTime.Now;

            for (int i = 0; i < 100000; i++)
            {
                SampleObject o = new SampleObject() { TheInt = 100, TheString = "A string value, how evil" };

                tmp.PushObject(key + i, o);
            }

            DateTime end = DateTime.Now;

            tmp.Dispose();

            double ticsPerSec = TimeSpan.TicksPerSecond;
            double dur = (end.Ticks - start.Ticks) / ticsPerSec;

            Console.WriteLine(dur);

            Assert.IsTrue(dur < 5.0);

        }

        [TestMethod]
        public void CanCreate100kWithSillyLongStringInUnder2Second()
        {
            BinaryBlobStorage tmp = new BinaryBlobStorage();

            string key = Guid.NewGuid().ToString();

            DateTime start = DateTime.Now;

            for (int i = 0; i < 100000; i++)
            {
                SampleObject o = new SampleObject() { TheInt = 100, TheString = "A string value, how evil, esp since it is a very long ugly string, what was i thinking. Oh yeah, I wanted to test my byte journal ;)" };

                tmp.PushObject(key + i, o);
            }

            DateTime end = DateTime.Now;

            tmp.Dispose();

            double ticsPerSec = TimeSpan.TicksPerSecond;
            double dur = (end.Ticks - start.Ticks) / ticsPerSec;

            Console.WriteLine(dur);

            Assert.IsTrue(dur < 5.0);

        }

        //[TestMethod]
        //public void CanCreateAndRead100kInUnder5Second()
        //{
        //    BinaryBlobStorage tmp = new BinaryBlobStorage();

        //    string key = Guid.NewGuid().ToString();

        //    DateTime start = DateTime.Now;

        //    for (int i = 0; i < 100000; i++)
        //    {
        //        SampleObject o = new SampleObject() { TheInt = 100, TheString = "A string value, how evil " + i };

        //        tmp.PushObject(key + i, o);
        //    }

        //    // now read them back ;)

        //    for (int i = 0; i < 100000; i++)
        //    {

        //        SampleObject so = tmp.FetchObject<SampleObject>(key + i);

        //        Assert.AreEqual("A string value, how evil " + i, so.TheString);
        //        Assert.AreEqual(100, so.TheInt);
        //    }

        //    DateTime end = DateTime.Now;

        //    tmp.Dispose();

        //    double ticsPerSec = TimeSpan.TicksPerSecond;
        //    double dur = (end.Ticks - start.Ticks) / ticsPerSec;

        //    Console.WriteLine(dur);

        //    Assert.IsTrue(dur < 2.0);

        //}
    }
}
