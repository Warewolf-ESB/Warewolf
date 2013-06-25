using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Dev2.Data.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for BinaryDataListIndexStorageTEst
    /// </summary>
    [TestClass]
    public class BinaryDataListIndexStorageTest
    {
        public BinaryDataListIndexStorageTest()
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
        public void CanAddIndex()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);

            Assert.AreEqual(1, bdlis.Count, "Cannot add to Index Storage");
        }

        [TestMethod]
        public void CanAddIndexWithUpdate()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);

            bdlis.AddIndex(key, 10, 110);

            long position;
            int length;

            bdlis.GetPositionLength(key, out position, out length);

            Assert.AreEqual(1, bdlis.Count, "Cannot add to Index Storage");
            Assert.AreEqual(10, position);
            Assert.AreEqual(110, length);
        }

        [TestMethod]
        public void CanAddIndexWithUpdateAfterRemove()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);

            string key2 = Guid.NewGuid().ToString();

            bdlis.AddIndex(key2, 100, 10);

            // update the 1st key
            bdlis.AddIndex(key, 10, 110);

            long position;
            int length;

            bdlis.GetPositionLength(key, out position, out length);

            Assert.AreEqual(2, bdlis.Count, "Cannot add to Index Storage");
            Assert.AreEqual(10, position);
            Assert.AreEqual(110, length);
        }

        [TestMethod]
        public void CanRemoveIndex()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);

            bdlis.RemoveIndex(key);

            Assert.AreEqual(0, bdlis.Count, "Cannot Remove From Index Storage");
        }

        [TestMethod]
        public void CanFetchAllKeys()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());


            for (int i = 0; i < 10; i++)
            {
                string key = Guid.NewGuid().ToString();

                bdlis.AddIndex(key, 0, 100);
            }

            ICollection<string> tmp = bdlis.Keys;

            Assert.AreEqual(10, tmp.Count, "Index Storage Keys is Faulty");
        }

        [TestMethod]
        public void CanContainKey()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);


            Assert.IsTrue(bdlis.ContainsKey(key), "Index Storage Contains Key is Faulty");
        }


        [TestMethod]
        public void CanGetPositionAndLength()
        {
            BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

            string key = Guid.NewGuid().ToString();

            bdlis.AddIndex(key, 0, 100);

            long position;
            int len;

            bdlis.GetPositionLength(key, out position, out len);

            Assert.AreEqual(0, position, "Index Storage PositionLength Faulty");
            Assert.AreEqual(100, len, "Index Storage PositionLength Faulty");
        }


        //[TestMethod]
        //public void CompactWillTrigger()
        //{
        //    BinaryDataListIndexStorage bdlis = new BinaryDataListIndexStorage(Guid.NewGuid().ToString());

        //    IList<string> keys = new List<string>(10000);

        //    for (int i = 0; i < 100000; i++)
        //    {
        //        string key = Guid.NewGuid().ToString();

        //        // select keys to delete ;)
        //        if ((i % 10) == 0)
        //        {
        //            keys.Add(key);
        //        }

        //        bdlis.AddIndex(key, 0, 100);
        //    }


        //    Assert.AreEqual(100000, bdlis.Count);

        //    foreach (string k in keys)
        //    {
        //        bdlis.RemoveIndex(k);
        //    }

        //    Assert.AreEqual(90000, bdlis.Count);
        //}
    }
}
