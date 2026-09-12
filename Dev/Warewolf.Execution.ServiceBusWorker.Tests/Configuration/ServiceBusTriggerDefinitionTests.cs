/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusTriggerDefinition — the reader that replaces
 *  WAREWOLF_SERVICEBUS_TRIGGER_QUEUE + the message body's "workflow" field with
 *  Settings/servicebustrigger.bite (WOLF-8517). A standalone copy of
 *  Warewolf.Execution.Lightweight.Auth.Models.ServiceBusTriggerDefinition — see that
 *  type's own tests for the Model A equivalent of every case here.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.ServiceBusWorker.Configuration;

namespace Warewolf.Execution.ServiceBusWorker.Tests.Configuration;

[TestClass]
[DoNotParallelize] // reads/writes the process-global AzureWebJobs.WorkflowQueueTrigger.Disabled env var
public class ServiceBusTriggerDefinitionTests
{
    private const string DisabledVar = "AzureWebJobs.WorkflowQueueTrigger.Disabled";

    private string? _tempSettingsDirectory;

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(DisabledVar, null);
        if (_tempSettingsDirectory is not null && Directory.Exists(_tempSettingsDirectory))
        {
            try { Directory.Delete(_tempSettingsDirectory, recursive: true); } catch { /* best effort */ }
        }
    }

    private string NewSettingsDirectory()
    {
        _tempSettingsDirectory = Path.Combine(Path.GetTempPath(), "wolf8517-sbtd-worker-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempSettingsDirectory);
        return _tempSettingsDirectory;
    }

    private static void WriteTriggerFile(string directory, string json) =>
        File.WriteAllText(Path.Combine(directory, ServiceBusTriggerDefinition.FileName), json);

    // ── Happy path ───────────────────────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_ValidFile_ParsesQueueNameAndWorkflowName()
    {
        var dir = NewSettingsDirectory();
        WriteTriggerFile(dir, """
            {
              "QueueName": "wwexecution-queue",
              "WorkflowName": "Hello World",
              "TriggerId": "1ac40da8-3b56-45f8-a1aa-00e6864db38b",
              "Name": "WwExecutionServiceBusWorkerTrigger",
              "ResourceId": "e83704af-0f03-47ce-89dd-ccf87e18adb5"
            }
            """);

        var definition = ServiceBusTriggerDefinition.Load(dir);

        Assert.IsNotNull(definition);
        Assert.AreEqual("wwexecution-queue", definition!.QueueName);
        Assert.AreEqual("Hello World", definition.WorkflowName);
        Assert.AreEqual(Guid.Parse("1ac40da8-3b56-45f8-a1aa-00e6864db38b"), definition.TriggerId);
        Assert.AreEqual("WwExecutionServiceBusWorkerTrigger", definition.Name);
        Assert.AreEqual(Guid.Parse("e83704af-0f03-47ce-89dd-ccf87e18adb5"), definition.ResourceId);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_CommentsAndTrailingCommas_AreTolerated()
    {
        var dir = NewSettingsDirectory();
        WriteTriggerFile(dir, """
            {
              // a comment
              "QueueName": "q",
              "WorkflowName": "wf",
            }
            """);

        var definition = ServiceBusTriggerDefinition.Load(dir);

        Assert.IsNotNull(definition);
        Assert.AreEqual("q", definition!.QueueName);
        Assert.AreEqual("wf", definition.WorkflowName);
    }

    // ── Disabled short-circuit ───────────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_TriggerDisabled_ReturnsNullWithoutTouchingDisk()
    {
        Environment.SetEnvironmentVariable(DisabledVar, "true");

        var definition = ServiceBusTriggerDefinition.Load(Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid()));

        Assert.IsNull(definition);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DataRow("false")]
    [DataRow("")]
    [DataRow(null)]
    public void Load_NotDisabled_RequiresTheFile(string? notDisabledValue)
    {
        Environment.SetEnvironmentVariable(DisabledVar, notDisabledValue);

        Assert.ThrowsException<ServiceBusTriggerConfigurationException>(
            () => ServiceBusTriggerDefinition.Load(Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid())));
    }

    // ── Failure modes ────────────────────────────────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_MissingFile_ThrowsActionableError()
    {
        var dir = NewSettingsDirectory();

        var ex = Assert.ThrowsException<ServiceBusTriggerConfigurationException>(
            () => ServiceBusTriggerDefinition.Load(dir));

        StringAssert.Contains(ex.Message, ServiceBusTriggerDefinition.FileName);
        StringAssert.Contains(ex.Message, "AzureWebJobs.WorkflowQueueTrigger.Disabled");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_MalformedJson_ThrowsActionableError()
    {
        var dir = NewSettingsDirectory();
        WriteTriggerFile(dir, "{ not valid json");

        var ex = Assert.ThrowsException<ServiceBusTriggerConfigurationException>(
            () => ServiceBusTriggerDefinition.Load(dir));

        StringAssert.Contains(ex.Message, "could not be parsed");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_MissingQueueName_ThrowsNamingTheMissingField()
    {
        var dir = NewSettingsDirectory();
        WriteTriggerFile(dir, """{ "WorkflowName": "Hello World" }""");

        var ex = Assert.ThrowsException<ServiceBusTriggerConfigurationException>(
            () => ServiceBusTriggerDefinition.Load(dir));

        StringAssert.Contains(ex.Message, nameof(ServiceBusTriggerDefinition.QueueName));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Load_MissingWorkflowName_ThrowsNamingTheMissingField()
    {
        var dir = NewSettingsDirectory();
        WriteTriggerFile(dir, """{ "QueueName": "wwexecution-queue" }""");

        var ex = Assert.ThrowsException<ServiceBusTriggerConfigurationException>(
            () => ServiceBusTriggerDefinition.Load(dir));

        StringAssert.Contains(ex.Message, nameof(ServiceBusTriggerDefinition.WorkflowName));
    }
}
