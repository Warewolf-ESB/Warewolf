
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
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Providers.Validation.Rules
{
    [TestClass]
    public class ComposableRuleTest
    {


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
// ReSharper disable InconsistentNaming
        public void ComposeAbleRule_Null()

        {
            //------------Setup for test--------------------------
            new ComposableRule<string>(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComposeAbleRule_Or_Null()
        {
            //------------Setup for test--------------------------
            new ComposableRule<string>(new Rule1(()=>"")).Or(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_And")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComposeAbleRule_And_Null()
        {
            //------------Setup for test--------------------------
            new ComposableRule<string>(new Rule1(() => "")).And(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Check")]
        public void ComposeAbleRule_SingleCondition()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(()=>""));
            Assert.IsNotNull(cr.Check());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_And")]
        public void ComposeAbleRule_AndConditionNotSatisfied()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "")).And(new Rule1(()=>"1"));
            Assert.IsNotNull(cr.Check());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_And")]
        public void ComposeAbleRule_AndConditionSatisfied()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "1")).And(new Rule1(() => "1"));
            Assert.IsNull(cr.Check());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_And")]
        public void ComposeAbleRule_AndConditionSatisfied_Three()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "1")).And(new Rule1(() => "1")).And(new Rule2(() => "2"));
            Assert.IsNull(cr.Check());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_And")]
        public void ComposeAbleRule_AndConditionFailed_Three()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "1")).And(new Rule1(() => "1")).And(new Rule2(() => "v"));
            Assert.IsNotNull(cr.Check());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        public void ComposeAbleRule_Or_ConditionSatisfied()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "")).Or(new Rule1(() => "1"));
            Assert.IsNotNull(cr.Check());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        public void ComposeAbleRule_Or_FirstConditionSatisfied()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "1")).Or(new Rule1(() => ""));
            Assert.IsNotNull(cr.Check());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        public void ComposeAbleRule_Or_NoConditionSatisfied_Three()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "")).Or(new Rule1(() => "d")).Or(new Rule2(()=>"d"));
            Assert.IsNotNull(cr.Check());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        public void ComposeAbleRule_Or_ConditionSatisfied_Three()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "")).Or(new Rule1(() => "d")).Or(new Rule2(() => "2"));
            Assert.IsNotNull(cr.Check());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ComposeAbleRule_Or")]
        public void ComposeAbleRule_Or_ConditionSatisfied_Three_NullReturned()
        {
            //------------Setup for test--------------------------
            var cr = new ComposableRule<string>(new Rule1(() => "1")).Or(new Rule1(() => "1")).Or(new Rule2(() => "2"));
            Assert.IsNull(cr.Check());

        }
        // ReSharper restore InconsistentNaming
    }

    public class Rule1 :Rule<string>
    {
        public Rule1(Func<string> getValue)
            : base(getValue)
        {
        }

        #region Overrides of RuleBase

        public override IActionableErrorInfo Check()
        {
            if(GetValue()!= "1")
                return new ActionableErrorInfo();
            return null;
        }

        #endregion
    }

    public class Rule2 : Rule<string>
    {
        public Rule2(Func<string> getValue)
            : base(getValue)
        {
        }

        #region Overrides of RuleBase

        public override IActionableErrorInfo Check()
        {
            if (GetValue() != "2")
                return new ActionableErrorInfo();
            return null;
        }

        #endregion
    }
}
