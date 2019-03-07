/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class WcfActionTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(WcfAction))]
        public void WcfAction_Validate_Default()
        {
            const string expectedFullName = "FullName";
            const string expectedMethod = "Method";

            var mockServiceInput = new Mock<IServiceInput>();

            var expectedInputs = new List<IServiceInput>
            {
                mockServiceInput.Object
            };

            var expectedReturnType = typeof(string);
            var expectedVariables = new List<INameValue>
            {
                new NameValue("name", "")
            };

            var wcfAction = new WcfAction
            {
                FullName = expectedFullName,
                Method = expectedMethod,
                Inputs = expectedInputs,
                ReturnType = expectedReturnType,
                Variables = expectedVariables
            };

            Assert.AreEqual(expectedFullName, wcfAction.FullName);
            Assert.AreEqual(expectedMethod, wcfAction.Method);
            Assert.AreEqual(expectedInputs, wcfAction.Inputs);
            Assert.AreEqual(1, wcfAction.Inputs.Count);
            Assert.AreEqual(expectedReturnType, wcfAction.ReturnType);
            Assert.AreEqual(expectedVariables, wcfAction.Variables);
            Assert.AreEqual(1, wcfAction.Variables.Count);
            Assert.AreEqual(expectedFullName + expectedMethod, wcfAction.GetHashCodeBySource());
        }
    }
}
