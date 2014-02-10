using System;
using System.Globalization;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    public class IsValidEmailAddressRuleTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IsValidEmailAddressRule_Check")]
        public void IsValidEmailAddressRule_Check_ItemIsValid_ResultIsNull()
        {
            Verify_Check(true, @"someone@this.co");
            Verify_Check(true, @"someone@this.com");
            Verify_Check(true, @"someone@this.co.za");
            Verify_Check(true, @"someone@co.za");
            Verify_Check(true, @"someone@this.co.za", @"someone@that.co.za");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("IsValidEmailAddressRule_Check")]
        public void IsValidEmailAddressRule_Check_ItemIsNotValid_ResultIsError()
        {
            Verify_Check(false, @"someone@this.c");
            Verify_Check(false, @"someone@this.com5");
            Verify_Check(false, @"someone#this.co.za");
            Verify_Check(false, @"@this.co.za");
            Verify_Check(false, @"someone@this");
            Verify_Check(false, @"someone@");
        }

        void Verify_Check(bool isValid, params string[] values)
        {
            //------------Setup for test--------------------------
            const char SplitToken = ',';
            Func<string> getValue = () => string.Join(SplitToken.ToString(CultureInfo.InvariantCulture), values);

            var rule = new IsValidEmailAddressRule(getValue, SplitToken) { LabelText = "The item" };

            //------------Execute Test---------------------------
            var result = rule.Check();

            //------------Assert Results-------------------------
            if(isValid)
            {
                Assert.IsNull(result);
            }
            else
            {
                StringAssert.Contains("The item contains an invalid email address", result.Message);
            }
        }
    }
}
