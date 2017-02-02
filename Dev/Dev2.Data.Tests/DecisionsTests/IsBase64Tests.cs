using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{    
    [TestClass]
    public class IsBase64Tests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsNotEqual_Invoke")]
        public void IsNotBase64_Invoke_ItemsEqual_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsBase64();
            string[] cols = new string[2];
            cols[0] = "aGVsbG8=";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsEqual_Invoke")]
        public void IsNotBase64_Invoke_NotEqualItems_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsBase64();
            string[] cols = new string[2];
            cols[0] = "aGVsbG8ASS@";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("IsEqual_HandlesType")]
        public void IsNotBase64_HandlesType_ReturnsIsEndsWithType()
        {
            var expected = enDecisionType.IsBase64;
            //------------Setup for test--------------------------
            var isEndsWith = new IsBase64();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isEndsWith.HandlesType());
        }
    }
}
