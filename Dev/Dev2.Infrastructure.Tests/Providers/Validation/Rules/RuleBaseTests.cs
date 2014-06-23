using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]    
    public class RuleBaseTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Rule_Constructor_GetValueIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rule = new TestRuleBase(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
        public void Rule_Constructor_GetValueIsNotNull_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rule = new TestRuleBase(() => "");

            //------------Assert Results-------------------------
            Assert.AreEqual("The", rule.LabelText);
            Assert.AreEqual("value is invalid.", rule.ErrorText);
            Assert.IsNull(rule.DoError);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_CreatError")]
        public void Rule_CreatError_ReturnsNonNullError()
        {
            //------------Setup for test--------------------------
            var doErrorWasAssigned = false;
            Action doError = () => { doErrorWasAssigned = true; };

            var rule = new TestRuleBase(() => "") { DoError = doError };

            //------------Execute Test---------------------------
            var error = rule.TestCreatError();

            //------------Assert Results-------------------------
            Assert.IsNotNull(error);
            Assert.AreEqual("The value is invalid.", error.Message);
            error.Do();
            Assert.IsTrue(doErrorWasAssigned);
        }
    }
}
