using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Tests.Diagnostics
{
    [TestClass]
    public class DebugItemTests
    {
        #region Constructor

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_With_NullArray_Expected_ThrowsArgumentNullException()
        {
            var item = new DebugItem(null);
        }

        [TestMethod]
        public void Constructor_With_Array_Expected_InitializesWithArray()
        {
            var result = new DebugItemResult { GroupName = "Hello", Value = "world" };
            var item = new DebugItem { result };
            Assert.AreEqual(1, item.Count);
            Assert.AreSame(result, item[0]);
        }

        #endregion

        #region Contains

        [TestMethod]
        public void Contains_With_NullFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem
            {
                new DebugItemResult { GroupName = "Hello", Value = "world"}
            };
            var result = item.Contains(null);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_With_EmptyFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem
            {
                new DebugItemResult { GroupName = "Hello", Value = "world"}
            };
            var result = item.Contains(string.Empty);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_With_ValidFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem
            {
                new DebugItemResult { GroupName = "Hello", Value = "world"}
            };
            var result = item.Contains("world");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_With_InvalidFilter_Expected_ReturnsInstance()
        {
            var item = new DebugItem
            {
                new DebugItemResult { GroupName = "Hello", Value = "world"}
            };
            var result = item.Contains("the");
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void Contains_With_Filter_Expected_IsCaseInsensitive()
        {
            var item = new DebugItem
            {
                new DebugItemResult { GroupName = "Hello", Value = "world"}
            };
            var result = item.Contains("hel");
            Assert.IsTrue(result);
        }

        #endregion

    }
}
