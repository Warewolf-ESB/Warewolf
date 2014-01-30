using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass][ExcludeFromCodeCoverage]
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
            Assert.AreEqual(0,ruleSet.Rules.Count);
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
            Assert.AreEqual(0,ruleSet.Rules.Count);
            Assert.AreEqual(0,validateRules.Count);
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
            ruleSet.Add(new TestRule("value",null));
            //------------Execute Test---------------------------
            var validateRules = ruleSet.ValidateRules();
            //------------Assert Results-------------------------
            Assert.AreEqual(0,validateRules.Count);
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
            Assert.AreEqual(1,validateRules.Count);
        }
    }

    class TestRule : Rule
    {
        internal TestRule(string value, Action onInvalid = null)
            : base(value, onInvalid)
        {
        }

        internal TestRule(string value, IActionableErrorInfo checkValue, Action onInvalid = null)
            : base(value, onInvalid)
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
