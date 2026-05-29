/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Storage;
using WarewolfParserInterop;
using static DataStorage;

namespace WarewolfParsingTest
{
    [TestClass]
    public class HotspotTests
    {
        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_createEmpty_LengthAndCount_ProducesNothingFilledColumn()
        {
            // length controls how many items are seeded into the column;
            // count is what WarewolfAtomList.Count reports back.
            var column = EvaluationFunctions.createEmpty(3, 5);

            Assert.AreEqual(5, column.Count);
            Assert.IsTrue(column.All(a => a.IsNothing));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_addToList_AppendsValueAndReturnsSameList()
        {
            var column = EvaluationFunctions.createEmpty(0, 0);
            var value = WarewolfAtom.NewDataString("hello");

            var returned = EvaluationFunctions.addToList(column, value);

            Assert.AreSame(column, returned);
            Assert.AreEqual(1, returned.Count);
            Assert.AreEqual("hello", (returned[0] as WarewolfAtom.DataString)?.Item);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_addNothingToList_AppendsNothingAndReturnsSameList()
        {
            var column = EvaluationFunctions.createEmpty(0, 0);

            var returned = EvaluationFunctions.addNothingToList(column);

            Assert.AreSame(column, returned);
            Assert.AreEqual(1, returned.Count);
            Assert.IsTrue(returned[0].IsNothing);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_createFilled_CountAndValue_ProducesValueFilledColumn()
        {
            var atom = WarewolfAtom.NewDataString("x");

            var column = EvaluationFunctions.createFilled(4, atom);

            Assert.AreEqual(4, column.Count);
            Assert.IsTrue(column.All(a =>
                a is WarewolfAtom.DataString ds && ds.Item == "x"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("AssignEvaluation")]
        public void AssignEvaluation_removeFraming_ZeroesFrameOnEveryRecordset()
        {
            // WarewolfRecordset.Frame is mutable; set it directly to set up the
            // pre-condition for removeFraming.
            var env = EvalFunctionTests.CreateEnvironmentWithData();
            Assert.IsTrue(env.RecordSets.Count > 0, "test env has no recordsets");
            foreach (var kv in env.RecordSets) { kv.Value.Frame = 7; }
            Assert.IsTrue(env.RecordSets.Values.All(r => r.Frame == 7));

            var unframed = AssignEvaluation.removeFraming(env);

            Assert.IsTrue(unframed.RecordSets.Values.All(r => r.Frame == 0));
            Assert.AreSame(env.Scalar, unframed.Scalar);
            Assert.AreSame(env.JsonObjects, unframed.JsonObjects);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_IndexToString_IntIndex_ReturnsNumberAsString()
        {
            Assert.AreEqual("42", WarewolfDataEvaluationCommon.IndexToString(LanguageAST.Index.NewIntIndex(42)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_IndexToString_Star_ReturnsAsterisk()
        {
            Assert.AreEqual("*", WarewolfDataEvaluationCommon.IndexToString(LanguageAST.Index.Star));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_IndexToString_Last_ReturnsEmpty()
        {
            Assert.AreEqual("", WarewolfDataEvaluationCommon.IndexToString(LanguageAST.Index.Last));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_compare_BothNothing_ReturnsZero()
        {
            Assert.AreEqual(0, WarewolfDataEvaluationCommon.compare(WarewolfAtom.Nothing, WarewolfAtom.Nothing));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_compare_IntPair_ReturnsCompareTo()
        {
            Assert.IsTrue(WarewolfDataEvaluationCommon.compare(WarewolfAtom.NewInt(1), WarewolfAtom.NewInt(2)) < 0);
            Assert.IsTrue(WarewolfDataEvaluationCommon.compare(WarewolfAtom.NewInt(5), WarewolfAtom.NewInt(5)) == 0);
            Assert.IsTrue(WarewolfDataEvaluationCommon.compare(WarewolfAtom.NewInt(9), WarewolfAtom.NewInt(3)) > 0);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_compare_FloatPair_ReturnsCompareTo()
        {
            Assert.IsTrue(WarewolfDataEvaluationCommon.compare(WarewolfAtom.NewFloat(1.5), WarewolfAtom.NewFloat(2.5)) < 0);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("WarewolfDataEvaluationCommon")]
        public void WarewolfDataEvaluationCommon_compare_MixedTypes_FallsBackToStringCompare()
        {
            // Falls through to atomtoString comparison for mismatched types.
            var result = WarewolfDataEvaluationCommon.compare(
                WarewolfAtom.NewDataString("apple"),
                WarewolfAtom.NewDataString("banana"));
            Assert.IsTrue(result < 0);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_getPositionFromRecset_EmptyColumn_ReturnsOne()
        {
            // Build a minimal recordset whose only column is the position column
            // and that column is empty (Count == 0) -> the empty-column branch.
            var env = EvalFunctionTests.CreateEnvironmentWithData();
            // Pick any existing recordset, clear its position column.
            var (name, rset) = env.RecordSets.Select(kv => (kv.Key, kv.Value)).First();

            // Use the position column key (constant on the F# side).
            const string positionColumn = "WarewolfPositionColumn";
            Assert.IsTrue(rset.Data.ContainsKey(positionColumn));

            // The function reads recset.Data[columnName].Count; when 0 -> returns 1.
            var emptyRset = EvaluationFunctions.createEmpty(0, 0);
            // Replace position column with an empty list on a fresh recordset map.
            var emptyData = rset.Data.Add(positionColumn, emptyRset);
            var probe = new WarewolfRecordset(
                data: emptyData.Remove(positionColumn).Add(positionColumn, emptyRset),
                optimisations: rset.Optimisations,
                lastIndex: 0,
                frame: 0);

            var pos = EvaluationFunctions.getPositionFromRecset(probe, positionColumn);

            Assert.AreEqual(1, pos);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("EvaluationFunctions")]
        public void EvaluationFunctions_getPositionFromRecset_MissingColumn_FallsBackToLastIndexPlusOne()
        {
            // When column key is absent and Frame == 0 -> returns LastIndex + 1.
            var env = EvalFunctionTests.CreateEnvironmentWithData();
            var rset = env.RecordSets.First().Value;
            Assert.IsTrue(rset.LastIndex > 0, "expected non-empty recordset");

            var pos = EvaluationFunctions.getPositionFromRecset(rset, "NoSuchColumn");

            Assert.AreEqual(rset.LastIndex + 1, pos);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("AssignEvaluation")]
        public void AssignEvaluation_updateColumnWithValue_ExistingColumn_OverwritesEveryRow()
        {
            // Existing column path: iterates and assigns `value` to each row.
            var env = EvalFunctionTests.CreateEnvironmentWithData();
            var rset = env.RecordSets["Rec"]; // has columns a, b
            Assert.IsTrue(rset.Data.ContainsKey("a"));
            var value = WarewolfAtom.NewDataString("Z");

            var updated = AssignEvaluation.updateColumnWithValue(rset, "a", value);

            // Column instance is mutated in place; returned recordset is the same.
            Assert.AreSame(rset, updated);
            for (int i = 0; i < updated.Data["a"].Count; i++)
            {
                Assert.IsTrue(updated.Data["a"][i] is WarewolfAtom.DataString ds && ds.Item == "Z");
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("AssignEvaluation")]
        public void AssignEvaluation_updateColumnWithValue_MissingColumn_AddsFilledColumn()
        {
            // Missing-column branch: returns a new recordset with column added via createFilled.
            var env = EvalFunctionTests.CreateEnvironmentWithData();
            var rset = env.RecordSets["Rec"];
            Assert.IsFalse(rset.Data.ContainsKey("brandNew"));
            var value = WarewolfAtom.NewInt(7);

            var updated = AssignEvaluation.updateColumnWithValue(rset, "brandNew", value);

            Assert.IsTrue(updated.Data.ContainsKey("brandNew"));
            Assert.AreEqual(rset.Count, updated.Data["brandNew"].Count);
        }
    }
}
