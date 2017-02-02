using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    /// <summary>
    /// Summary description for IsStartsWithTests
    /// </summary>
    [TestClass]
    public class IsStartsWithTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]        
        public void IsStartsWith_Invoke_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var startsWith = new IsStartsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Test";
            //------------Execute Test---------------------------
            bool result = startsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void IsStartsWith_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var startsWith = new IsStartsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";
            //------------Execute Test---------------------------
            bool result = startsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("IsStartsWith_HandlesType")]
        public void IsStartsWith_HandlesType_ReturnsNotStartWithType()
        {
            var startsWith = enDecisionType.IsStartsWith;
            //------------Setup for test--------------------------
            var isStartsWith = new IsStartsWith();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(startsWith, isStartsWith.HandlesType());
        }
    }
}
