/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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

            new TestRuleBase(null);


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
            Assert.AreEqual("", rule.LabelText);
            Assert.AreEqual("Value is invalid.", rule.ErrorText);
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
            Assert.AreEqual("Value is invalid.", error.Message);
            error.Do();
            Assert.IsTrue(doErrorWasAssigned);
        }
    }
}
