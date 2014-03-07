using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IndexListTest
    {
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can init properly")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanInitNormally()
        {

            IndexList il = new IndexList(null, 5);

            Assert.AreEqual(1, il.MinValue);
            Assert.AreEqual(5, il.MaxValue);
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can init properly")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanInitWithGaps()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 3 };
            IndexList il = new IndexList(gaps, 5);

            Assert.AreEqual(1, il.MinValue);
            Assert.AreEqual(5, il.MaxValue);
            Assert.AreEqual(3, il.Count());
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can set max value when it is in the gaps collection ;)")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanSetMaxValueWhenInGaps()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 5 };
            IndexList il = new IndexList(gaps, 5);

            il.SetMaxValue(5, false);

            Assert.AreEqual(1, il.MinValue);
            Assert.AreEqual(5, il.MaxValue);
            Assert.AreEqual(3, il.Count());
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can count correctly when the min value is not 1")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanCountCorrectlyWhenMinValueGreaterThan1()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 5 };
            IndexList il = new IndexList(gaps, 4, 3);

            Assert.AreEqual(3, il.MinValue);
            Assert.AreEqual(4, il.MaxValue);
            Assert.AreEqual(1, il.Count());
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can detect what is in and out of its collection ;)")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_ContainsOperatesAsExpected()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 5 };
            IndexList il = new IndexList(gaps, 5);

            Assert.IsFalse(il.Contains(1));
            Assert.IsTrue(il.Contains(4));
        }
    }
}
