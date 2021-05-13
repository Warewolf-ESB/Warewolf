/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.DataList
{
    [TestClass]
	[TestCategory("Studio Datalist Core")]
    public class InputOutputViewModelEqualityComparerTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InputOutputViewModelEqualityComparer_Equals")]
        public void InputOutputViewModelEqualityComparer_Equals_WhenEqual_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var inputOutputViewModelEqualityComparer = new InputOutputViewModelEqualityComparer();

            var obj1 = new Mock<IInputOutputViewModel>();
            var obj2 = new Mock<IInputOutputViewModel>();

            obj1.Setup(c => c.DisplayName).Returns("rs().val");
            obj2.Setup(c => c.DisplayName).Returns("rs().val");

            //------------Execute Test---------------------------
            var result = inputOutputViewModelEqualityComparer.Equals(obj1.Object, obj2.Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InputOutputViewModelEqualityComparer_Equals")]
        public void InputOutputViewModelEqualityComparer_Equals_WhenNotEqual_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var inputOutputViewModelEqualityComparer = new InputOutputViewModelEqualityComparer();

            var obj1 = new Mock<IInputOutputViewModel>();
            var obj2 = new Mock<IInputOutputViewModel>();

            obj1.Setup(c => c.DisplayName).Returns("rs().val2");
            obj2.Setup(c => c.DisplayName).Returns("rs().val");

            //------------Execute Test---------------------------
            var result = inputOutputViewModelEqualityComparer.Equals(obj1.Object, obj2.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }
    }
}
