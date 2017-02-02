using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class NotRegExTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("NotRegEx_Invoke")]
        public void GivenSomeString_NotRegEx_Invoke_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notRegEx = new NotRegEx();
            string[] cols = new string[2];
            cols[0] = "Number 5 should";
            cols[1] = "d";
            //------------Execute Test---------------------------
            bool result = notRegEx.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("NotRegEx_Invoke")]
        public void NotRegEx_Invoke_NotRegEx_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotRegEx();
            string[] cols = new string[2];
            cols[0] = "324";
            cols[1] = "d";
            //------------Execute Test---------------------------
            bool result = notStartsWith.Invoke(cols);
            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Sanele Mthmembu")]
        [TestCategory("NotRegEx_HandlesType")]
        public void NotRegEx_HandlesType_ReturnsNotRegExType()
        {
            var decisionType = enDecisionType.NotRegEx;
            //------------Setup for test--------------------------
            var notRegEx = new NotRegEx();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(decisionType, notRegEx.HandlesType());
        }
    }
}
