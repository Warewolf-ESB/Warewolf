using System;
using System.Text;
using System.Collections.Generic;
using Dev2.Data.Decisions.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.DecisionsTests
{
    /// <summary>
    /// Summary description for NotStartsWithTests
    /// </summary>
    [TestClass]
    public class NotStartsWithTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotStartsWith_Invoke")]
        public void NotStartsWith_Invoke_DoesStartWith_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotStartsWith();
            string[] cols = new string[2];
            cols[0] = "TestData";
            cols[1] = "Test";
            
            //------------Execute Test---------------------------

            bool result = notStartsWith.Invoke(cols);

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("NotStartsWith_Invoke")]
        public void NotStartsWith_Invoke_DoesntStartWith_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var notStartsWith = new NotStartsWith();
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
