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
    }
}
