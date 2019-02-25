/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class ServiceTestOutputTOTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServiceTestOutputTO))]
        public void ServiceTestOutputTO_Validate_Defaults()
        {
            var testRunResult = new Common.Interfaces.TestRunResult
            {
                Message = "Message",
                TestName = "TestName",
                RunTestResult = Common.Interfaces.RunResult.None
            };
            var optionsForValueList = new System.Collections.Generic.List<string>
            {
                ">"
            };

            var serviceTestOutputTO = new ServiceTestOutputTO
            {
                Variable = "[[variable]]",
                Value = "hello",
                From = "from",
                To = "to",
                AssertOp = "=",
                HasOptionsForValue = false,
                OptionsForValue = optionsForValueList,
                Result = testRunResult
            };

            serviceTestOutputTO.OnSearchTypeChanged();

            Assert.AreEqual("[[variable]]", serviceTestOutputTO.Variable);
            Assert.AreEqual("hello", serviceTestOutputTO.Value);
            Assert.AreEqual("from", serviceTestOutputTO.From);
            Assert.AreEqual("to", serviceTestOutputTO.To);
            Assert.AreEqual("=", serviceTestOutputTO.AssertOp);
            Assert.IsFalse(serviceTestOutputTO.HasOptionsForValue);
            Assert.AreEqual(1, serviceTestOutputTO.OptionsForValue.Count);
            Assert.AreEqual(">", serviceTestOutputTO.OptionsForValue[0]);
            Assert.AreEqual(testRunResult, serviceTestOutputTO.Result);
        }
    }
}
