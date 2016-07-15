using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data
{
    [TestClass]
    public class EnvironmentIndexIteratorTests
    {
        [TestMethod]
        public void EnvironmentIndexIterator_ShouldHaveConstructor()
        {
            var indexes = new List<int> { 1, 2, 3 };
            EnvironmentIndexIterator environmentIndexIterator = new EnvironmentIndexIterator(indexes);
            Assert.IsNotNull(environmentIndexIterator);
        }

        [TestMethod]
        public void EnvironmentIndexIterator_MaxIndex_ShouldReturnLastIndex()
        {
            var indexes = new List<int> { 1, 2, 3 };
            EnvironmentIndexIterator environmentIndexIterator = new EnvironmentIndexIterator(indexes);
            Assert.IsNotNull(environmentIndexIterator);
            var maxIndex = environmentIndexIterator.MaxIndex();
            Assert.AreEqual(3, maxIndex);
        }

        [TestMethod]
        public void EnvironmentIndexIterator_HasMore_ShouldReturnTrue()
        {
            var indexes = new List<int> { 1, 2, 3 };
            EnvironmentIndexIterator environmentIndexIterator = new EnvironmentIndexIterator(indexes);
            Assert.IsNotNull(environmentIndexIterator);
            var prObj = new PrivateObject(environmentIndexIterator);
            Assert.IsFalse(environmentIndexIterator.IsEmpty);
            var enumerator = prObj.GetField("_enumerator") as IEnumerator<int>;
            Assert.IsNotNull(enumerator);
            var current = enumerator.Current;
            Assert.AreEqual(0, current);
            Assert.IsTrue(environmentIndexIterator.HasMore());
            var fetchNextIndex = environmentIndexIterator.FetchNextIndex();
            Assert.AreEqual(1, fetchNextIndex);
        }
    }
}
