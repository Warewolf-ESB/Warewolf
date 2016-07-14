using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsNotTextTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotText_Invoke")]
        public void GivenNumber_IsNotText_Invoke_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var isNotText = new IsNotText();
            string[] cols = new string[2];
            cols[0] = "9";
            //------------Execute Test---------------------------
            bool result = isNotText.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
            //------------Execute Test---------------------------
            var emptyString = new[] { string.Empty };
            result = isNotText.Invoke(emptyString);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsNotText_Invoke")]
        public void IsNotText_Invoke_IsNotText_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsNotText();
            string[] cols = new string[2];
            cols[0] = "Text";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsNotText_HandlesType")]
        public void IsNotText_HandlesType_ReturnsIsNotTextType()
        {
            var decisionType = enDecisionType.IsNotText;
            //------------Setup for test--------------------------
            var isNotText = new IsNotText();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isNotText.HandlesType());
        }
    }
}
