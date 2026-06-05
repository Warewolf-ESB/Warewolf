/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    /// <summary>
    /// Coverage for <see cref="DataListVerifyPart"/> ctor
    /// (Crap Score 420, Cyclomatic Complexity 20).
    ///
    /// The ctor exercises a tangle of branches based on whether:
    ///   - recordset contains '[' and ']' (CSV-style indexer)
    ///   - recordset contains '(' and ')' (function-style indexer)
    ///   - field contains '(' but not ')' (unterminated)
    ///   - field is empty vs populated
    ///   - useRaw is true
    ///   - recordset is null / empty
    /// All paths are reachable through <see cref="IntellisenseFactory"/>.
    /// </summary>
    [TestClass]
    public class DataListVerifyPartCtorTests
    {
        // -----------------------------------------------------------------
        // Single-arg (display name) ctor
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_DisplayNameOnly_StoresDisplayValueOnly()
        {
            var part = IntellisenseFactory.CreateJsonPart("[[json]]");

            Assert.AreEqual("[[json]]", part.DisplayValue);
            Assert.IsTrue(part.IsJson);
            Assert.IsNull(part.Recordset);
            Assert.IsNull(part.Field);
        }

        // -----------------------------------------------------------------
        // Scalar parts (recordset == null)
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_NullRecordset_NormalisesRecordsetToEmpty_FormatsScalarDisplay()
        {
            var part = IntellisenseFactory.CreateDataListValidationScalarPart("myScalar");

            Assert.AreEqual(string.Empty, part.Recordset);
            Assert.AreEqual("[[myScalar]]", part.DisplayValue);
            Assert.IsTrue(part.IsScalar);
            Assert.IsFalse(part.HasRecordsetIndex);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_NullRecordset_FieldWithUnclosedParen_DisplayKeepsOpenBracket()
        {
            // field "rec(" contains '(' but no ')' ⇒ short-circuit branch.
            var part = IntellisenseFactory.CreateDataListValidationScalarPart("rec(");

            Assert.AreEqual("[[rec(", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_NullRecordset_WithDescription_PreservesDescription()
        {
            var part = IntellisenseFactory.CreateDataListValidationScalarPart("scalarA", "my desc");

            Assert.AreEqual("scalarA", part.Field);
            Assert.AreEqual("my desc", part.Description);
            Assert.AreEqual("[[scalarA]]", part.DisplayValue);
        }

        // -----------------------------------------------------------------
        // Recordset normalisation: [] and () indexer cleanup
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetWithSquareBracketIndexer_StripsBrackets()
        {
            // "rec[1]" has [ and ] but no ( ⇒ strips both square brackets.
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec[1]", "field");

            Assert.AreEqual("rec1", part.Recordset);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetWithSquareAndRoundBrackets_TruncatesAtParen()
        {
            // "rec[1](2)" — contains '[' and ']' AND '(' so the ctor takes the
            // substring(0, indexof '(') branch ⇒ "rec[1]".
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("rec[1](2)", "f");

            Assert.AreEqual("rec[1]", part.Recordset);
        }

        // -----------------------------------------------------------------
        // Recordset with field present
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetWithField_NoIndexer_FormatsCleanDisplay()
        {
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "name");

            Assert.AreEqual("recs", part.Recordset);
            Assert.AreEqual("name", part.Field);
            Assert.AreEqual("[[recs().name]]", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetWithField_AndExistingParenIndexer_StripsBeforeRebuilding()
        {
            // recordset already has "(1)" — display path strips it then
            // re-emits with the current RecordsetIndex (empty string here).
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs(1)", "name");

            Assert.AreEqual("[[recs().name]]", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetWithField_AndExplicitIndex_DisplayUsesIndex()
        {
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "name", "desc", "3");

            Assert.AreEqual("[[recs(3).name]]", part.DisplayValue);
            Assert.AreEqual("3", part.RecordsetIndex);
            Assert.AreEqual("desc", part.Description);
            Assert.IsTrue(part.HasRecordsetIndex);
        }

        // -----------------------------------------------------------------
        // Recordset with no field (count-style display)
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetOnly_NoIndexer_FormatsBareDisplay()
        {
            // field is "" so the DisplayRecordsetOnly branch fires.
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "");

            Assert.AreEqual("[[recs()]]", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetOnly_AlreadyHasIndexer_StripsAndRebuilds()
        {
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs(7)", "");

            Assert.AreEqual("[[recs()]]", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_RecordsetOnly_WithExplicitIndex_DisplayCarriesIndex()
        {
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "", "desc", "5");

            Assert.AreEqual("[[recs(5)]]", part.DisplayValue);
            Assert.AreEqual("5", part.RecordsetIndex);
        }

        // -----------------------------------------------------------------
        // useRaw = true branch
        // -----------------------------------------------------------------

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_UseRaw_WithField_ConcatenatesRecordsetAndField()
        {
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "field", useRawPartsForDisplayValue: true);

            Assert.AreEqual("[[recsfield]]", part.DisplayValue);
        }

        [TestMethod]
        [Owner("Coverage")]
        [TestCategory(nameof(DataListVerifyPart))]
        public void Ctor_UseRaw_WithoutField_WrapsFieldOnly()
        {
            // recordset present but field is empty ⇒ uses only the field branch
            // (which produces "[[]]").
            var part = IntellisenseFactory.CreateDataListValidationRecordsetPart("recs", "", useRawPartsForDisplayValue: true);

            Assert.AreEqual("[[]]", part.DisplayValue);
        }
    }
}
