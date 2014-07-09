using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public partial class RuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Rule_Constructor_NullValidator_ThrowsArgumentException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rule = new Rule(null, null);

            //------------Assert Results-------------------------
        }
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Rule_Constructor_NullFieldName_ThrowsArgumentException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rule = new Rule(new Mock<IValidator>().Object, null);

            //------------Assert Results-------------------------
        }
    }
}
