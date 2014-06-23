using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Tests.BinaryDataList
{
    /// <summary>
    /// Summary description for LoopedIndexIteratorTEst
    /// </summary>
    [TestClass]    
    public class LoopedIndexIteratorTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanIterateNormally()
        {
            const int maxValue = 5;
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
            const int maxValue = 5;
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


        [TestMethod]
        [TestCategory("LoopedIndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("Test to ensure the IsEmpty property works as expected")]
        public void LoopedIndexIterator_UnitTest_CanDetectIsEmptyCorrectly()
        {
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, 0);
            Assert.IsFalse(ii.IsEmpty);
        }

        [TestMethod]
        [TestCategory("LoopedIndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("Test to ensure the MaxIndex works correctly")]
        public void LoopedIndexIterator_UnitTest_ReturnsCorrectMaxIndex()
        {
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, 0);
            Assert.AreEqual(10,ii.MaxIndex());
        }

        [TestMethod]
        [TestCategory("LoopedIndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("Test to ensure the MinIndex works correctly")]
        public void LoopedIndexIterator_UnitTest_ReturnsCorrectMinIndex()
        {
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, 0);
            Assert.AreEqual(10, ii.MinIndex());
        }

        [TestMethod]
        [TestCategory("LoopedIndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("Test to ensure the Count property works correctly")]
        public void LoopedIndexIterator_UnitTest_ReturnsCorrectCount()
        {
            IIndexIterator ii = Dev2BinaryDataListFactory.CreateLoopedIndexIterator(10, 2);
            Assert.AreEqual(2, ii.Count);
        }

        
    }
}
