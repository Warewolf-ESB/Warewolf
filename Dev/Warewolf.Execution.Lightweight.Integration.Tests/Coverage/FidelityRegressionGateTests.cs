/*
 * Unit tests for FidelityRegressionGate — the rules that decide whether a round-trip fidelity
 * report constitutes a regression against the committed baseline.
 *
 * These run in milliseconds and execute no workflows. That is the point of extracting the gate:
 * the sweep itself costs ~60 workflow executions and roughly a minute, which is far too expensive
 * a way to find out that a comparison rule is wrong.
 *
 * The cases are drawn from real reports, and named after them, because both bugs this gate has had
 * looked reasonable in the abstract and only became obviously wrong once attached to an activity:
 *   - RabbitMQ Consume flipping to PassBothFailedIdentically because the agent has no broker
 *     (environment, not fidelity — must NOT fail the build)
 *   - FlowDecision flattened into a non-branching FlowStep, surfacing as ExecutionMismatch
 *     (fidelity — MUST fail the build)
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    [TestClass]
    [TestCategory("RoundTripFidelity")]
    public class FidelityRegressionGateTests
    {
        const string ReleaseRoot = "Resources - Release";
        const string LoadRoot = "Resources - Load";

        /// <summary>
        /// A root the sweep no longer scans. 'Server Tests Setup' really was dropped from
        /// CorpusRoots (it costs 22 minutes of connection timeouts for three non-passing rows),
        /// which makes it the honest example of the retired-root case.
        /// </summary>
        const string RetiredRoot = "Server Tests Setup";

        static string SampleIn(string root) =>
            @"C:\somewhere\Dev\" + root + @"\Resources\Examples\Sample.bite";

        static BaselineDocument Baseline(params (string Name, string Status, string SamplePath)[] rows) =>
            BaselineGeneratedAgainst(new List<string> { ReleaseRoot, LoadRoot }, rows);

        static BaselineDocument BaselineGeneratedAgainst(
            List<string> corpusRoots,
            params (string Name, string Status, string SamplePath)[] rows) =>
            new()
            {
                Harness = new HarnessProvenance
                {
                    HarnessVersion = FidelityRegressionGate.CurrentHarnessVersion,
                    MaxSamplesPerType = 8,
                    CorpusRoots = corpusRoots,
                },
                Results = rows
                    .Select(r => new BaselineEntry { StudioName = r.Name, Status = r.Status, SamplePath = r.SamplePath })
                    .ToList(),
            };

        static List<FidelityRow> Current(params (string Name, string Status)[] rows) =>
            rows.Select(r => new FidelityRow(r.Name, r.Status, SampleIn(ReleaseRoot), "detail for " + r.Name))
                .ToList();

        static readonly string[] OnlyReleasePresent = { ReleaseRoot };

        // ── IsRegression ──────────────────────────────────────────────────────

        [TestMethod]
        public void IsRegression_PassToPassBothFailedIdentically_IsNotARegression()
        {
            // The RabbitMQ/Service case: both sides still failed identically, so the round-trip
            // preserved behaviour. Only the sandbox changed.
            Assert.IsFalse(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.PassBothFailedIdentically)));
        }

        [TestMethod]
        public void IsRegression_PassToExecutionMismatch_IsARegression()
        {
            // The FlowDecision case this gate exists for.
            Assert.IsTrue(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.ExecutionMismatch)));
        }

        [TestMethod]
        public void IsRegression_PassToExecutionAsymmetric_IsARegression()
        {
            Assert.IsTrue(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.ExecutionAsymmetric)));
        }

        [TestMethod]
        public void IsRegression_PassToTranslationFailed_IsARegression()
        {
            Assert.IsTrue(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.TranslationFailed)));
        }

        [TestMethod]
        public void IsRegression_PassToStatusesCarryingNoInformation_IsNotARegression()
        {
            // Losing the only corpus sample, or discovering the workflow is non-deterministic,
            // says nothing about the converter either way.
            Assert.IsFalse(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.NoCorpusSample)));
            Assert.IsFalse(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.NonDeterministic)));
        }

        [TestMethod]
        public void IsRegression_BaselineNeverPassed_IsNeverARegression()
        {
            // One-directional by design: an activity that has never round-tripped cannot block CI.
            Assert.IsFalse(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.PassBothFailedIdentically), nameof(FidelityStatus.ExecutionMismatch)));
            Assert.IsFalse(FidelityRegressionGate.IsRegression(
                nameof(FidelityStatus.TranslationFailed), nameof(FidelityStatus.ExecutionMismatch)));
        }

        [TestMethod]
        public void IsImprovement_NonPassToPass_IsAnImprovement()
        {
            Assert.IsTrue(FidelityRegressionGate.IsImprovement(
                nameof(FidelityStatus.ExecutionMismatch), nameof(FidelityStatus.Pass)));
            Assert.IsFalse(FidelityRegressionGate.IsImprovement(
                nameof(FidelityStatus.Pass), nameof(FidelityStatus.Pass)));
        }

        // ── Compare ───────────────────────────────────────────────────────────

        [TestMethod]
        public void Compare_PassToPassBothFailedIdentically_ReportsNothing()
        {
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("RabbitMQ Consume", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot))),
                Current(("RabbitMQ Consume", nameof(FidelityStatus.PassBothFailedIdentically))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count, "a behaviour-preserving status must not fail the build");
            Assert.AreEqual(0, verdict.Improvements.Count);
            Assert.AreEqual(0, verdict.Skipped.Count);
        }

        [TestMethod]
        public void Compare_PassToExecutionMismatch_ReportsRegressionNamingTheActivityAndDetail()
        {
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("Decision", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot))),
                Current(("Decision", nameof(FidelityStatus.ExecutionMismatch))),
                OnlyReleasePresent);

            Assert.AreEqual(1, verdict.Regressions.Count);
            StringAssert.Contains(verdict.Regressions[0], "Decision");
            StringAssert.Contains(verdict.Regressions[0], nameof(FidelityStatus.ExecutionMismatch));
            StringAssert.Contains(verdict.Regressions[0], "detail for Decision",
                "the report detail is what tells a reviewer why it broke");
        }

        [TestMethod]
        public void Compare_NonPassToPass_IsAnImprovementNotAFailure()
        {
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("Assign", nameof(FidelityStatus.ExecutionMismatch), SampleIn(ReleaseRoot))),
                Current(("Assign", nameof(FidelityStatus.Pass))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count);
            Assert.AreEqual(1, verdict.Improvements.Count);
            StringAssert.Contains(verdict.Improvements[0], "Assign");
        }

        [TestMethod]
        public void Compare_BaselineRowAbsentFromThisRun_IsIgnored()
        {
            // A retired or renamed toolbox entry: the subset table is the source of truth, not the
            // baseline, so a row with no counterpart is neither a regression nor an improvement.
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("Retired Tool", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot))),
                Current(("Assign", nameof(FidelityStatus.Pass))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count);
            Assert.AreEqual(0, verdict.Improvements.Count);
            Assert.AreEqual(0, verdict.Skipped.Count);
        }

        [TestMethod]
        public void Compare_BaselineMeasuredAgainstAnAbsentCorpusRoot_IsSkippedNotScored()
        {
            // The RabbitMQ case as it actually reached CI: the baseline's sample lived in a root
            // the agent's TestBinaries artifact does not carry, so the agent scored a completely
            // different workflow and called the difference a regression.
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("RabbitMQ Publish", nameof(FidelityStatus.Pass), SampleIn(LoadRoot))),
                Current(("RabbitMQ Publish", nameof(FidelityStatus.ExecutionMismatch))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count,
                "the two runs did not measure the same workflow, so neither verdict is a true statement");
            Assert.AreEqual(1, verdict.Skipped.Count);
            StringAssert.Contains(verdict.Skipped[0], LoadRoot);
        }

        [TestMethod]
        public void Compare_BaselineMeasuredAgainstASinceRetiredCorpusRoot_IsStillSkipped()
        {
            // A root dropped from CorpusRoots is absent by definition, but it is also no longer in
            // the list the gate matches paths against — so without consulting the baseline's own
            // recorded roots it goes unrecognised, and an unrecognised root is treated as
            // comparable. That is backwards: retiring a root guarantees the two runs measured
            // different samples. This is the case that would silently start mis-scoring rows the
            // next time someone trims CorpusRoots.
            var verdict = FidelityRegressionGate.Compare(
                BaselineGeneratedAgainst(
                    new List<string> { ReleaseRoot, RetiredRoot },
                    ("Select and apply", nameof(FidelityStatus.Pass), SampleIn(RetiredRoot))),
                Current(("Select and apply", nameof(FidelityStatus.TranslationFailed))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count);
            Assert.AreEqual(1, verdict.Skipped.Count);
            StringAssert.Contains(verdict.Skipped[0], RetiredRoot);
        }

        [TestMethod]
        public void Compare_BaselineMeasuredAgainstAPresentCorpusRoot_IsStillScored()
        {
            // The skip is narrow: it must not become a way for genuine regressions to escape.
            var verdict = FidelityRegressionGate.Compare(
                Baseline(("Decision", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot))),
                Current(("Decision", nameof(FidelityStatus.ExecutionMismatch))),
                OnlyReleasePresent);

            Assert.AreEqual(1, verdict.Regressions.Count);
            Assert.AreEqual(0, verdict.Skipped.Count);
        }

        [TestMethod]
        public void Compare_BaselineWithNoResults_ReportsNothing()
        {
            var verdict = FidelityRegressionGate.Compare(
                new BaselineDocument(),
                Current(("Assign", nameof(FidelityStatus.ExecutionMismatch))),
                OnlyReleasePresent);

            Assert.AreEqual(0, verdict.Regressions.Count);
        }

        [TestMethod]
        public void Compare_ScoresEveryRowNotJustTheFirst()
        {
            var verdict = FidelityRegressionGate.Compare(
                Baseline(
                    ("Decision", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot)),
                    ("Switch", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot)),
                    ("Assign", nameof(FidelityStatus.ExecutionMismatch), SampleIn(ReleaseRoot))),
                Current(
                    ("Decision", nameof(FidelityStatus.ExecutionMismatch)),
                    ("Switch", nameof(FidelityStatus.TranslationFailed)),
                    ("Assign", nameof(FidelityStatus.Pass))),
                OnlyReleasePresent);

            Assert.AreEqual(2, verdict.Regressions.Count);
            Assert.AreEqual(1, verdict.Improvements.Count);
        }

        // ── Corpus root identification ────────────────────────────────────────

        [TestMethod]
        public void TryGetCorpusRoot_RecognisesEachKnownRootRegardlessOfMachine()
        {
            // The recorded path is absolute and written by whichever machine generated the report,
            // so only the root segment survives the trip between a dev checkout and an agent.
            Assert.IsTrue(FidelityRegressionGate.TryGetCorpusRoot(
                @"D:\a\1\WindowsTests\Resources - Release\Resources\Hello World.bite", out var ci));
            Assert.AreEqual(ReleaseRoot, ci);

            Assert.IsTrue(FidelityRegressionGate.TryGetCorpusRoot(
                @"C:\Users\dev\warewolf\Dev\Resources - ServerTests\Resources\Sample.bite", out var local));
            Assert.AreEqual("Resources - ServerTests", local);
        }

        [TestMethod]
        public void TryGetCorpusRoot_RetiredRoot_IsRecognisedOnlyWhenSupplied()
        {
            var retiredSample = SampleIn(RetiredRoot);

            Assert.IsFalse(FidelityRegressionGate.TryGetCorpusRoot(retiredSample, out _),
                RetiredRoot + " is no longer scanned, so the default overload must not claim it");

            Assert.IsTrue(FidelityRegressionGate.TryGetCorpusRoot(
                retiredSample, new[] { ReleaseRoot, RetiredRoot }, out var root));
            Assert.AreEqual(RetiredRoot, root);
        }

        [TestMethod]
        public void TryGetCorpusRoot_AcceptsForwardSlashes()
        {
            Assert.IsTrue(FidelityRegressionGate.TryGetCorpusRoot(
                "/mnt/agent/WindowsTests/Resources - Load/Resources/Sample.bite", out var root));
            Assert.AreEqual("Resources - Load", root);
        }

        [TestMethod]
        public void TryGetCorpusRoot_PrefersTheLongestMatchingRoot()
        {
            // 'Warewolf.Execution.Lightweight\Resources' is two segments; a one-segment root must
            // not shadow it, or its rows would be attributed to the wrong corpus.
            Assert.IsTrue(FidelityRegressionGate.TryGetCorpusRoot(
                @"C:\repo\Dev\Warewolf.Execution.Lightweight\Resources\tools\http get\httpbin.bite",
                out var root));
            StringAssert.Contains(root, "Warewolf.Execution.Lightweight");
        }

        [TestMethod]
        public void TryGetCorpusRoot_UnknownOrEmptyPath_ReturnsFalse()
        {
            Assert.IsFalse(FidelityRegressionGate.TryGetCorpusRoot(@"C:\elsewhere\Sample.bite", out _));
            Assert.IsFalse(FidelityRegressionGate.TryGetCorpusRoot(null, out _));
            Assert.IsFalse(FidelityRegressionGate.TryGetCorpusRoot("   ", out _));
        }

        // ── Failure message ───────────────────────────────────────────────────

        [TestMethod]
        public void BuildFailureMessage_NamesEveryRegressionAndHowToReBaseline()
        {
            var verdict = FidelityRegressionGate.Compare(
                Baseline(
                    ("Decision", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot)),
                    ("Switch", nameof(FidelityStatus.Pass), SampleIn(ReleaseRoot))),
                Current(
                    ("Decision", nameof(FidelityStatus.ExecutionMismatch)),
                    ("Switch", nameof(FidelityStatus.TranslationFailed))),
                OnlyReleasePresent);

            var message = FidelityRegressionGate.BuildFailureMessage(verdict);

            StringAssert.Contains(message, "Decision");
            StringAssert.Contains(message, "Switch");
            StringAssert.Contains(message, "fidelity-allowlist.json",
                "the message has to tell whoever hits it how to re-baseline");
        }
    }
}
