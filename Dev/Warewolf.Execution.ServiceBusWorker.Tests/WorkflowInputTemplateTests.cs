/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins per-message expansion of --workflow-inputs-json in the ShovelBridge E2E harness:
 *  a bulk run must publish N distinct message bodies, because identical bodies serialise
 *  every execution behind one sp_getapplock in dbo.usp_jobs1_LogStart.
 */

using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.ServiceBusWorker.E2EHarness;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

/// <summary>
/// Covers per-message expansion of <c>--workflow-inputs-json</c>. The behaviour that matters for
/// the ShovelBridge load test is that N messages end up with N *distinct* bodies — identical
/// bodies serialise the whole run behind one sp_getapplock in dbo.usp_jobs1_LogStart.
/// </summary>
[TestClass]
public class WorkflowInputTemplateTests
{
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_GivenCorrelationIdPlaceholder_SubstitutesTheCorrelationId()
    {
        //------------Setup for test--------------------------
        const string template = @"{""message"":""loadtest-{correlationId}""}";

        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template, "abc123-000042");

        //------------Assert Results-------------------------
        Assert.AreEqual(@"{""message"":""loadtest-abc123-000042""}", result);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_GivenNoPlaceholder_ReturnsInputUnchanged()
    {
        //------------Setup for test--------------------------
        // The pre-existing single-message behaviour must be preserved byte-for-byte.
        const string template = @"{""message"":""loadtest""}";

        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template, "abc123");

        //------------Assert Results-------------------------
        Assert.AreEqual(template, result, "A literal inputs map must pass through untouched.");
    }

    [DataTestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Expand_GivenNullOrBlankInputs_ReturnsInputUnchanged(string? template)
    {
        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template!, "abc123");

        //------------Assert Results-------------------------
        Assert.AreEqual(template, result, "Blank inputs must not be altered - the harness omits the map entirely.");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_GivenMultiplePlaceholders_SubstitutesEveryOccurrence()
    {
        //------------Setup for test--------------------------
        const string template = @"{""a"":""{correlationId}"",""b"":""x-{correlationId}""}";

        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template, "id7");

        //------------Assert Results-------------------------
        Assert.AreEqual(@"{""a"":""id7"",""b"":""x-id7""}", result);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_ResultRemainsValidJsonDeserialisableToAStringMap()
    {
        //------------Setup for test--------------------------
        // Program.cs deserialises the expanded text into Dictionary<string,string>; expansion
        // must not corrupt the surrounding JSON document.
        const string template = @"{""message"":""loadtest-{correlationId}""}";

        //------------Execute Test---------------------------
        var expanded = WorkflowInputTemplate.Expand(template, "5b8c44934e2841b2a0fec0fe3993f9c6-000999");
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(expanded);

        //------------Assert Results-------------------------
        Assert.IsNotNull(map);
        Assert.AreEqual("loadtest-5b8c44934e2841b2a0fec0fe3993f9c6-000999", map!["message"]);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_AcrossAWholeLoadRun_ProducesDistinctBodiesForEveryMessage()
    {
        //------------Setup for test--------------------------
        // The property the load test actually depends on: 1000 correlationIds -> 1000 distinct
        // bodies -> 1000 distinct content hashes -> no applock serialisation in usp_jobs1_LogStart.
        const string template = @"{""message"":""loadtest-{correlationId}""}";
        var correlationIds = Enumerable.Range(0, 1000).Select(i => $"prefix-{i:D6}").ToArray();

        //------------Execute Test---------------------------
        var bodies = correlationIds.Select(id => WorkflowInputTemplate.Expand(template, id)).ToArray();

        //------------Assert Results-------------------------
        Assert.AreEqual(1000, bodies.Distinct().Count(), "Every message in a load run must have a distinct body.");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_WithoutPlaceholderAcrossAWholeLoadRun_ProducesIdenticalBodies()
    {
        //------------Setup for test--------------------------
        // Characterises the regression this change fixes: the old literal config gave 1000
        // byte-identical bodies, which is what serialised the run.
        const string template = @"{""message"":""loadtest""}";
        var correlationIds = Enumerable.Range(0, 1000).Select(i => $"prefix-{i:D6}").ToArray();

        //------------Execute Test---------------------------
        var bodies = correlationIds.Select(id => WorkflowInputTemplate.Expand(template, id)).ToArray();

        //------------Assert Results-------------------------
        Assert.AreEqual(1, bodies.Distinct().Count(), "Without the placeholder every message body is identical.");
    }

    [DataTestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    [DataRow(@"{""message"":""loadtest-{correlationId}""}", true)]
    [DataRow(@"{""message"":""loadtest""}", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    [DataRow(@"{""message"":""{CorrelationId}""}", false)] // case-sensitive: must match the JSON field name
    public void HasCorrelationIdPlaceholder_DetectsThePlaceholderExactly(string? template, bool expected)
    {
        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.HasCorrelationIdPlaceholder(template!);

        //------------Assert Results-------------------------
        Assert.AreEqual(expected, result);
    }
}
