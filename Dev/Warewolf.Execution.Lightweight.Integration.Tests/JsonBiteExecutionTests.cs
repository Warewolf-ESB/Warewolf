using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.Bite.Json;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Runs a JSON &lt;BiteDefinition&gt; resource (the WF-free format) through the real executor, no server involved.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class JsonBiteExecutionTests
    {
        sealed class NoOpLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }

        static string HelloWorldXaml() => File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Resources", "hangfiredemo", "Hello World.bite"));

        static WorkflowExecutionResult Run(string resource, string input)
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bite");
            File.WriteAllText(path, resource);
            try
            {
                return new WorkflowExecutor(new NoOpLogger())
                    .Execute(new WorkflowExecutionRequest { WorkflowFilePath = path, RawInputPayload = input });
            }
            finally { File.Delete(path); }
        }

        [TestMethod]
        public void JsonBody_ExecutesLikeItsXamlSource()
        {
            var xaml = HelloWorldXaml();
            var fromXaml = Run(xaml, "{\"Name\":\"Wolf\"}");
            var fromJson = Run(BiteJsonWriter.ConvertResource(xaml), "{\"Name\":\"Wolf\"}");

            Assert.IsTrue(fromXaml.IsSuccess, string.Join("; ", fromXaml.Errors));
            Assert.IsTrue(fromJson.IsSuccess, string.Join("; ", fromJson.Errors));
            var json = fromJson.ReadPayloadAsync().GetAwaiter().GetResult();
            Assert.AreEqual(fromXaml.ReadPayloadAsync().GetAwaiter().GetResult(), json);
            StringAssert.Contains(json, "Hello Wolf.");
        }

        [TestMethod]
        public void CorruptJsonBody_FailsCleanly()
        {
            var corrupt = BiteJsonWriter.ConvertResource(HelloWorldXaml()).Replace("\"start\": \"", "\"start\": \"zzz");
            Assert.IsTrue(corrupt.Contains("zzz"), "The corruption must actually change the body.");

            var result = Run(corrupt, "{\"Name\":\"Wolf\"}");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Count > 0);
        }
    }
}
