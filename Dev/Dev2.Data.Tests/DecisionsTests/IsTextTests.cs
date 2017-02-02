using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class IsTextTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsText_Invoke")]
        public void GivenNumber_IsText_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var isText = new IsText();
            string[] cols = new string[2];
            cols[0] = "9";
            //------------Execute Test---------------------------
            bool result = isText.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
            //------------Execute Test---------------------------
            var emptyString = new[] { string.Empty };
            result = isText.Invoke(emptyString);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("IsText_Invoke")]
        public void IsText_Invoke_IsText_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new IsText();
            string[] cols = new string[2];
            cols[0] = "Text";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsText_HandlesType")]
        public void IsText_HandlesType_ReturnsIsTextType()
        {
            var decisionType = enDecisionType.IsText;
            //------------Setup for test--------------------------
            var isText = new IsText();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, isText.HandlesType());
        }
    }
}
