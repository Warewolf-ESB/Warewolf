using System;
using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    [TestClass]
    public class NotBetweenTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotBetween_Invoke")]
        public void NotBetween_Invoke_IsBetween_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotBetween();
            string[] cols = new string[3];
            cols[0] = "15";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotBetween_Invoke")]
        public void NotBetween_Invoke_NotBetween_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotBetween();
            string[] cols = new string[3];
            cols[0] = "30";
            cols[1] = "10";
            cols[2] = "20";

            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }
}
