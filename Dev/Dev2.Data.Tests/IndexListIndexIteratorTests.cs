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
            IndexListIndexIterator indexListIndexIterator = new IndexListIndexIterator(indexes);
            Assert.IsNotNull(indexListIndexIterator);
            var prObj = new PrivateObject(indexListIndexIterator);
            Assert.IsFalse(indexListIndexIterator.IsEmpty);
            prObj.SetField("_enumerator", 2);
            Assert.IsTrue(indexListIndexIterator.HasMore());
            prObj.SetField("_enumerator", 3);
            Assert.IsFalse(indexListIndexIterator.HasMore());
        }
    }
}
