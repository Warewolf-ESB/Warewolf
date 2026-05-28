/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    public class BaseConvertTOTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_FullConstructor_EmptyTypes_DefaultsToBase64AndText()
        {
            var to = new BaseConvertTO("input", "", "", "", 1);

            Assert.AreEqual("Base 64", to.ToType);
            Assert.AreEqual("Text", to.FromType);
            Assert.AreEqual(string.Empty, to.ToExpression);
            Assert.AreEqual("input", to.FromExpression);
            Assert.AreEqual(1, to.IndexNumber);
            Assert.IsFalse(to.Inserted);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_FullConstructor_KeepsSuppliedValuesAndInsertedFlag()
        {
            var to = new BaseConvertTO("input", "Base 64", "Text", "[[out]]", 2, true);

            Assert.AreEqual("Base 64", to.FromType);
            Assert.AreEqual("Text", to.ToType);
            Assert.AreEqual("[[out]]", to.ToExpression);
            Assert.IsTrue(to.Inserted);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_FromTypeSetter_IgnoresNull()
        {
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1) { FromType = null };

            Assert.AreEqual("Base 64", to.FromType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_ToTypeSetter_IgnoresNull()
        {
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1) { ToType = null };

            Assert.AreEqual("Text", to.ToType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_CanAddAndCanRemove_DependOnFromExpression()
        {
            var empty = new BaseConvertTO();
            Assert.IsTrue(empty.CanRemove());
            Assert.IsFalse(empty.CanAdd());

            empty.FromExpression = "[[x]]";
            Assert.IsFalse(empty.CanRemove());
            Assert.IsTrue(empty.CanAdd());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_ClearRow_ResetsAllExpressionsAndTypes()
        {
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1);

            to.ClearRow();

            // FromType/ToType setters ignore "" because we're passing "" not null,
            // so they retain "" rather than ignoring; ClearRow assigns "" directly.
            Assert.AreEqual("", to.FromType);
            Assert.AreEqual("", to.ToType);
            Assert.AreEqual(string.Empty, to.FromExpression);
            Assert.AreEqual(string.Empty, to.ToExpression);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_IsFromExpressionFocused_GetSet_RoundTrip()
        {
            var to = new BaseConvertTO { IsFromExpressionFocused = true };

            Assert.IsTrue(to.IsFromExpressionFocused);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_GetRuleSet_FromExpressionWithValue_AddsTwoRules()
        {
            var to = new BaseConvertTO { FromExpression = "[[a]]" };

            var ruleSet = (RuleSet)to.GetRuleSet("FromExpression", "<DataList></DataList>");

            Assert.AreEqual(2, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_GetRuleSet_FromExpressionEmpty_ReturnsEmptyRuleSet()
        {
            var to = new BaseConvertTO();

            var ruleSet = (RuleSet)to.GetRuleSet("FromExpression", "<DataList></DataList>");

            Assert.AreEqual(0, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_GetRuleSet_UnknownProperty_ReturnsEmptyRuleSet()
        {
            var to = new BaseConvertTO { FromExpression = "[[a]]" };

            var ruleSet = (RuleSet)to.GetRuleSet("SomethingElse", "<DataList></DataList>");

            Assert.AreEqual(0, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_TypedEquals_NullFalse_SameRefTrue_EqualValuesTrue()
        {
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1);

            Assert.IsFalse(to.Equals((BaseConvertTO)null));
            Assert.IsTrue(to.Equals(to));
            Assert.IsTrue(to.Equals(new BaseConvertTO("x", "Base 64", "Text", "y", 1)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_TypedEquals_DifferentFromType_ReturnsFalse()
        {
            var a = new BaseConvertTO("x", "Base 64", "Text", "y", 1);
            var b = new BaseConvertTO("x", "Text", "Text", "y", 1);

            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_ObjectEquals_HandlesNullSameRefAndType()
        {
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1);

            Assert.IsFalse(to.Equals((object)null));
            Assert.IsTrue(to.Equals((object)to));
            Assert.IsFalse(to.Equals("not a base convert to"));
            Assert.IsTrue(to.Equals((object)new BaseConvertTO("x", "Base 64", "Text", "y", 1)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(BaseConvertTO))]
        public void BaseConvertTO_GetHashCode_IsStableForSameInstance()
        {
            // Errors is reference-hashed, so equal instances need not share a hash code;
            // the contract we can rely on is stability per instance.
            var to = new BaseConvertTO("x", "Base 64", "Text", "y", 1);

            Assert.AreEqual(to.GetHashCode(), to.GetHashCode());
        }
    }
}
