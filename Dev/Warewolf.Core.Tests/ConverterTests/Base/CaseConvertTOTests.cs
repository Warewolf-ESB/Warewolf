/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Providers.Validation.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.ConverterTests.Base
{
    [TestClass]
    public class CaseConvertTOTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_Constructor_EmptyConvertType_DefaultsToUpper()
        {
            var to = new CaseConvertTO("abc", "", "", 1);

            Assert.AreEqual("UPPER", to.ConvertType);
            Assert.AreEqual("abc", to.StringToConvert);
            Assert.AreEqual(1, to.IndexNumber);
            Assert.IsFalse(to.Inserted);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_Constructor_KeepsSuppliedConvertTypeAndInsertedFlag()
        {
            var to = new CaseConvertTO("abc", "lower", "res", 2, true);

            Assert.AreEqual("lower", to.ConvertType);
            Assert.AreEqual("res", to.Result);
            Assert.IsTrue(to.Inserted);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_StringToConvertSetter_AlsoSetsResult()
        {
            var to = new CaseConvertTO { StringToConvert = "hello" };

            Assert.AreEqual("hello", to.Result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_ConvertTypeSetter_IgnoresNull()
        {
            var to = new CaseConvertTO("abc", "lower", "res", 1) { ConvertType = null };

            Assert.AreEqual("lower", to.ConvertType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_Result_WhenBlank_FallsBackToStringToConvert()
        {
            var to = new CaseConvertTO { StringToConvert = "value" };
            to.Result = "   ";

            Assert.AreEqual("value", to.Result);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_CanAddAndCanRemove_DependOnStringToConvert()
        {
            var empty = new CaseConvertTO();
            Assert.IsTrue(empty.CanRemove());
            Assert.IsFalse(empty.CanAdd());

            var filled = new CaseConvertTO { StringToConvert = "x" };
            Assert.IsFalse(filled.CanRemove());
            Assert.IsTrue(filled.CanAdd());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_ClearRow_ResetsToDefaults()
        {
            var to = new CaseConvertTO("abc", "lower", "res", 1);

            to.ClearRow();

            Assert.AreEqual(string.Empty, to.StringToConvert);
            Assert.AreEqual("UPPER", to.ConvertType);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_GetRuleSet_StringToConvertWithValue_AddsExpressionRule()
        {
            var to = new CaseConvertTO { StringToConvert = "[[a]]" };

            var ruleSet = (RuleSet)to.GetRuleSet("StringToConvert", "<DataList></DataList>");

            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_GetRuleSet_StringToConvertEmpty_AddsEmptyRule()
        {
            var to = new CaseConvertTO();

            var ruleSet = (RuleSet)to.GetRuleSet("StringToConvert", "<DataList></DataList>");

            Assert.AreEqual(1, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_GetRuleSet_UnknownProperty_ReturnsEmptyRuleSet()
        {
            var to = new CaseConvertTO { StringToConvert = "x" };

            var ruleSet = (RuleSet)to.GetRuleSet("SomethingElse", "<DataList></DataList>");

            Assert.AreEqual(0, ruleSet.Rules.Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_TypedEquals_NullFalse_SameRefTrue_EqualValuesTrue()
        {
            var to = new CaseConvertTO("abc", "UPPER", "abc", 1);

            Assert.IsFalse(to.Equals((ICaseConvertTO)null));
            Assert.IsTrue(to.Equals((ICaseConvertTO)to));
            Assert.IsTrue(to.Equals(new CaseConvertTO("abc", "UPPER", "abc", 1)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_TypedEquals_DifferentConvertType_ReturnsFalse()
        {
            var to = new CaseConvertTO("abc", "UPPER", "abc", 1);
            var other = new CaseConvertTO("abc", "lower", "abc", 1);

            Assert.IsFalse(to.Equals(other));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_ObjectEquals_HandlesNullSameRefAndType()
        {
            var to = new CaseConvertTO("abc", "UPPER", "abc", 1);

            Assert.IsFalse(to.Equals((object)null));
            Assert.IsTrue(to.Equals((object)to));
            Assert.IsFalse(to.Equals("not a case convert to"));
            Assert.IsTrue(to.Equals((object)new CaseConvertTO("abc", "UPPER", "abc", 1)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory(nameof(CaseConvertTO))]
        public void CaseConvertTO_GetHashCode_EqualForEqualInstances()
        {
            var a = new CaseConvertTO("abc", "UPPER", "abc", 1);
            var b = new CaseConvertTO("abc", "UPPER", "abc", 1);

            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
