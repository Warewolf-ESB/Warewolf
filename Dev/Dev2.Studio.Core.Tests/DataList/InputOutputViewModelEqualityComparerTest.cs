
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.DataList
{
    /// <summary>
    /// Summary description for InputOutputViewModelEqualityComparerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class InputOutputViewModelEqualityComparerTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InputOutputViewModelEqualityComparer_Equals")]
        public void InputOutputViewModelEqualityComparer_Equals_WhenEqual_ExpectTrue()
        {
            //------------Setup for test--------------------------
            var inputOutputViewModelEqualityComparer = new InputOutputViewModelEqualityComparer();

            Mock<IInputOutputViewModel> obj1 = new Mock<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> obj2 = new Mock<IInputOutputViewModel>();

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

            Mock<IInputOutputViewModel> obj1 = new Mock<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> obj2 = new Mock<IInputOutputViewModel>();

            obj1.Setup(c => c.DisplayName).Returns("rs().val2");
            obj2.Setup(c => c.DisplayName).Returns("rs().val");

            //------------Execute Test---------------------------
            var result = inputOutputViewModelEqualityComparer.Equals(obj1.Object, obj2.Object);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("InputOutputViewModelEqualityComparer_GetHashCode")]
        public void InputOutputViewModelEqualityComparer_Equals_WhenValidObject_ExpectHashCode()
        {
            //------------Setup for test--------------------------
            var inputOutputViewModelEqualityComparer = new InputOutputViewModelEqualityComparer();

            Mock<IInputOutputViewModel> obj1 = new Mock<IInputOutputViewModel>();
            Mock<IInputOutputViewModel> obj2 = new Mock<IInputOutputViewModel>();

            obj1.Setup(c => c.DisplayName).Returns("rs().val");
            obj2.Setup(c => c.DisplayName).Returns("rs().val");

            //------------Execute Test---------------------------
            var result = inputOutputViewModelEqualityComparer.GetHashCode(obj1.Object);
            var result2 = inputOutputViewModelEqualityComparer.GetHashCode(obj2.Object);

            //------------Assert Results-------------------------
            Assert.AreEqual(253903065, result);
            Assert.AreEqual(result, result2);
        }
    }
}
