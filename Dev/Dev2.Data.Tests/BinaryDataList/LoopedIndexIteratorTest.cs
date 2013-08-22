using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Tests.Runtime.BinaryDataList
{
    /// <summary>
    /// Summary description for LoopedIndexIteratorTEst
    /// </summary>
    [TestClass]
    public class LoopedIndexIteratorTest
    {
        public LoopedIndexIteratorTest()
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
        public void CanIterateNormally()
        {
            int maxValue = 5;
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, maxValue);
            int cnt = 0;
            while (ii.HasMore())
            {
                ii.FetchNextIndex();
                cnt++;
            }

            Assert.AreEqual(maxValue, cnt);
        }

        [TestMethod]
        public void CanIterateSameValue()
        {
            int maxValue = 5;
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, maxValue);
            int cnt = 0;
            int sum = 0;
            while (ii.HasMore())
            {
                sum += ii.FetchNextIndex();
                cnt++;
            }

            Assert.AreEqual(maxValue, cnt);
            Assert.AreEqual(50, sum);
        }

        
    }
}
