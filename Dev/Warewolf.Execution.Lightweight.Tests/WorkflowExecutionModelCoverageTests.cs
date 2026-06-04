/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    public class WorkflowExecutionRequestCoverageTests
    {
        [TestMethod]
        [TestCategory("WorkflowExecutionRequest_Coverage")]
        public void Defaults_AreSane()
        {
            var r = new WorkflowExecutionRequest();
            Assert.IsNull(r.WorkflowsDirectory);
            Assert.IsNull(r.WorkflowFilePath);
            Assert.IsNull(r.WorkflowName);
            Assert.IsNotNull(r.InputParameters);
            Assert.AreEqual(0, r.InputParameters.Count);
            Assert.IsFalse(r.IsDebug);
            Assert.AreEqual(EmitionTypes.JSON, r.ReturnType);
            Assert.IsNull(r.WebServerUri);
            Assert.IsFalse(r.IsValid);
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionRequest_Coverage")]
        public void IsValid_RequiresNonWhitespaceFilePath()
        {
            Assert.IsFalse(new WorkflowExecutionRequest { WorkflowFilePath = "" }.IsValid);
            Assert.IsFalse(new WorkflowExecutionRequest { WorkflowFilePath = "   " }.IsValid);
            Assert.IsTrue(new WorkflowExecutionRequest { WorkflowFilePath = "c:\\x.bite" }.IsValid);
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionRequest_Coverage")]
        public void Setters_RoundTrip()
        {
            var uri = new Uri("https://example.com/api");
            var r = new WorkflowExecutionRequest
            {
                WorkflowsDirectory = "C:\\dir",
                WorkflowFilePath = "C:\\dir\\wf.bite",
                WorkflowName = "MyWf",
                IsDebug = true,
                ReturnType = EmitionTypes.XML,
                WebServerUri = uri
            };
            r.InputParameters["a"] = "1";

            Assert.AreEqual("C:\\dir", r.WorkflowsDirectory);
            Assert.AreEqual("C:\\dir\\wf.bite", r.WorkflowFilePath);
            Assert.AreEqual("MyWf", r.WorkflowName);
            Assert.IsTrue(r.IsDebug);
            Assert.AreEqual(EmitionTypes.XML, r.ReturnType);
            Assert.AreSame(uri, r.WebServerUri);
            Assert.AreEqual("1", r.InputParameters["a"]);
            Assert.IsTrue(r.IsValid);
        }
    }

    [TestClass]
    public class WorkflowExecutionResultCoverageTests
    {
        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public void Defaults_AreSane()
        {
            var r = new WorkflowExecutionResult();
            Assert.IsNull(r.Payload);
            Assert.IsNull(r.ContentType);
            Assert.IsNull(r.PayloadWriter);
            Assert.IsFalse(r.IsSuccess);
            Assert.AreEqual(Guid.Empty, r.ExecutionId);
            Assert.IsNotNull(r.Outputs);
            Assert.IsNotNull(r.Errors);
            Assert.IsNotNull(r.DebugStates);
            Assert.AreEqual(TimeSpan.Zero, r.Duration);
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public async Task ReadPayloadAsync_NoWriter_FallsBackToPayloadString()
        {
            var r = new WorkflowExecutionResult { Payload = "hello" };
            Assert.AreEqual("hello", await r.ReadPayloadAsync());
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public async Task ReadPayloadAsync_NoWriterNoPayload_ReturnsEmpty()
        {
            var r = new WorkflowExecutionResult();
            Assert.AreEqual(string.Empty, await r.ReadPayloadAsync());
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public async Task ReadPayloadAsync_WithWriter_StreamsContent()
        {
            var r = new WorkflowExecutionResult();
            typeof(WorkflowExecutionResult)
                .GetProperty("PayloadWriter")!
                .SetValue(r, (Func<Stream, CancellationToken, Task>)(async (s, ct) =>
                {
                    var bytes = Encoding.UTF8.GetBytes("streamed!");
                    await s.WriteAsync(bytes, 0, bytes.Length, ct);
                }));

            Assert.AreEqual("streamed!", await r.ReadPayloadAsync());
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public void Failure_FactoryPopulatesError()
        {
            var r = WorkflowExecutionResult.Failure("boom");
            Assert.IsFalse(r.IsSuccess);
            Assert.AreEqual(1, r.Errors.Count);
            Assert.AreEqual("boom", r.Errors[0]);
            Assert.AreNotEqual(default(DateTime), r.StartTime);
            Assert.AreNotEqual(default(DateTime), r.EndTime);
        }

        [TestMethod]
        [TestCategory("WorkflowExecutionResult_Coverage")]
        public void Setters_RoundTrip()
        {
            var id = Guid.NewGuid();
            var start = DateTime.UtcNow.AddMinutes(-1);
            var end = DateTime.UtcNow;
            var r = new WorkflowExecutionResult
            {
                Payload = "p", ContentType = "application/json",
                IsSuccess = true, ExecutionId = id,
                Duration = TimeSpan.FromSeconds(2),
                StartTime = start, EndTime = end
            };
            r.Outputs["x"] = 42;
            r.Errors.Add("e");
            r.DebugStates.Add(new DebugStepResult { DisplayName = "step" });

            Assert.AreEqual("p", r.Payload);
            Assert.AreEqual("application/json", r.ContentType);
            Assert.IsTrue(r.IsSuccess);
            Assert.AreEqual(id, r.ExecutionId);
            Assert.AreEqual(TimeSpan.FromSeconds(2), r.Duration);
            Assert.AreEqual(start, r.StartTime);
            Assert.AreEqual(end, r.EndTime);
            Assert.AreEqual(42, r.Outputs["x"]);
            Assert.AreEqual("e", r.Errors[0]);
            Assert.AreEqual("step", r.DebugStates[0].DisplayName);
        }
    }

    [TestClass]
    public class LightweightExecutionTokenCoverageTests
    {
        [TestMethod]
        [TestCategory("LightweightExecutionToken_Coverage")]
        public void IsUserCanceled_DefaultsToFalse_AndIsMutable()
        {
            var t = new LightweightExecutionToken();
            Assert.IsFalse(t.IsUserCanceled);
            t.IsUserCanceled = true;
            Assert.IsTrue(t.IsUserCanceled);
        }
    }
}
