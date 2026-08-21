/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins the WorkflowQueueTrigger message contract (route/workflow/inputs), the
 *  route-resolution defaulting/validation rules, and the poison-message contract
 *  (exceptions are never swallowed so the Functions runtime dead-letters the message).
 */

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Json;
using Warewolf.Execution.ServiceBusWorker;
using Warewolf.Execution.ServiceBusWorker.Functions;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

[TestClass]
public class WorkflowQueueTriggerTests
{
    static WorkflowQueueTrigger NewTrigger(Mock<IWwExecutionClient> client) =>
        new(client.Object, NullLogger<WorkflowQueueTrigger>.Instance);

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_ValidSecureMessage_CallsExecuteSecureAsync_WithParsedInputs()
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecuteSecureAsync("Hello World",
                It.Is<IDictionary<string, string?>>(d => d["Name"] == "FromServiceBus"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        var body = """{ "route": "secure", "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }""";
        await NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None);

        client.Verify(c => c.ExecuteSecureAsync("Hello World",
            It.Is<IDictionary<string, string?>>(d => d["Name"] == "FromServiceBus"),
            It.IsAny<CancellationToken>()), Times.Once);
        client.Verify(c => c.ExecutePublicAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_ValidPublicMessage_CallsExecutePublicAsync()
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecutePublicAsync("Hello World", It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        var body = """{ "route": "public", "workflow": "Hello World" }""";
        await NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None);

        client.Verify(c => c.ExecutePublicAsync("Hello World", It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_MissingRoute_DefaultsToSecure()
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecuteSecureAsync("Hello World", It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        var body = """{ "workflow": "Hello World" }""";
        await NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None);

        client.Verify(c => c.ExecuteSecureAsync("Hello World", It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DataRow("SECURE")]
    [DataRow("Public")]
    [DataRow(" secure ")]
    public async Task RunAsync_RouteCaseAndWhitespaceInsensitive_Accepted(string route)
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecuteSecureAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");
        client.Setup(c => c.ExecutePublicAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        var body = JsonSerializer.Serialize(new { route, workflow = "Hello World" });

        // Should not throw for any of these normalized values.
        await NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_UnsupportedRoute_ThrowsInvalidOperationException()
    {
        var client = new Mock<IWwExecutionClient>();
        var body = """{ "route": "ftp", "workflow": "Hello World" }""";

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None));

        client.VerifyNoOtherCalls();
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_MissingWorkflowField_ThrowsInvalidOperationException()
    {
        var client = new Mock<IWwExecutionClient>();
        var body = """{ "route": "secure" }""";

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_BlankWorkflowField_ThrowsInvalidOperationException()
    {
        var client = new Mock<IWwExecutionClient>();
        var body = """{ "workflow": "   " }""";

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_InvalidJson_ThrowsJsonException()
    {
        var client = new Mock<IWwExecutionClient>();
        var body = "{ not valid json";

        await Assert.ThrowsExceptionAsync<JsonException>(
            () => NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None));

        client.VerifyNoOtherCalls();
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_NoInputs_PassesEmptyDictionary()
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecuteSecureAsync("Hello World",
                It.Is<IDictionary<string, string?>>(d => d.Count == 0),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("{}");

        var body = """{ "workflow": "Hello World" }""";
        await NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None);

        client.Verify(c => c.ExecuteSecureAsync("Hello World",
            It.Is<IDictionary<string, string?>>(d => d.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task RunAsync_ClientThrows_ExceptionBubblesUnswallowed()
    {
        var client = new Mock<IWwExecutionClient>();
        client.Setup(c => c.ExecuteSecureAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("engine unreachable"));

        var body = """{ "workflow": "Hello World" }""";

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => NewTrigger(client).RunAsync(body, context: null!, CancellationToken.None));
        Assert.AreEqual("engine unreachable", ex.Message,
            "The trigger must let client failures bubble up unmodified — the Functions runtime relies on this to abandon/dead-letter the message.");
    }
}
