using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data
{
    [TestClass]
    public class IndexListIndexIteratorTests
    {        
        [TestMethod]
        public void IndexListIndexIterator_ShouldHaveConstructor()
        {
            var indexes = new List<int> {1, 2, 3};
            IndexListIndexIterator indexListIndexIterator = new IndexListIndexIterator(indexes);
            Assert.IsNotNull(indexListIndexIterator);
        }
           
        [TestMethod]
        public void IndexListIndexIterator_MaxIndex_ShouldReturnLastIndex()
        {
            var indexes = new List<int> {1, 2, 3};
            IndexListIndexIterator indexListIndexIterator = new IndexListIndexIterator(indexes);
            Assert.IsNotNull(indexListIndexIterator);
            var maxIndex = indexListIndexIterator.MaxIndex();
            Assert.AreEqual(3, maxIndex);
        }

        [TestMethod]
        public void IndexListIndexIterator_HasMore_ShouldReturnTrue()
        {
            var indexes = new List<int> {1, 2, 3};
            var indexListIndexIterator = new IndexListIndexIterator(indexes);
            Assert.IsNotNull(indexListIndexIterator);
            var prObj = new PrivateObject(indexListIndexIterator);
            Assert.IsFalse(indexListIndexIterator.IsEmpty);
            var current = (int) prObj.GetField("_current");
            Assert.IsNotNull(current);
            Assert.AreEqual(0, current);
            Assert.IsTrue(indexListIndexIterator.HasMore());
            var fetchNextIndex = indexListIndexIterator.FetchNextIndex();
            Assert.AreEqual(1, fetchNextIndex);
        }
    }
}
