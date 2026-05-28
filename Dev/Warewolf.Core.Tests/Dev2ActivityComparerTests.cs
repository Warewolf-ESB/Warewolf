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
    public class Dev2ActivityComparerTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2ActivityComparer))]
        public void Dev2ActivityComparer_Equals_BothNull_ReturnsTrue()
        {
            var comparer = new Dev2ActivityComparer();

            Assert.IsTrue(comparer.Equals(null, null));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2ActivityComparer))]
        public void Dev2ActivityComparer_Equals_OneNull_ReturnsFalse()
        {
            var comparer = new Dev2ActivityComparer();
            var activity = new Mock<IDev2Activity>().Object;

            Assert.IsFalse(comparer.Equals(activity, null));
            Assert.IsFalse(comparer.Equals(null, activity));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2ActivityComparer))]
        public void Dev2ActivityComparer_Equals_BothNonNull_DelegatesToInstanceEquals()
        {
            var comparer = new Dev2ActivityComparer();
            var x = new Mock<IDev2Activity>();
            var y = new Mock<IDev2Activity>();
            x.Setup(a => a.Equals(It.IsAny<IDev2Activity>())).Returns(true);

            Assert.IsTrue(comparer.Equals(x.Object, y.Object));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(Dev2ActivityComparer))]
        public void Dev2ActivityComparer_GetHashCode_AlwaysOne()
        {
            var comparer = new Dev2ActivityComparer();

            Assert.AreEqual(1, comparer.GetHashCode(new Mock<IDev2Activity>().Object));
        }
    }
}
