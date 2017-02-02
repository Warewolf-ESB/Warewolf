using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotEmailTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotEmail_Invoke")]
        public void GivenSomeString_IsNotEmail_Invoke_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotEmail = new IsNotEmail();
            string[] cols = new string[2];
            cols[0] = "something";
            //------------Execute Test---------------------------
            bool result = isNotEmail.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotEmail_Invoke")]
        public void IsNotEmail_Invoke_IsNotEmail_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isNotEmail = new IsNotEmail();
            string[] cols = new string[2];
            cols[0] = "soumething@something.com";
            //------------Execute Test---------------------------
            bool result = isNotEmail.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotEmail_HandlesType")]
        public void IsNotEmail_HandlesType_ReturnsIsNotEmailType()
        {
            var decisionType = enDecisionType.IsNotEmail;
            //------------Setup for test--------------------------
            var isNotEmail = new IsNotEmail();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotEmail.HandlesType());
        }
    }
}
