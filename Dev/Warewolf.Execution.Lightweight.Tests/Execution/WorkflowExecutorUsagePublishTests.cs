/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests (8501) verifying WorkflowExecutor no longer publishes usage telemetry
 *  itself — it records the execution facts into the ambient UsagePublishContext
 *  for UsagePublishMiddleware to pick up and publish once the full request
 *  pipeline has completed. See UsagePublishMiddlewareTests for the publish-side
 *  contract.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class WorkflowExecutorUsagePublishTests
    {
        private string _tempWorkflowFile = null!;

        [TestInitialize]
        public void Setup()
        {
            // License gate would short-circuit before any usage context is
            // recorded, so disable it for these tests (mirrors WorkflowExecutorLicenseGateTests).
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
            UsagePublishContext.Current = null;
            _tempWorkflowFile = Path.GetTempFileName();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", null);
            UsagePublishContext.Current = null;
            try { File.Delete(_tempWorkflowFile); } catch { /* best effort */ }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_UnexpectedExceptionDuringXamlLoad_RecordsFailureIntoUsagePublishContext()
        {
            // A well-formed <XamlDefinition> element whose content is NOT a
            // resolvable Activity XAML type — ActivityXamlServices.Load throws,
            // landing in WorkflowExecutor's generic `catch (Exception ex)` block.
            File.WriteAllText(_tempWorkflowFile, @"<?xml version=""1.0"" encoding=""utf-8""?>
<Service Name=""BrokenWorkflow"" ResourceType=""WorkflowService"" ID=""00000000-0000-0000-0000-000000000002"">
  <Actions>
    <Action Type=""Workflow"">
      <XamlDefinition><NotARealActivityElement xmlns=""not-a-real-xaml-namespace"" /></XamlDefinition>
    </Action>
  </Actions>
  <DataList></DataList>
</Service>");

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            Assert.IsFalse(result.IsSuccess, "Malformed XAML must fail execution.");

            var usage = UsagePublishContext.Current;
            Assert.IsNotNull(usage,
                "WorkflowExecutor must record the execution facts into UsagePublishContext for UsagePublishMiddleware to publish, instead of calling an emitter directly.");
            // NOTE: the generic catch block uses Path.GetFileNameWithoutExtension(request.WorkflowFilePath),
            // NOT the resolved <Service Name="..."> value — this is the exact pre-existing
            // (pre-move) behaviour, faithfully preserved by this refactor.
            Assert.AreEqual(Path.GetFileNameWithoutExtension(_tempWorkflowFile), usage!.WorkflowName);
            Assert.AreEqual(result.ExecutionId, usage.ExecutionId);
            Assert.IsFalse(usage.IsSuccess);
            Assert.AreEqual(1, usage.ErrorCount);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_NoXamlDefinition_EarlyReturn_DoesNotRecordUsagePublishContext()
        {
            // Mirrors WorkflowExecutorLicenseGateTests' fixture: an empty
            // <XamlDefinition/> causes an early `return WorkflowExecutionResult.Failure(...)`
            // BEFORE the try block that records usage facts — matching the
            // pre-move behaviour where this path never called the usage emitter either.
            File.WriteAllText(_tempWorkflowFile, @"<?xml version=""1.0"" encoding=""utf-8""?>
<Service Name=""Test"" ResourceType=""WorkflowService"" ID=""00000000-0000-0000-0000-000000000001"">
  <Actions>
    <Action Type=""Workflow"">
      <XamlDefinition></XamlDefinition>
    </Action>
  </Actions>
  <DataList></DataList>
</Service>");

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(UsagePublishContext.Current,
                "The no-XamlDefinition early-return path must not record any usage facts (nothing for the middleware to publish), matching pre-move behaviour.");
        }

        /// <summary>Null logger for tests.</summary>
        private sealed class NullExecutionLogger : IExecutionLogger
        {
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
        }
    }
}
