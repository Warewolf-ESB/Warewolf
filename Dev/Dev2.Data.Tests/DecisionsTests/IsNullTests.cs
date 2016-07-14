using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNullTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNull_Invoke")]
        public void GivenSomeString_IsNull_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNull = new IsNull();
            string[] cols = new string[2];
            cols[0] = "Eight";
            //------------Execute Test---------------------------
            bool result = isNull.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            //------------Execute Test---------------------------
            result = isNull.Invoke(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNull_Invoke")]
        public void IsNull_Invoke_IsNull_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsNull();
            string[] cols = new string[2];
            cols[0] = null;
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNull_HandlesType")]
        public void IsNull_HandlesType_ReturnsIsNullType()
        {
            var decisionType = enDecisionType.IsNull;
            //------------Setup for test--------------------------
            var isNull = new IsNull();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNull.HandlesType());
        }
    }
}