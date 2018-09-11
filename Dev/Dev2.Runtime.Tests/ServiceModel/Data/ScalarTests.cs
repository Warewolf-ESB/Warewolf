using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class ScalarTests
    {
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void ConstructorWithNoParametersExpectedInitializesListProperties()
        {
            var scalar = new Scalar();
            Assert.IsNotNull(scalar);
            Assert.IsFalse(scalar.IsEditable);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Scalar_GetHashCode_ShouldReturnHashCode()
        {
            var scalar = new Scalar {Name = "MyScalar"};
            var hashCode = scalar.GetHashCode();
            Assert.IsNotNull(hashCode);
        }


        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void GivenDifferemtScalar_Scalar_Equals_ShouldReturnFalse()
        {
            var scalar = new Scalar {Name = "MyScalar"};
            var other = new Scalar();
            var scalarEqual = scalar.Equals(other);
            Assert.IsFalse(scalarEqual);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void GivenDifferemtScalar_Scalar_EqualsOperator_ShouldReturnFalse()
        {
            var scalar = new Scalar {Name = "MyScalar"};
            var other = new Scalar();
            Assert.IsFalse(scalar == other);
            Assert.IsTrue(scalar != other);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void GivenNullScalar_Scalar_EqualsOperator_ShouldReturnFalse()
        {
            var scalar = new Scalar { Name = "MyScalar" };
            var other = new Scalar();
            var equals = Scalar.Comparer.Equals(scalar, other);
            Assert.IsFalse(equals);
        }
    }
}