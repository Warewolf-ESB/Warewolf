using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    /// <summary>
    /// Summary description for IsEndsWithTests
    /// </summary>
    [TestClass]
    public class IsEndsWithTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsEndsWith_Invoke")]
        public void IsEndsWith_Invoke_DoesEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsEndsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Data";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("IsEndsWith_Invoke")]
        public void IsEndsWith_Invoke_DoesntEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var endsWith = new IsEndsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";
            //------------Execute Test---------------------------
            bool result = endsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }
        
        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsEndsWith_HandlesType")]
        public void IsEndsWith_HandlesType_ReturnsIsEndsWithType()
        {
            var expected = enDecisionType.IsEndsWith;
            //------------Setup for test--------------------------
            var isEndsWith = new IsEndsWith();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(expected, isEndsWith.HandlesType());
        }
    }
}
