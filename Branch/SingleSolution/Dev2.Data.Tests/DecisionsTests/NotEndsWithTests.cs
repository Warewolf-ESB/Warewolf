using System;
using System.Text;
using System.Collections.Generic;
using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    /// <summary>
    /// Summary description for NotEndsWithTests
    /// </summary>
    [TestClass]
    public class NotEndsWithTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotEndsWith_Invoke")]
        public void NotEndsWith_Invoke_DoesEndWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotEndsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Data";

            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotEndsWith_Invoke")]
        public void NotEndsWith_Invoke_DoesntEndWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotEndsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "No";

            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }
    }
}
