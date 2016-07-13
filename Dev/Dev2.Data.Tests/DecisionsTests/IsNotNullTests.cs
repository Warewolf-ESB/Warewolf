using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotNullTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotNull_Invoke")]
        public void GivenSomeString_IsNotNull_Invoke_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotNull = new IsNotNull();
            string[] cols = new string[2];
            cols[0] = "Eight";
            //------------Execute Test---------------------------
            bool result = isNotNull.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            //------------Execute Test---------------------------
            result = isNotNull.Invoke(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotNull_Invoke")]
        public void IsNotNull_Invoke_IsNotNull_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsNotNull();
            string[] cols = new string[2];
            cols[0] = null;
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotNull_HandlesType")]
        public void IsNotNull_HandlesType_ReturnsIsNotNullType()
        {
            var decisionType = enDecisionType.IsNotNull;
            //------------Setup for test--------------------------
            var isNotNull = new IsNotNull();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotNull.HandlesType());
        }
    }
}