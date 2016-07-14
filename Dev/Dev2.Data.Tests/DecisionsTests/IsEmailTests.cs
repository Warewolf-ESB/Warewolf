using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsEmailTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsEmail_Invoke")]
        public void GivenSomeString_IsEmail_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isEmail = new IsEmail();
            string[] cols = new string[2];
            cols[0] = "something";
            //------------Execute Test---------------------------
            bool result = isEmail.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsEmail_Invoke")]
        public void IsEmail_Invoke_IsEmail_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isEmail = new IsEmail();
            string[] cols = new string[2];
            cols[0] = "soumething@something.com";
            //------------Execute Test---------------------------
            bool result = isEmail.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsEmail_HandlesType")]
        public void IsEmail_HandlesType_ReturnsIsEmailType()
        {
            var decisionType = enDecisionType.IsEmail;
            //------------Setup for test--------------------------
            var isEmail = new IsEmail();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isEmail.HandlesType());
        }
    }
}
