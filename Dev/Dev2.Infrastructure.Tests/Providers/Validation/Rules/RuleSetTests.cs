using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RuleSetTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_Construct")]
        public void RuleSet_Construct_Construct_NoException()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var ruleSet = new RuleSet();
            //------------Assert Results-------------------------
            Assert.IsNotNull(ruleSet);
            Assert.AreEqual(0, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RuleSet_Construct")]
        public void RuleSet_Construct_ConstructWithRules_RulesAdded()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var ruleSet = new RuleSet(new [] { new IsNullRule(() => null) });

            //------------Assert Results-------------------------
            Assert.IsNotNull(ruleSet);
            Assert.AreEqual(1, ruleSet.Rules.Count);
        }      
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_Validate")]
        public void RuleSet_Validate_EmptyList_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();
            //------------Execute Test---------------------------
            var validateRules = ruleSet.ValidateRules();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, ruleSet.Rules.Count);
            Assert.AreEqual(0, validateRules.Count);
        }        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_AddRule")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RuleSet_AddRule_Null_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();
            //------------Execute Test---------------------------
            ruleSet.Add(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_AddRule")]
        public void RuleSet_AddRule_Rule_AddsToRuleList()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();
            //------------Execute Test---------------------------
            ruleSet.Add(new TestRule("Value"));
            //------------Assert Results-------------------------
            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_Validate")]
        public void RuleSet_Validate_WithNullRule_ReturnsValueOfCheck()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();
            ruleSet.Add(new TestRule("value", null));
            //------------Execute Test---------------------------
            var validateRules = ruleSet.ValidateRules();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, validateRules.Count);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RuleSet_Validate")]
        public void RuleSet_Validate_WithNotNullRule_ReturnsValueOfCheck()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();
            ruleSet.Add(new TestRule("value", new ActionableErrorInfo()));
            //------------Execute Test---------------------------
            var validateRules = ruleSet.ValidateRules();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, validateRules.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RuleSet_ValidateRules")]
        public void RuleSet_ValidateRules_WithRule_AssignsArgsToRuleAndReturnsValueOfCheck()
        {
            //------------Setup for test--------------------------
            var doErrorWasAssigned = false;
            Action doError = () => { doErrorWasAssigned = true; };

            const string LabelText = "Label Text";

            var ruleSet = new RuleSet();
            ruleSet.Add(new IsStringEmptyRule(() => ""));

            //------------Execute Test---------------------------
            var errors = ruleSet.ValidateRules(LabelText, doError);

            //------------Assert Results-------------------------
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(LabelText + " cannot be empty", errors[0].Message);
            errors[0].Do();
            Assert.IsTrue(doErrorWasAssigned);
        }
    }

    class TestRule : Rule<object>
    {
        internal TestRule(string value)
            : base(() => value)
        {
        }

        internal TestRule(string value, IActionableErrorInfo checkValue)
            : base(() => value)
        {
            CheckValue = checkValue;
        }

        public IActionableErrorInfo CheckValue { get; set; }

        #region Overrides of Rule

        public override IActionableErrorInfo Check()
        {
            return CheckValue;
        }

        #endregion
    }
}
