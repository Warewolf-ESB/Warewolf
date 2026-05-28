/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests
{
    [TestClass]
    public class Dev2UniqueActivityComparerTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2UniqueActivityComparer))]
        public void Dev2UniqueActivityComparer_Equals_BothNull_ReturnsTrue()
        {
            var comparer = new Dev2UniqueActivityComparer();

            Assert.IsTrue(comparer.Equals(null, null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2UniqueActivityComparer))]
        public void Dev2UniqueActivityComparer_Equals_OneNull_ReturnsFalse()
        {
            var comparer = new Dev2UniqueActivityComparer();
            var activity = new Mock<IDev2Activity>().Object;

            Assert.IsFalse(comparer.Equals(activity, null));
            Assert.IsFalse(comparer.Equals(null, activity));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2UniqueActivityComparer))]
        public void Dev2UniqueActivityComparer_Equals_MatchingUniqueIds_ReturnsTrue()
        {
            var comparer = new Dev2UniqueActivityComparer();
            var x = new Mock<IDev2Activity>();
            var y = new Mock<IDev2Activity>();
            x.SetupGet(a => a.UniqueID).Returns("shared-id");
            y.SetupGet(a => a.UniqueID).Returns("shared-id");

            Assert.IsTrue(comparer.Equals(x.Object, y.Object));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2UniqueActivityComparer))]
        public void Dev2UniqueActivityComparer_Equals_DifferentUniqueIds_ReturnsFalse()
        {
            var comparer = new Dev2UniqueActivityComparer();
            var x = new Mock<IDev2Activity>();
            var y = new Mock<IDev2Activity>();
            x.SetupGet(a => a.UniqueID).Returns("a");
            y.SetupGet(a => a.UniqueID).Returns("b");

            Assert.IsFalse(comparer.Equals(x.Object, y.Object));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2UniqueActivityComparer))]
        public void Dev2UniqueActivityComparer_GetHashCode_DelegatesToInstance()
        {
            var comparer = new Dev2UniqueActivityComparer();
            var activity = new Mock<IDev2Activity>().Object;

            Assert.AreEqual(activity.GetHashCode(), comparer.GetHashCode(activity));
        }
    }
}
