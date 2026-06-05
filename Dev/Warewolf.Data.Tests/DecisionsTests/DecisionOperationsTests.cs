/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data.Decisions.Operations;
using Warewolf.Options;

namespace Warewolf.Data.Tests.DecisionsTests
{
    /// <summary>
    /// Covers the whole <see cref="Warewolf.Data.Decisions.Operations"/> family. Each test asserts the
    /// operation's <c>HandlesType()</c> mapping plus its <c>Invoke()</c> result for a matching and a
    /// non-matching input (behaviour, not just line hits).
    /// </summary>
    [TestClass]
    public class DecisionOperationsTests
    {
        const string Owner = "Ashley Lewis";
        const string Category = "Warewolf.Data.Decisions.Operations";

        // ---- Error -------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsError_Invoke_NonEmptyTrue_EmptyFalse()
        {
            var op = new IsError();
            Assert.AreEqual(enDecisionType.IsError, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "boom" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotError_Invoke_EmptyTrue_NonEmptyFalse()
        {
            var op = new IsNotError();
            Assert.AreEqual(enDecisionType.IsNotError, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "" }));
            Assert.IsFalse(op.Invoke(new[] { "boom" }));
        }

        // ---- Null --------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNull_Invoke_NullValueTrue_NonNullFalse_NullArrayFalse()
        {
            var op = new IsNull();
            Assert.AreEqual(enDecisionType.IsNull, op.HandlesType());
            Assert.IsTrue(op.Invoke(new string[] { null }));
            Assert.IsFalse(op.Invoke(new[] { "x" }));
            Assert.IsFalse(op.Invoke(null));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotNull_Invoke_NonNullTrue_NullValueFalse_NullArrayFalse()
        {
            var op = new IsNotNull();
            Assert.AreEqual(enDecisionType.IsNotNull, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "x" }));
            Assert.IsFalse(op.Invoke(new string[] { null }));
            Assert.IsFalse(op.Invoke(null));
        }

        // ---- Numeric -----------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNumeric_Invoke_NumberTrue_TextFalse()
        {
            var op = new IsNumeric();
            Assert.AreEqual(enDecisionType.IsNumeric, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "123" }));
            Assert.IsFalse(op.Invoke(new[] { "abc" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotNumeric_Invoke_TextTrue_NumberFalse()
        {
            var op = new IsNotNumeric();
            Assert.AreEqual(enDecisionType.IsNotNumeric, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "abc" }));
            Assert.IsFalse(op.Invoke(new[] { "123" }));
        }

        // ---- Text --------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsText_Invoke_AlphaTrue_NumberFalse()
        {
            var op = new IsText();
            Assert.AreEqual(enDecisionType.IsText, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "abc" }));
            Assert.IsFalse(op.Invoke(new[] { "123" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotText_Invoke_NumberTrue_AlphaFalse_BlankTrue()
        {
            var op = new IsNotText();
            Assert.AreEqual(enDecisionType.IsNotText, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "123" }));
            Assert.IsFalse(op.Invoke(new[] { "abc" }));
            Assert.IsTrue(op.Invoke(new[] { "" }));
        }

        // ---- Alphanumeric ------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsAlphanumeric_Invoke_AlphaNumTrue_SymbolFalse_BlankFalse()
        {
            var op = new IsAlphanumeric();
            Assert.AreEqual(enDecisionType.IsAlphanumeric, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "abc123" }));
            Assert.IsFalse(op.Invoke(new[] { "!!!" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotAlphanumeric_Invoke_SymbolTrue_AlphaNumFalse_BlankTrue()
        {
            var op = new IsNotAlphanumeric();
            Assert.AreEqual(enDecisionType.IsNotAlphanumeric, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "!!!" }));
            Assert.IsFalse(op.Invoke(new[] { "abc123" }));
            Assert.IsTrue(op.Invoke(new[] { "" }));
        }

        // ---- Date --------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsDate_Invoke_DateTrue_NonDateFalse()
        {
            var op = new IsDate();
            Assert.AreEqual(enDecisionType.IsDate, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "2020-01-01" }));
            Assert.IsFalse(op.Invoke(new[] { "notadate" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotDate_Invoke_NonDateTrue_DateFalse()
        {
            var op = new IsNotDate();
            Assert.AreEqual(enDecisionType.IsNotDate, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "notadate" }));
            Assert.IsFalse(op.Invoke(new[] { "2020-01-01" }));
        }

        // ---- Email -------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsEmail_Invoke_EmailTrue_NonEmailFalse()
        {
            var op = new IsEmail();
            Assert.AreEqual(enDecisionType.IsEmail, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "user@example.com" }));
            Assert.IsFalse(op.Invoke(new[] { "notanemail" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotEmail_Invoke_NonEmailTrue_EmailFalse()
        {
            var op = new IsNotEmail();
            Assert.AreEqual(enDecisionType.IsNotEmail, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "notanemail" }));
            Assert.IsFalse(op.Invoke(new[] { "user@example.com" }));
        }

        // ---- Base64 ------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsBase64_Invoke_Base64True_InvalidFalse_BlankFalse()
        {
            var op = new IsBase64();
            Assert.AreEqual(enDecisionType.IsBase64, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "SGVsbG8=" })); // "Hello"
            Assert.IsFalse(op.Invoke(new[] { "not base64!" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotBase64_Invoke_InvalidTrue_Base64False_BlankFalse()
        {
            var op = new IsNotBase64();
            Assert.AreEqual(enDecisionType.IsNotBase64, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "not base64!" }));
            Assert.IsFalse(op.Invoke(new[] { "SGVsbG8=" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        // ---- Binary ------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsBinary_Invoke_BinaryTrue_NonBinaryFalse_BlankFalse()
        {
            var op = new IsBinary();
            Assert.AreEqual(enDecisionType.IsBinary, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "0101" }));
            Assert.IsFalse(op.Invoke(new[] { "123" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotBinary_Invoke_NonBinaryTrue_BinaryFalse_BlankFalse()
        {
            var op = new IsNotBinary();
            Assert.AreEqual(enDecisionType.IsNotBinary, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "123" }));
            Assert.IsFalse(op.Invoke(new[] { "0101" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        // ---- Hex ---------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsHex_Invoke_HexTrue_NonHexFalse_BlankFalse()
        {
            var op = new IsHex();
            Assert.AreEqual(enDecisionType.IsHex, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "1A2B" }));
            Assert.IsFalse(op.Invoke(new[] { "xyz" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotHex_Invoke_NonHexTrue_HexFalse_BlankFalse()
        {
            var op = new IsNotHex();
            Assert.AreEqual(enDecisionType.IsNotHex, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "xyz" }));
            Assert.IsFalse(op.Invoke(new[] { "1A2B" }));
            Assert.IsFalse(op.Invoke(new[] { "" }));
        }

        // ---- Xml ---------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsXml_Invoke_XmlTrue_PlainTextFalse()
        {
            var op = new IsXml();
            Assert.AreEqual(enDecisionType.IsXML, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "<tag>value</tag>" }));
            Assert.IsFalse(op.Invoke(new[] { "plain text" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotXml_Invoke_PlainTextTrue_XmlFalse()
        {
            var op = new IsNotXml();
            Assert.AreEqual(enDecisionType.IsNotXML, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "plain text" }));
            Assert.IsFalse(op.Invoke(new[] { "<tag>value</tag>" }));
        }

        // ---- Regex -------------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsRegEx_Invoke_MatchTrue_NoMatchFalse()
        {
            var op = new IsRegEx();
            Assert.AreEqual(enDecisionType.IsRegEx, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "h.*o" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "z+" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NotRegEx_Invoke_NoMatchTrue_MatchFalse()
        {
            var op = new NotRegEx();
            Assert.AreEqual(enDecisionType.NotRegEx, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "z+" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "h.*o" }));
        }

        // ---- Contains ----------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsContains_Invoke_ContainsTrue_NotContainsFalse()
        {
            var op = new IsContains();
            Assert.AreEqual(enDecisionType.IsContains, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "ell" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "xyz" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NotContains_Invoke_NotContainsTrue_ContainsFalse()
        {
            var op = new NotContains();
            Assert.AreEqual(enDecisionType.NotContain, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "xyz" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "ell" }));
        }

        // ---- StartsWith --------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsStartsWith_Invoke_StartsTrue_NotStartsFalse()
        {
            var op = new IsStartsWith();
            Assert.AreEqual(enDecisionType.IsStartsWith, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "he" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "lo" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NotStartsWith_Invoke_NotStartsTrue_StartsFalse()
        {
            var op = new NotStartsWith();
            Assert.AreEqual(enDecisionType.NotStartsWith, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "lo" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "he" }));
        }

        // ---- EndsWith ----------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsEndsWith_Invoke_EndsTrue_NotEndsFalse()
        {
            var op = new IsEndsWith();
            Assert.AreEqual(enDecisionType.IsEndsWith, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "lo" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "he" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NotEndsWith_Invoke_NotEndsTrue_EndsFalse()
        {
            var op = new NotEndsWith();
            Assert.AreEqual(enDecisionType.NotEndsWith, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "hello", "he" }));
            Assert.IsFalse(op.Invoke(new[] { "hello", "lo" }));
        }

        // ---- Equality ----------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsEqual_Invoke_NumericEqual_NumericNotEqual_StringEqual()
        {
            var op = new IsEqual();
            Assert.AreEqual(enDecisionType.IsEqual, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "5", "5" }));
            Assert.IsFalse(op.Invoke(new[] { "5", "6" }));
            Assert.IsTrue(op.Invoke(new[] { "abc", "abc" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsNotEqual_Invoke_DifferentTrue_SameFalse()
        {
            var op = new IsNotEqual();
            Assert.AreEqual(enDecisionType.IsNotEqual, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "abc", "abd" }));
            Assert.IsFalse(op.Invoke(new[] { "abc", "abc" }));
        }

        // ---- Relational --------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsGreaterThan_Invoke_NumericAndStringBranches_BlankFalse()
        {
            var op = new IsGreaterThan();
            Assert.AreEqual(enDecisionType.IsGreaterThan, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "5", "3" }));   // numeric
            Assert.IsFalse(op.Invoke(new[] { "3", "5" }));
            Assert.IsTrue(op.Invoke(new[] { "b", "a" }));   // string
            Assert.IsFalse(op.Invoke(new[] { "", "a" }));   // blank short-circuit
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsGreaterThanOrEqual_Invoke_EqualTrue_LessFalse()
        {
            var op = new IsGreaterThanOrEqual();
            Assert.AreEqual(enDecisionType.IsGreaterThanOrEqual, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "5", "5" }));
            Assert.IsFalse(op.Invoke(new[] { "3", "5" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsLessThan_Invoke_NumericAndStringBranches_BlankFalse()
        {
            var op = new IsLessThan();
            Assert.AreEqual(enDecisionType.IsLessThan, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "3", "5" }));   // numeric
            Assert.IsFalse(op.Invoke(new[] { "5", "3" }));
            Assert.IsTrue(op.Invoke(new[] { "a", "b" }));   // string
            Assert.IsFalse(op.Invoke(new[] { "", "a" }));   // blank short-circuit
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsLessThanOrEqual_Invoke_EqualTrue_GreaterFalse()
        {
            var op = new IsLessThanOrEqual();
            Assert.AreEqual(enDecisionType.IsLessThanOrEqual, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "5", "5" }));
            Assert.IsFalse(op.Invoke(new[] { "6", "5" }));
        }

        // ---- Between -----------------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void IsBetween_Invoke_NumericInsideTrue_OutsideFalse_DateInsideTrue()
        {
            var op = new IsBetween();
            Assert.AreEqual(enDecisionType.IsBetween, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "5", "1", "10" }));
            Assert.IsFalse(op.Invoke(new[] { "20", "1", "10" }));
            Assert.IsTrue(op.Invoke(new[] { "2020-06-01", "2020-01-01", "2020-12-01" }));
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void NotBetween_Invoke_OutsideTrue_InsideFalse()
        {
            var op = new NotBetween();
            Assert.AreEqual(enDecisionType.NotBetween, op.HandlesType());
            Assert.IsTrue(op.Invoke(new[] { "20", "1", "10" }));
            Assert.IsFalse(op.Invoke(new[] { "5", "1", "10" }));
        }

        // ---- DecisionUtils -----------------------------------------------------------

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void DecisionUtils_IsNumericComparison_BothNumeric_ReturnsFalseAndParses()
        {
            var isString = DecisionUtils.IsNumericComparison(new[] { "5", "3" }, out var numbers);
            Assert.IsFalse(isString);
            Assert.AreEqual(5m, numbers[0]);
            Assert.AreEqual(3m, numbers[1]);
        }

        [TestMethod]
        [Owner(Owner)]
        [TestCategory(Category)]
        public void DecisionUtils_IsNumericComparison_NonNumeric_ReturnsTrue()
        {
            var isString = DecisionUtils.IsNumericComparison(new[] { "abc", "3" }, out _);
            Assert.IsTrue(isString);
        }
    }
}
