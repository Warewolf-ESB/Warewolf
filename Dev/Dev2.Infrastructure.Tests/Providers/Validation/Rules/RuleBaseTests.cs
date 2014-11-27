
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RuleBaseTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void Rule_Constructor_GetValueIsNull_ThrowsArgumentNullException()
// ReSharper restore InconsistentNaming
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
// ReSharper disable ObjectCreationAsStatement
            new TestRuleBase(null);
// ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Rule_Constructor")]
// ReSharper disable InconsistentNaming
        public void Rule_Constructor_GetValueIsNotNull_PropertiesInitialized()
// ReSharper restore InconsistentNaming
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
// ReSharper disable InconsistentNaming
        public void Rule_CreatError_ReturnsNonNullError()
// ReSharper restore InconsistentNaming
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
