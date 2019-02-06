/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
    [TestClass]
    public class TypeSwitchTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TypeSwitch_Do_WhenNullCases_ExpectArgumentNullException()
        {
            TypeSwitch.Do(new object(), null);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        [ExpectedException(typeof(Exception))]
        public void TypeSwitch_Do_WhenNullCases_ExpectException()
        {
            var case1 = new TypeSwitch.CaseInfo {IsDefault = false, Action = null, Target = typeof(object)};

            TypeSwitch.Do(null, case1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        [ExpectedException(typeof(NullReferenceException))]
        public void TypeSwitch_Do_WhenCaseActionNull_ExpectException()
        {
            var case1 = new TypeSwitch.CaseInfo { IsDefault = false, Action = null, Target = typeof(object) };
            var obj = new object();

            TypeSwitch.Do(obj, case1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Do_WhenCaseActionNotNull_ExpectActionSet()
        {
            var wasCalled = false;
            var action = new Action(delegate { wasCalled = true; });
            var case1 = TypeSwitch.Case<object>(action);

            var obj = new object();

            TypeSwitch.Do(obj, case1);

            Assert.IsTrue(wasCalled, "expected action to be called");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Do_WhenCaseActionNull_ExpectCallAction_Success()
        {
            Action action = null;
            var case1 = TypeSwitch.Case<object>(action);

            case1.Action.Invoke(null);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        [ExpectedException(typeof(Exception))]
        public void TypeSwitch_Do_WhenSourceNullAndNoDefaultAction_ExpectException()
        {
            var act = new Action<object>(delegate { });
            var case1 = new TypeSwitch.CaseInfo { IsDefault = false, Action = act, Target = typeof(object) };

            TypeSwitch.Do(null, case1);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Do_WhenSourceNullAndDefaultActionNotNull_ExpectNullActionValue()
        {
            var act = new Action<object>(delegate { });
            var case1 = new TypeSwitch.CaseInfo { IsDefault = true, Action = act, Target = typeof(object) };

            TypeSwitch.Do(null, case1);

            Assert.AreEqual(null, case1.Target.DeclaringType);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Do_WhenDefault_ExpectActionCalled()
        {
            var wasCalled = false;
            var case1 = TypeSwitch.Default(delegate { wasCalled = true; });

            TypeSwitch.Do(typeof(object), case1);

            Assert.IsTrue(wasCalled, "expected action to be called");
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Case_Generic_ActionIsNotNull_Success()
        {
            var result = TypeSwitch.Case(delegate(object o) { });

            result.Action.Invoke(null);
            Assert.AreEqual(typeof(object), result.Target);
            Assert.IsFalse(result.IsDefault);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Case_Generic_ActionIsNull_Success()
        {
            Action<object> action = null;
            var result = TypeSwitch.Case(action);
            result.Action.Invoke(null);

            Assert.AreEqual(typeof(object), result.Target);
            Assert.IsFalse(result.IsDefault);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Default_ActionNotNull_Success()
        {
            var result = TypeSwitch.Default(delegate { });

            result.Action.Invoke(null);
            Assert.AreEqual(null, result.Target);
            Assert.IsTrue(result.IsDefault);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(TypeSwitch))]
        public void TypeSwitch_Default_ActionNull_Success()
        {
            var result = TypeSwitch.Default(null);

            result.Action.Invoke(null);
            Assert.AreEqual(null, result.Target);
            Assert.IsTrue(result.IsDefault);
        }
    }
}
