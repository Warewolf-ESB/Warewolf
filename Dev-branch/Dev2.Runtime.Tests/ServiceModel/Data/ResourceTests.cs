using System;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class ResourceTests
    {
        #region Equals

        [TestMethod]
        public void EqualsWithSameItemKeyExpectedReturnsTrue()
        {
            var key = new Resource();
            var result = key.Equals(key);
            Assert.IsTrue(result);
            result = key.Equals((object)key);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void EqualsWithNullExpectedReturnsFalse()
        {
            var key = new Resource();
            var result = key.Equals(null);
            Assert.IsFalse(result);
            result = key.Equals((object)null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsWithDifferentItemKeyHavingSamePropertiesExpectedReturnsTrue()
        {
            var key = new Resource();
            var other = new Resource();
            var result = key.Equals(other);
            Assert.IsTrue(result);
            result = key.Equals((object)other);
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void EqualsWithDifferentItemKeyHavingDifferentPropertiesExpectedReturnsFalse()
        {
            var key = new Resource();
            var other = new Resource { ResourceID = Guid.NewGuid() };
            var result = key.Equals(other);
            Assert.IsFalse(result);
            result = key.Equals((object)other);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void EqualsWithObjectExpectedReturnsFalse()
        {
            var key = new Resource();
            var result = key.Equals(new object());
            Assert.IsFalse(result);
        }

        #endregion

    }
}
