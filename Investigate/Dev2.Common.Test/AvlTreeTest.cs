using System.Collections.Generic;
using Dev2.Common.Patterns;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for AvlTreeTest
    /// </summary>
    [TestClass]
    public class AvlTreeTest
    {
        public AvlTreeTest()
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

        /*[TestMethod]
        public void CanAdd()
        {
            AvlTree<string> keyTree = new AvlTree<string>();

            keyTree.Add("abc");

            Assert.IsTrue(keyTree.Contains("abc"));
        }

        [TestMethod]
        public void CanDelete()
        {
            AvlTree<string> keyTree = new AvlTree<string>();

            keyTree.Add("abc");

            keyTree.Delete("abc");

            Assert.IsFalse(keyTree.Contains("abc"));
        }

        [TestMethod]
        public void CanGetValuesCollection()
        {
            AvlTree<string> keyTree = new AvlTree<string>();

            keyTree.Add("abc");

            keyTree.Add("def");

            List<string> vals = new List<string>();

            foreach (var val in keyTree.ValuesCollection)
            {
                vals.Add(val);
            }

            CollectionAssert.AreEqual(new List<string> { "abc", "def" }, vals);
        }*/

        //[TestMethod]
        //public void CanAddBinaryStorageKey()
        //{
        //    AvlTree<BinaryStorageKey> keyTree = new AvlTree<BinaryStorageKey>();

        //    BinaryStorageKey bsk = new BinaryStorageKey() { nsHashCode = 1, pos = 0, uid = Guid.NewGuid() };

        //    keyTree.Add(bsk);

        //    Assert.IsTrue(keyTree.Contains(bsk));
        //}

        //[TestMethod]
        //public void CanAdd200KBinaryStorageKeyUnder1Second()
        //{
        //    AvlTree<BinaryStorageKey> keyTree = new AvlTree<BinaryStorageKey>();

        //    DateTime start = DateTime.Now;

        //    for (int i = 0; i < 200000; i++)
        //    {
        //        BinaryStorageKey bsk = new BinaryStorageKey() { nsHashCode = 1, pos = 0, uid = Guid.NewGuid() };

        //        keyTree.Add(bsk);
        //    }

        //    DateTime end = DateTime.Now;

        //    double dur = end.Ticks - start.Ticks;
        //    double len = (dur / TimeSpan.TicksPerSecond);

        //    Console.WriteLine(len);

        //    Assert.IsTrue(len < 1.0);
        //}

    }
}
