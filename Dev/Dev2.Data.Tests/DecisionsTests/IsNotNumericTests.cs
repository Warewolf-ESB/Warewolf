using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotNumericTests
    {

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotNumeric_Invoke")]
        public void GivenSomeString_IsNotNumeric_Invoke_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotNumeric = new IsNotNumeric();
            string[] cols = new string[2];
            cols[0] = "Eight";
            //------------Execute Test---------------------------
            bool result = isNotNumeric.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotNumeric_Invoke")]
        public void IsNotNumeric_Invoke_IsNotNumeric_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsNotNumeric();
            string[] cols = new string[2];
            cols[0] = "324";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotNumeric_HandlesType")]
        public void IsNotNumeric_HandlesType_ReturnsIsNotNumericType()
        {
            var decisionType = enDecisionType.IsNotNumeric;
            //------------Setup for test--------------------------
            var isNotNumeric = new IsNotNumeric();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotNumeric.HandlesType());
        }
    }
}
