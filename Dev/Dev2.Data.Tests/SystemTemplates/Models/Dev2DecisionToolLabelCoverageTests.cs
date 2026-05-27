/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

// -----------------------------------------------------------------------------
// Coverage uplift for the ToolLabelGenerator nested type in Dev2Decision.
//
// The pre-existing Dev2DecisionTests covered the populated-column-count == 0,
// 1 and 2 paths of ToolLabelGenerator.Generate() in detail, but the entire
// _populatedColumnCount == 3 path — including the seven starred-index switch
// arms in ResolveStarredIndicesForLabel — was untested.
//
// These tests drive Dev2Decision.GenerateToolLabel with all three Col1/Col2/
// Col3 populated, varying which columns are recordset(*) expressions, and
// asserting the full label string for every combination of starred and
// scalar columns.
//
// Branches covered (relative to Dev2Decision.cs):
//   * Generate()    line 284 — the _populatedColumnCount == 3 entry.
//   * Generate()    line 294 — the non-starred fall-through return.
//   * ResolveStarredIndicesForLabel:
//     - Col3-only starred                   (lines 331-342)
//     - Col2-only starred                   (lines 343-354)
//     - Col2 + Col3 starred                 (lines 355-376)
//     - Col1-only starred                   (lines 377-388)
//     - Col1 + Col3 starred                 (lines 389-413)
//     - Col1 + Col2 starred                 (lines 414-438)
//     - All three starred                   (lines 439-472)
//     - No columns starred → returns null   (line 473-474)
//
// All assertions match exactly the strings produced by the production code as
// of the time these tests were authored.  Where a switch arm contains an
// inner loop the tests use a Func-returning Moq setup so each
// EvalAsListOfStrings call produces a fresh List<string>; this exercises the
// loop bodies that mutate allColNValues via RemoveAt(0).
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage.Interfaces;

namespace Dev2.Data.Tests.SystemTemplates.Models
{
    [TestClass]
    public class Dev2DecisionToolLabelCoverageTests
    {
        const string Category = "Dev2Decision_ToolLabelGenerator_Coverage";

        // ------------------------------------------------------------------
        // count == 3, no starred columns — fall-through return path
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_NoStars_UsesFallThrough()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[c1]]",
                Col2 = "[[c2]]",
                Col3 = "[[c3]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual("If [[c1]] Is Between [[c2]] and [[c3]]", result);
            Assert.AreEqual(0, error.FetchErrors().Count);
        }

        // ------------------------------------------------------------------
        // count == 3, only Col3 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col3Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[c1]]",
                Col2 = "[[c2]]",
                Col3 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual(
                "If [[c1]] Is Between [[c2]] AND [[a]] AND [[c1]] Is Between [[c2]] AND [[b]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, only Col2 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col2Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[c1]]",
                Col2 = "[[recset(*).field]]",
                Col3 = "[[c3]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual(
                "If [[c1]] Is Between [[a]] AND [[c3]] AND [[c1]] Is Between [[b]] AND [[c3]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, only Col1 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col1Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[recset(*).field]]",
                Col2 = "[[c2]]",
                Col3 = "[[c3]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual(
                "If [[a]] Is Between [[c2]] AND [[c3]] AND [[b]] Is Between [[c2]] AND [[c3]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, Col2 + Col3 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col2AndCol3Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[c1]]",
                Col2 = "[[recset(*).a]]",
                Col3 = "[[recset(*).b]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            // With two-element lists allCol2 and allCol3 each shrink to one entry after RemoveAt(0);
            // Math.Max(1, 1) = 1 so the inner loop runs one extra append.
            Assert.AreEqual(
                "If [[c1]] Is Between [[a]] AND [[a]] AND [[c1]] Is Between [[b]] AND [[b]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, Col1 + Col3 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col1AndCol3Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[recset(*).a]]",
                Col2 = "[[c2]]",
                Col3 = "[[recset(*).b]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            Assert.AreEqual(
                "If [[a]] Is Between [[c2]] AND [[a]] AND [[b]] Is Between [[c2]] AND [[b]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, Col1 + Col2 starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col1AndCol2Starred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[recset(*).a]]",
                Col2 = "[[recset(*).b]]",
                Col3 = "[[c3]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            // Note that the production code uses allCol2Values[0] (not [i]) when
            // appending inside the inner loop — see Dev2Decision.cs line 435.
            Assert.AreEqual(
                "If [[a]] Is Between [[a]] AND [[c3]] AND [[b]] Is Between [[b]] AND [[c3]]",
                result);
        }

        // ------------------------------------------------------------------
        // count == 3, all three columns starred
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_AllStarred()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[recset(*).a]]",
                Col2 = "[[recset(*).b]]",
                Col3 = "[[recset(*).c]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            // Same allCol2Values[0] quirk as the Col1+Col2 case (line 469).
            Assert.AreEqual(
                "If [[a]] Is Between [[a]] AND [[a]] AND [[b]] Is Between [[b]] AND [[b]]",
                result);
        }

        // ------------------------------------------------------------------
        // Exercise the OR Dev2DecisionMode to confirm mode is woven into the label
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_Col3Starred_OrMode()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[c1]]",
                Col2 = "[[c2]]",
                Col3 = "[[recset(*).field]]",
                EvaluationFn = enDecisionType.IsBetween
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[a]]", "[[b]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.OR, out var error);

            Assert.AreEqual(
                "If [[c1]] Is Between [[c2]] AND [[a]] OR [[c1]] Is Between [[c2]] AND [[b]]",
                result);
        }

        // ------------------------------------------------------------------
        // Single-value mock to exercise the non-loop fast path of each arm.
        // ------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        [TestCategory(Category)]
        public void Dev2Decision_GenerateToolLabel_ThreeCols_AllStarred_SingleValue()
        {
            var dec = new Dev2Decision
            {
                Col1 = "[[recset(*).a]]",
                Col2 = "[[recset(*).b]]",
                Col3 = "[[recset(*).c]]",
                EvaluationFn = enDecisionType.IsEqual
            };

            var env = new Mock<IExecutionEnvironment>();
            env.Setup(e => e.EvalAsListOfStrings(It.IsAny<string>(), It.IsAny<int>()))
               .Returns(() => new List<string> { "[[only]]" });

            var result = dec.GenerateToolLabel(env.Object, Dev2DecisionMode.AND, out var error);

            // Single-element lists shrink to empty after the initial RemoveAt(0); the
            // inner loop performs zero iterations and the result is just the seed.
            Assert.AreEqual("If [[only]] = [[only]] AND [[only]]", result);
        }
    }
}
