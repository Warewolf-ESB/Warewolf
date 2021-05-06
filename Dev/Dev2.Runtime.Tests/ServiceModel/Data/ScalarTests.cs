/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Data;
using Dev2.Data.Interfaces.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class ScalarTests
    {
        [TestMethod]
        public void ConstructorWithNoParametersExpectedInitializesListProperties()
        {
            var scalar = new Scalar();
            Assert.IsNotNull(scalar);
            Assert.IsFalse(scalar.IsEditable);
        }

        [TestMethod]
        public void Scalar_GetHashCode_ShouldReturnHashCode()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var hashCode = scalar.GetHashCode();
            Assert.IsNotNull(hashCode);
        }


        [TestMethod]
        public void GivenDifferemtScalar_Scalar_Equals_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var other = new Scalar();
            var scalarEqual = scalar.Equals(other);
            Assert.IsFalse(scalarEqual);
        }

        [TestMethod]
        public void GivenDifferemtScalar_Scalar_EqualsOperator_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var other = new Scalar();
            Assert.IsFalse(scalar == other);
            Assert.IsTrue(scalar != other);
        }
        [TestMethod]
        public void GivenNullScalar_Scalar_EqualsOperator_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var other = new Scalar();
            var equals = Scalar.Comparer.Equals(scalar, other);
            Assert.IsFalse(equals);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_SameObject_ShouldReturnTrue()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var equals = Scalar.Comparer.Equals(scalar, scalar);
            Assert.IsTrue(equals);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_XisNull_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var equals = Scalar.Comparer.Equals(null, scalar);
            Assert.IsFalse(equals);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_YisNull_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var equals = Scalar.Comparer.Equals(scalar, null);
            Assert.IsFalse(equals);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_GetHashCode_ShouldReturnHashCode()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var expected = scalar.GetHashCode();
            var actual = Scalar.Comparer.GetHashCode(scalar);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_GetHashCode_ShouldReturn0()
        {
            var scalar = new Scalar { };
            var expected = scalar.GetHashCode();
            var actual = Scalar.Comparer.GetHashCode(scalar);
            Assert.AreEqual(0, expected);
            Assert.AreEqual(0, actual);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_ComparerEqualityComparer_SetIODirection()
        {
            enDev2ColumnArgumentDirection columnDirection;
            columnDirection = enDev2ColumnArgumentDirection.None;
            var scalar = new Scalar { Name = "MyScalar", Description = "MyScalar description", IODirection = columnDirection, IsEditable = true, Value = "Value" };

            Assert.AreEqual(columnDirection, scalar.IODirection);
            Assert.AreEqual("MyScalar", scalar.Name);
            Assert.AreEqual("MyScalar description", scalar.Description);
            Assert.AreEqual(true, scalar.IsEditable);
            Assert.AreEqual("Value", scalar.Value);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_GivenDifferentObjectType_Equals_ShouldReturnFalse()
        {
            var scalar = new Scalar { };
            var other = new object();
            var scalarEqual = scalar.Equals(other);
            Assert.IsFalse(scalarEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_GivenNullObject_Equals_ShouldReturnFalse()
        {
            object scalar = new Scalar { };
            object scalarOther = null;
            var scalarEqual = scalar.Equals(scalarOther);
            Assert.IsFalse(scalarEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_GivenObject_Equals_ShouldReturnTrue()
        {
            object scalar = new Scalar { };
            object scalarOther = new Scalar { };
            var scalarEqual = scalar.Equals(scalarOther);
            Assert.IsTrue(scalarEqual);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [Owner("Candice Daniel")]
        [TestCategory("Scalar")]
        public void Scalar_GivenObjectScalar_Equals_ShouldReturnTrue()
        {
            object scalar = new Scalar { };
            object scalarOther = scalar;
            var scalarEqual = scalar.Equals(scalarOther);
            Assert.IsTrue(scalarEqual);
        }
    }
}