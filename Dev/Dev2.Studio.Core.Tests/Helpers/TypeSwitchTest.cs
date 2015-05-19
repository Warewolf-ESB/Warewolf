
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
using Dev2.Studio.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Helpers
{
    /// <summary>
    /// Summary description for TypeSwitchTest
    /// </summary>
    [TestClass]
    public class TypeSwitchTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Do")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeSwitch_Do_WhenNullCases_ExpectArgumentNullException()
        {
            //------------Execute Test---------------------------
            TypeSwitch.Do(new object(), null);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Do")]
        [ExpectedException(typeof(Exception))]
        public void TypeSwitch_Do_WhenNullCases_ExpectException()
        {
            //------------Setup for test--------------------------
            TypeSwitch.CaseInfo case1 = new TypeSwitch.CaseInfo {IsDefault = false, Action = null, Target = typeof(object)};

            //------------Execute Test---------------------------

            TypeSwitch.Do(null, case1);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Do")]
        [ExpectedException(typeof(NullReferenceException))]
        public void TypeSwitch_Do_WhenCaseActionNull_ExpectActionSet()
        {
            //------------Setup for test--------------------------
            TypeSwitch.CaseInfo case1 = new TypeSwitch.CaseInfo { IsDefault = false, Action = null, Target = typeof(object) };
            var obj = new object();

            //------------Execute Test---------------------------
            TypeSwitch.Do(obj, case1);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Do")]
        [ExpectedException(typeof(Exception))]
        public void TypeSwitch_Do_WhenSourceNullAndNoDefaultAction_ExpectException()
        {
            //------------Setup for test--------------------------
            var act = new Action<object>(delegate { });
            TypeSwitch.CaseInfo case1 = new TypeSwitch.CaseInfo { IsDefault = false, Action = act, Target = typeof(object) };

            //------------Execute Test---------------------------
            TypeSwitch.Do(null, case1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Do")]
        public void TypeSwitch_Do_WhenSourceNullAndDefaultActionSent_ExpectNullActionValue()
        {
            //------------Setup for test--------------------------
            var act = new Action<object>(delegate { });
            TypeSwitch.CaseInfo case1 = new TypeSwitch.CaseInfo { IsDefault = true, Action = act, Target = typeof(object) };

            //------------Execute Test---------------------------
            TypeSwitch.Do(null, case1);

            //------------Assert Results-------------------------
            Assert.AreEqual(null, case1.Target.DeclaringType);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Case")]
        public void TypeSwitch_Do_WhenCaseGeneric_ExpectNotDefaultAndTargetEqualsObject()
        {
            //------------Execute Test---------------------------
            var result = TypeSwitch.Case(delegate(object o) { });

            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(object), result.Target);
            Assert.IsFalse(result.IsDefault);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("TypeSwitch_Case")]
        public void TypeSwitch_Do_WhenDefault_ExpectDefaultAndTargetNull()
        {
            //------------Execute Test---------------------------
            var result = TypeSwitch.Default(delegate { });

            //------------Assert Results-------------------------
            Assert.AreEqual(null, result.Target);
            Assert.IsTrue(result.IsDefault);

        }
    }
}
