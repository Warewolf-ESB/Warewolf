/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure
{
    /// <summary>
    /// Coverage for <see cref="ThreadPoolStartupConfigurator"/> — the WOLF-8512 mitigation for
    /// a small number of CPU-heavy concurrent Service Bus trigger executions starving the CLR
    /// ThreadPool badly enough that even the trigger's own <c>ExecutionTimeout</c> safety net
    /// (a <c>Task.Delay</c>-based race) never gets scheduled.
    ///
    /// <see cref="ThreadPool.SetMinThreads"/> mutates real, process-wide state with no seam to
    /// fake it — the whole point of the class under test is that side effect — so every test
    /// here restores the pre-test values in a <c>finally</c> block and the class is
    /// <see cref="DoNotParallelizeAttribute"/>-marked, matching the convention already used in
    /// <c>LoggingConfigurationTests</c> for the same reason.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class ThreadPoolStartupConfiguratorTests
    {
        // ── ComputeMinThreads (pure function) ───────────────────────────────────

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(4)]
        public void ComputeMinThreads_SmallConcurrency_FloorsOnProcessorCount(int maxConcurrentExecutions)
        {
            var (worker, completionPort) = ThreadPoolStartupConfigurator.ComputeMinThreads(maxConcurrentExecutions);

            Assert.IsTrue(worker >= Environment.ProcessorCount * 2,
                $"Expected worker floor to be at least 2x processor count ({Environment.ProcessorCount}), got {worker}.");
            Assert.IsTrue(completionPort >= Environment.ProcessorCount,
                $"Expected completion-port floor to be at least processor count ({Environment.ProcessorCount}), got {completionPort}.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ComputeMinThreads_LargeConcurrency_ScalesWithExecutionCount()
        {
            // Large enough that the per-execution multiplier dominates over the
            // processor-count floor regardless of the test machine's core count.
            const int maxConcurrentExecutions = 64;

            var (worker, completionPort) = ThreadPoolStartupConfigurator.ComputeMinThreads(maxConcurrentExecutions);

            Assert.AreEqual(maxConcurrentExecutions * 4, worker);
            Assert.AreEqual(maxConcurrentExecutions * 2, completionPort);
        }

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow(0)]
        [DataRow(-5)]
        public void ComputeMinThreads_NonPositiveInput_TreatedAsOne(int maxConcurrentExecutions)
        {
            var atZeroOrNegative = ThreadPoolStartupConfigurator.ComputeMinThreads(maxConcurrentExecutions);
            var atOne = ThreadPoolStartupConfigurator.ComputeMinThreads(1);

            Assert.AreEqual(atOne, atZeroOrNegative);
        }

        // ── Configure (mutates real ThreadPool state) ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Configure_NeverLowersBelowExistingRuntimeDefaults()
        {
            ThreadPool.GetMinThreads(out var originalWorker, out var originalCompletionPort);
            try
            {
                // maxConcurrentExecutions=1 drives the smallest possible computed floor, so this
                // asserts the "never lower" guarantee rather than the "raise" behaviour below.
                ThreadPoolStartupConfigurator.Configure(1);

                ThreadPool.GetMinThreads(out var afterWorker, out var afterCompletionPort);
                Assert.IsTrue(afterWorker >= originalWorker,
                    $"Expected worker min threads to never drop below {originalWorker}, got {afterWorker}.");
                Assert.IsTrue(afterCompletionPort >= originalCompletionPort,
                    $"Expected completion-port min threads to never drop below {originalCompletionPort}, got {afterCompletionPort}.");
            }
            finally
            {
                ThreadPool.SetMinThreads(originalWorker, originalCompletionPort);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Configure_RaisesWhenComputedFloorExceedsCurrent()
        {
            ThreadPool.GetMinThreads(out var originalWorker, out var originalCompletionPort);
            try
            {
                // Deliberately below whatever this floor would compute to, so the "raise" branch
                // is guaranteed to be exercised regardless of the test machine's current defaults.
                ThreadPool.SetMinThreads(1, 1);

                const int maxConcurrentExecutions = 8;
                var (expectedWorker, expectedCompletionPort) =
                    ThreadPoolStartupConfigurator.ComputeMinThreads(maxConcurrentExecutions);
                ThreadPoolStartupConfigurator.Configure(maxConcurrentExecutions);

                ThreadPool.GetMinThreads(out var afterWorker, out var afterCompletionPort);
                Assert.AreEqual(expectedWorker, afterWorker);
                Assert.AreEqual(expectedCompletionPort, afterCompletionPort);
            }
            finally
            {
                ThreadPool.SetMinThreads(originalWorker, originalCompletionPort);
            }
        }
    }
}
