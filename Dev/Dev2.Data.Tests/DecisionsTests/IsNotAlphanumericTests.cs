using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotAlphanumericTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsNotAlphanumeric_Invoke")]
        public void IsNotAlphanumeric_Invoke_DoesEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotAlphanumeric = new IsNotAlphanumeric();
            var cols = new string[2];
            cols[0] = "'";
            //------------Execute Test---------------------------
            var result = isNotAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsNotAlphanumeric_Invoke")]
        public void IsNotAlphanumeric_Invoke_DoesntEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotAlphanumeric = new IsNotAlphanumeric();
            var cols = new string[2];
            cols[0] = "TestData";
            //------------Execute Test---------------------------
            var result = isNotAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotAlphanumeric_HandlesType")]
        public void IsNotAlphanumeric_HandlesType_ReturnsIsNotAlphanumericType()
        {
            var expected = enDecisionType.IsNotAlphanumeric;
            //------------Setup for test--------------------------
            var IsNotAlphanumeric = new IsNotAlphanumeric();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, IsNotAlphanumeric.HandlesType());
        }
    }
}
