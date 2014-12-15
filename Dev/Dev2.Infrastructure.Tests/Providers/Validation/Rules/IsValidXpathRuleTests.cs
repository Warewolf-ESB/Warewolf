
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class IsValidXpathRuleTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidXpathRule_Check")]
        public void IsValidXpathRule_Check_StringIsEmpty_ReturnsError()
        {
            //------------Setup for test--------------------------
            var rule = new IsValidXpathRule(() => "") { LabelText = "Xpath" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("Xpath is not a valid expression", errorInfo.Message);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidXpathRule_Check")]
        public void IsValidXpathRule_Check_StringIsInvalidXPath_ReturnsError()
        {
            //------------Setup for test--------------------------
            var rule = new IsValidXpathRule(() => "$$!") { LabelText = "Xpath" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNotNull(errorInfo);
            Assert.AreEqual("Xpath is not a valid expression", errorInfo.Message);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("IsValidXpathRule_Check")]
        public void IsValidXpathRule_Check_StringInvalidXPath_ReturnsNoError()
        {
            //------------Setup for test--------------------------
            var rule = new IsValidXpathRule(() => "//root/number[@id='1']/text()") { LabelText = "Xpath" };
            //------------Execute Test---------------------------
            var errorInfo = rule.Check();
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfo);
        }
    }
}
