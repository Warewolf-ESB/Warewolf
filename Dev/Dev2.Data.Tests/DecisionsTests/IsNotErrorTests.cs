using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotErrorTests
    {

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotError_Invoke")]
        public void GivenSomeString_IsNotError_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotError = new IsNotError();
            string[] cols = new string[2];
            cols[0] = "Eight";
            //------------Execute Test---------------------------
            bool result = isNotError.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotError_Invoke")]
        public void IsNotError_Invoke_IsNotError_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotError = new IsNotError();
            string[] cols = new string[2];
            cols[0] = "";
            //------------Execute Test---------------------------
            bool result = isNotError.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotError_HandlesType")]
        public void IsNotError_HandlesType_ReturnsIsNotErrorType()
        {
            var decisionType = enDecisionType.IsNotError;
            //------------Setup for test--------------------------
            var isNotError = new IsNotError();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotError.HandlesType());
        }
    }
}
