using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tu.Rules;

namespace Tu.Web.Tests
{
    [TestClass]
    public class RulesServiceTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_Constructor")]
        public void RulesService_Constructor_Default()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rulesService = new RulesService();

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RulesService_Constructor_NullRuleSet_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var rulesService = new RulesService(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_IsValid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RulesService_IsValid_NullRuleName_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var rulesService = new RulesService(new Mock<IRuleSet>().Object);

            //------------Execute Test---------------------------
            rulesService.IsValid(null, null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_IsValid")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RulesService_IsValid_NullFieldName_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var rulesService = new RulesService(new Mock<IRuleSet>().Object);

            //------------Execute Test---------------------------
            rulesService.IsValid("xx", null, null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_IsValid")]
        public void RulesService_IsValid_InvokesRuleSetGetRule()
        {
            //------------Setup for test--------------------------
            var ruleSet = new Mock<IRuleSet>();
            ruleSet.Setup(r => r.GetRule(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

            var rulesService = new RulesService(ruleSet.Object);

            //------------Execute Test---------------------------
            rulesService.IsValid("xx", "xx", "xx");

            //------------Assert Results-------------------------
            ruleSet.Verify(r => r.GetRule(It.IsAny<string>(), It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_IsValid")]
        public void RulesService_IsValid_RuleNotFound_TrueValidationResult()
        {
            //------------Setup for test--------------------------
            var ruleSet = new Mock<IRuleSet>();
            ruleSet.Setup(r => r.GetRule(It.IsAny<string>(), It.IsAny<string>())).Returns((IRule)null);

            var rulesService = new RulesService(ruleSet.Object);

            //------------Execute Test---------------------------
            var result = rulesService.IsValid("xx", "xx", "xx");

            //------------Assert Results-------------------------
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RulesService_IsValid")]
        public void RulesService_IsValid_RuleFound_InvokesRule()
        {
            //------------Setup for test--------------------------
            const bool ExpectedIsValid = false;
            var errors = new List<string>(new[] { "Error 1", "Error 2" });

            var rule = new Mock<IRule>();
            rule.Setup(r => r.Errors).Returns(errors);
            rule.Setup(r => r.IsValid(It.IsAny<object>())).Returns(ExpectedIsValid).Verifiable();

            var ruleSet = new Mock<IRuleSet>();
            ruleSet.Setup(r => r.GetRule(It.IsAny<string>(), It.IsAny<string>())).Returns(rule.Object);

            var rulesService = new RulesService(ruleSet.Object);

            //------------Execute Test---------------------------
            var result = rulesService.IsValid("xx", "xx", "xx");

            //------------Assert Results-------------------------
            rule.Verify(r => r.IsValid(It.IsAny<object>()));
            Assert.AreEqual(ExpectedIsValid, result.IsValid);
            Assert.IsTrue(errors.SequenceEqual(result.Errors));
        }
    }
}
