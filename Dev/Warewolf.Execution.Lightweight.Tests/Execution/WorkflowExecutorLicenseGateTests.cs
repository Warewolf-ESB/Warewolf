using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    /// <summary>
    /// Tests for the license/subscription gate in <see cref="WorkflowExecutor"/>.
    /// The gate is controlled by WAREWOLF_LICENSE_CHECK_ENABLED env var.
    /// </summary>
    [TestClass]
    public class WorkflowExecutorLicenseGateTests
    {
        private string _tempWorkflowFile = null!;

        [TestInitialize]
        public void Setup()
        {
            // Create a minimal valid workflow XML so we get past file-exists validation
            _tempWorkflowFile = Path.GetTempFileName();
            File.WriteAllText(_tempWorkflowFile, @"<?xml version=""1.0"" encoding=""utf-8""?>
<Service Name=""Test"" ResourceType=""WorkflowService"" ID=""00000000-0000-0000-0000-000000000001"">
  <Actions>
    <Action Type=""Workflow"">
      <XamlDefinition></XamlDefinition>
    </Action>
  </Actions>
  <DataList></DataList>
</Service>");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", null);
            try { File.Delete(_tempWorkflowFile); } catch { }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_LicenseCheckDisabledViaFalse_DoesNotBlockExecution()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            // Should NOT fail with "license/subscription" message (may fail for other reasons like empty XAML)
            Assert.IsFalse(result.Errors.Any(e => e.Contains("license", StringComparison.OrdinalIgnoreCase)),
                $"Expected no license error but got: {string.Join("; ", result.Errors)}");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_LicenseCheckDisabledViaZero_DoesNotBlockExecution()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "0");

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            Assert.IsFalse(result.Errors.Any(e => e.Contains("license", StringComparison.OrdinalIgnoreCase)),
                $"Expected no license error but got: {string.Join("; ", result.Errors)}");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_LicenseCheckEnabled_NoValidLicense_BlocksExecution()
        {
            // Explicitly enable (or leave default which is enabled)
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "true");

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            // Should fail with license-related message (either "required" or "unable to validate")
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(
                result.Errors.Any(e => e.Contains("license", StringComparison.OrdinalIgnoreCase))
                || result.Errors.Any(e => e.Contains("subscription", StringComparison.OrdinalIgnoreCase)),
                $"Expected license/subscription error but got: {string.Join("; ", result.Errors)}");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_LicenseCheckDefault_EnvVarNotSet_DefaultsToEnabled()
        {
            // Remove env var entirely — default is enabled
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", null);

            var executor = new WorkflowExecutor(new NullExecutionLogger());
            var request = new WorkflowExecutionRequest { WorkflowFilePath = _tempWorkflowFile };

            var result = executor.Execute(request);

            // Should block because no valid license in test environment
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(
                result.Errors.Any(e => e.Contains("license", StringComparison.OrdinalIgnoreCase))
                || result.Errors.Any(e => e.Contains("subscription", StringComparison.OrdinalIgnoreCase)),
                $"Expected license/subscription error but got: {string.Join("; ", result.Errors)}");
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
