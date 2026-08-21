/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusResultFunction — the GET /secure/servicebus-result/{id}
 *  polling endpoint for the secure Service Bus trigger's async response path.
 *
 *  Mirrors WorkflowResumeFunctionTests' approach: drives the route/response mapping
 *  directly against a real IServiceBusReplayAndResultStore (Hangfire-backed, via the
 *  store's test seam) without going through the full HTTP authorization middleware
 *  pipeline — that pipeline's behaviour is already covered by
 *  WorkflowAuthorizationMiddlewareTests / MiddlewarePipelineIntegrationTests and
 *  applies identically to this route (it carries the same
 *  [RequireWorkflowPermission(WorkflowPermission.View)] attribute as every other
 *  secure HTTP function).
 */

using Hangfire.MemoryStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Functions;

[TestClass]
public class ServiceBusResultFunctionTests
{
    private static async Task<(HttpStatusCode Status, JObject Body)> Invoke(
        IServiceBusReplayAndResultStore store, string correlationId)
    {
        var ctx = new HttpFunctionContext();
        var req = new FakeHttpRequestData(ctx, new Uri($"https://engine.test/secure/servicebus-result/{correlationId}"));
        var function = new ServiceBusResultFunction(store);

        var response = await function.GetResult(req, correlationId, ctx);

        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        var text = await reader.ReadToEndAsync();
        return (response.StatusCode, JObject.Parse(text));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task GetResult_NoResultRecorded_404_NotFound()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());

        var (status, body) = await Invoke(store, "unknown-correlation-id");

        Assert.AreEqual(HttpStatusCode.NotFound, status);
        Assert.AreEqual("not_found", body["error"]?.ToString());
        Assert.AreEqual("unknown-correlation-id", body["correlationId"]?.ToString());
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task GetResult_SucceededResult_200_WithOutputsParsedAsJson()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        store.SaveResult(new ServiceBusTriggerResult
        {
            CorrelationId = "corr-ok",
            Status = ServiceBusTriggerStatus.Succeeded,
            Workflow = "Hello World",
            Caller = "alice@example.com",
            Outputs = "{\"Greeting\":\"Hi Bob\"}",
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        });

        var (status, body) = await Invoke(store, "corr-ok");

        Assert.AreEqual(HttpStatusCode.OK, status);
        Assert.AreEqual("Succeeded", body["status"]?.ToString());
        Assert.AreEqual("Hello World", body["workflow"]?.ToString());
        Assert.AreEqual("alice@example.com", body["caller"]?.ToString());
        Assert.AreEqual("Hi Bob", body["outputs"]?["Greeting"]?.ToString());
        Assert.IsNull(body["error"]);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task GetResult_DeniedResult_200_WithErrorReason_NoOutputs()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        store.SaveResult(new ServiceBusTriggerResult
        {
            CorrelationId = "corr-denied",
            Status = ServiceBusTriggerStatus.Denied,
            Workflow = "Admin Only",
            Caller = "bob@example.com",
            Error = "Denied by workflow authorization policy.",
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        });

        var (status, body) = await Invoke(store, "corr-denied");

        Assert.AreEqual(HttpStatusCode.OK, status);
        Assert.AreEqual("Denied", body["status"]?.ToString());
        Assert.AreEqual("Denied by workflow authorization policy.", body["error"]?.ToString());
        Assert.IsNull(body["outputs"]);
    }
}
