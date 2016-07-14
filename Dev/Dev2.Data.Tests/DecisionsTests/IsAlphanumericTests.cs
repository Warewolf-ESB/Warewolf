using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsAlphanumericTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsAlphanumeric_Invoke")]
        public void IsAlphanumeric_Invoke_DoesEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isAlphanumeric = new IsAlphanumeric();
            string[] cols = new string[2];
            cols[0] = "'";
            //------------Execute Test---------------------------
            bool result = isAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsAlphanumeric_Invoke")]
        public void IsAlphanumeric_Invoke_DoesntEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isAlphanumeric = new IsAlphanumeric();
            string[] cols = new string[2];
            cols[0] = "TestData";
            //------------Execute Test---------------------------
            bool result = isAlphanumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            result = isAlphanumeric.Invoke(new[] { string.Empty});
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsAlphanumeric_HandlesType")]
        public void IsAlphanumeric_HandlesType_ReturnsIsAlphanumericType()
        {
            var expected = enDecisionType.IsAlphanumeric;
            //------------Setup for test--------------------------
            var isAlphanumeric = new IsAlphanumeric();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isAlphanumeric.HandlesType());
        }
    }
}
