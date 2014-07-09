using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tu.Rules;

namespace Tu.Core.Tests.Rules
{
    [TestClass]
    public class RuleSetTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RuleSet_GetRule")]
        public void RuleSet_GetRule_InvalidRuleName_Null()
        {
            //------------Setup for test--------------------------
            var ruleSet = new RuleSet();

            //------------Execute Test---------------------------
            var rule = ruleSet.GetRule(Guid.NewGuid().ToString(), "xxx");

            //------------Assert Results-------------------------
            Assert.IsNull(rule);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("RuleSet_GetRule")]
        public void RuleSet_GetRule_ValidRuleNames_RuleInstance()
        {
            //------------Setup for test--------------------------
            var ruleNames = new[]
            {
                new KeyValuePair<string, Type>("AddressLineRule", typeof(AddressLineRule)),
                new KeyValuePair<string, Type>("CountryCodeRule", typeof(CountryCodeRule)),
                new KeyValuePair<string, Type>("EmailAddressRule", typeof(EmailAddressRule)),
                new KeyValuePair<string, Type>("ForenameRule", typeof(ForenameRule)),
                new KeyValuePair<string, Type>("GenderRule", typeof(GenderRule)),
                new KeyValuePair<string, Type>("GovIDRule", typeof(GovIDRule)),
                new KeyValuePair<string, Type>("MaritalStatusRule", typeof(MaritalStatusRule)),
                new KeyValuePair<string, Type>("MobileNumberRule", typeof(MobileNumberRule)),
                new KeyValuePair<string, Type>("PostalCodeRule", typeof(PostalCodeRule)),
                new KeyValuePair<string, Type>("SurnameRule", typeof(SurnameRule)),
                new KeyValuePair<string, Type>("TelCodeRule", typeof(TelCodeRule)),
                new KeyValuePair<string, Type>("TelNumberRule", typeof(TelNumberRule)),
                new KeyValuePair<string, Type>("TitleRule", typeof(TitleRule))
            };

            //------------Execute Test---------------------------
            var ruleSet = new RuleSet();

            foreach(var ruleName in ruleNames)
            {
                var rule = ruleSet.GetRule(ruleName.Key, ruleName.Key);

                //------------Assert Results-------------------------
                Assert.IsInstanceOfType(rule, ruleName.Value);
            }
        }
    }
}
