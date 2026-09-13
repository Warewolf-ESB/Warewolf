using Dev2.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Result of an <c>execute_test</c> run — the test-execution counterpart of
    /// <see cref="WorkflowExecutionResult"/>. Carries the post-execution
    /// <see cref="IServiceTestModelTO"/> (whose <c>TestSteps[].Result</c>/<c>Outputs[].Result</c>
    /// were populated in place by the shared <c>Dev2.Activities</c> assertion engine and by
    /// <c>TestOutputEvaluator</c>) so <c>ExecuteTestTool</c> can shape the full per-step/per-output
    /// breakdown for its response.
    /// </summary>
    public class TestExecutionResult
    {
        /// <summary>Whether the test run itself completed (not whether the test passed) — mirrors
        /// <see cref="WorkflowExecutionResult.IsSuccess"/>'s "did execution succeed" meaning.
        /// A workflow that ran to completion but whose test assertions failed is still
        /// <c>IsSuccess = true</c>; see <see cref="Result"/>/<see cref="TestPassed"/> for the
        /// test's own pass/fail verdict.</summary>
        public bool IsSuccess { get; set; }

        /// <summary>The execution ID assigned to this test run.</summary>
        public Guid ExecutionId { get; set; }

        /// <summary>The test's name, echoed back for convenience.</summary>
        public string TestName { get; set; }

        /// <summary>Whether the test passed overall (steps + top-level outputs + error expectation).</summary>
        public bool TestPassed { get; set; }

        /// <summary>The test's overall <see cref="RunResult"/> classification.</summary>
        public RunResult Result { get; set; } = RunResult.None;

        /// <summary>Human-readable failure detail; empty when the test passed.</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The post-execution test definition — <c>TestSteps[].Result</c> and <c>Outputs[].Result</c>
        /// carry each step's/output's individual pass/fail detail.
        /// </summary>
        public IServiceTestModelTO ServiceTest { get; set; }

        /// <summary>Errors that occurred during the underlying workflow execution itself (not test
        /// assertion failures) — mirrors <see cref="WorkflowExecutionResult.Errors"/>.</summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>Per-activity debug states captured during the run, reusing
        /// <see cref="DebugStepResult.AssertResultList"/> for per-step assertion detail.</summary>
        public List<DebugStepResult> DebugStates { get; set; } = new();

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }

        /// <summary>Creates a failure result for a run that could not execute at all (e.g. the
        /// workflow failed to compile) — mirrors <see cref="WorkflowExecutionResult.Failure"/>.</summary>
        public static TestExecutionResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            TestPassed = false,
            Result = RunResult.TestInvalid,
            Message = errorMessage,
            Errors = new List<string> { errorMessage },
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
        };
    }
}
