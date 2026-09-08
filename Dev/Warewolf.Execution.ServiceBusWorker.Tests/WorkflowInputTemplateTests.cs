/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins per-message expansion of --workflow-inputs-json in the ShovelBridge E2E harness:
 *  a bulk run must publish N distinct message bodies, because identical bodies serialise
 *  every execution behind one sp_getapplock in dbo.usp_jobs1_LogStart.
 */

using System;
using System.Collections.Generic;
using System.Linq;
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

    // ── {correlationGuid} (WOLF-8512) ────────────────────────────────────────────────────
    // ProfilerMain keys on MandateId, a SQL uniqueidentifier, so {correlationId} cannot be used
    // there — it is 39 chars and will not parse as a GUID. Without a per-message key reaching the
    // database there is nothing for Phase 4c to reconcile, which is why ProfilerMain could not be
    // verified per message the way RabbitProcess was.

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_GivenCorrelationGuidPlaceholder_SubstitutesAParseableGuid()
    {
        //------------Setup for test--------------------------
        const string template = @"{""MandateId"":""{correlationGuid}""}";

        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template, "648ddd78686d49c792c19bbf0806a1e6-000042");

        //------------Assert Results-------------------------
        Assert.AreEqual(@"{""MandateId"":""648ddd78-686d-49c7-92c1-9bbf08000042""}", result);
        var emitted = JsonSerializer.Deserialize<Dictionary<string, string>>(result)!["MandateId"];
        Assert.IsTrue(Guid.TryParse(emitted, out _), $"MandateId must parse as a uniqueidentifier: {emitted}");
    }

    /// <summary>
    /// Pins the exact shape: 26 hex characters of the run prefix, then the 6-digit sequence,
    /// dashed 8-4-4-4-12. Test-ShovelBridgeE2E.ps1's Get-WwCorrelationGuidStem reproduces this
    /// to build the reconciliation query, and asserts the same literal.
    /// </summary>
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void ToCorrelationGuid_TakesTwentySixHexOfPrefixThenTheSixDigitSequence()
    {
        //------------Execute Test---------------------------
        var guid = WorkflowInputTemplate.ToCorrelationGuid("648ddd78686d49c792c19bbf0806a1e6-000042");

        //------------Assert Results-------------------------
        Assert.AreEqual("648ddd78-686d-49c7-92c1-9bbf08000042", guid);
        StringAssert.StartsWith(guid.Replace("-", string.Empty), "648ddd78686d49c792c19bbf08",
            "The first 26 hex characters must come from the run's correlationId prefix.");
        StringAssert.EndsWith(guid, "000042", "The last 6 characters must be the message sequence.");
    }

    /// <summary>Per-MESSAGE uniqueness: this is what makes Phase 4c's 1:1 reconciliation possible.</summary>
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void ToCorrelationGuid_DifferentSequences_ProduceDistinctGuids()
    {
        //------------Setup for test--------------------------
        const string prefix = "648ddd78686d49c792c19bbf0806a1e6";
        var correlationIds = Enumerable.Range(0, 1000).Select(i => $"{prefix}-{i:D6}").ToArray();

        //------------Execute Test---------------------------
        var guids = correlationIds.Select(WorkflowInputTemplate.ToCorrelationGuid).ToArray();

        //------------Assert Results-------------------------
        Assert.AreEqual(1000, guids.Distinct().Count(), "Every message in a run needs its own MandateId.");
        Assert.IsTrue(guids.All(g => Guid.TryParse(g, out _)), "Every emitted MandateId must be a valid GUID.");
    }

    /// <summary>
    /// Per-RUN uniqueness, and it is a correctness requirement rather than tidiness:
    /// Profiler.usp_InsertProfilerRecommendation is an upsert keyed on (MandateId, ForPaymentCycle),
    /// so reusing MandateIds across runs would return AlreadyExists=1, insert nothing, and make a
    /// perfectly healthy second run look like 1000 lost messages.
    /// </summary>
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void ToCorrelationGuid_SameSequenceInDifferentRuns_ProducesDistinctGuids()
    {
        //------------Execute Test---------------------------
        var runOne = WorkflowInputTemplate.ToCorrelationGuid("648ddd78686d49c792c19bbf0806a1e6-000042");
        var runTwo = WorkflowInputTemplate.ToCorrelationGuid("0f4f49d148c8400ab1c2d3e4f5a6b7c8-000042");

        //------------Assert Results-------------------------
        Assert.AreNotEqual(runOne, runTwo,
            "A later run must not reuse the previous run's MandateIds, or every upsert returns AlreadyExists=1.");
    }

    /// <summary>A single-message run has no -NNNNNN suffix; it must still emit a usable GUID.</summary>
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void ToCorrelationGuid_SingleMessageRunWithNoSequenceSuffix_DefaultsSequenceToZero()
    {
        //------------Execute Test---------------------------
        var guid = WorkflowInputTemplate.ToCorrelationGuid("648ddd78686d49c792c19bbf0806a1e6");

        //------------Assert Results-------------------------
        Assert.IsTrue(Guid.TryParse(guid, out _), $"Expected a parseable GUID, got '{guid}'.");
        StringAssert.EndsWith(guid, "000000", "With no sequence suffix the sequence defaults to zero.");
    }

    /// <summary>
    /// The value is interpolated straight into a workflow input that SQL Server will parse, so a
    /// malformed GUID must never be emitted even for correlationId shapes the harness does not
    /// currently generate.
    /// </summary>
    [DataTestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    [DataRow("short-000001")]
    [DataRow("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz-000001")]  // no hex characters at all
    [DataRow("648ddd78686d49c792c19bbf0806a1e6-1")]       // under-width sequence
    [DataRow("-000001")]
    public void ToCorrelationGuid_UnexpectedShapes_StillEmitAParseableGuid(string correlationId)
    {
        //------------Execute Test---------------------------
        var guid = WorkflowInputTemplate.ToCorrelationGuid(correlationId);

        //------------Assert Results-------------------------
        Assert.IsTrue(Guid.TryParse(guid, out _), $"'{correlationId}' produced a malformed GUID: '{guid}'.");
    }

    [DataTestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void ToCorrelationGuid_NullOrBlankCorrelationId_ReturnsTheEmptyGuid(string? correlationId)
    {
        //------------Execute Test---------------------------
        var guid = WorkflowInputTemplate.ToCorrelationGuid(correlationId);

        //------------Assert Results-------------------------
        Assert.AreEqual("00000000-0000-0000-0000-000000000000", guid);
    }

    /// <summary>
    /// The harness prints a prominent "all N messages carry an IDENTICAL inputs map" warning when
    /// no placeholder is present. A template using only {correlationGuid} does vary per message,
    /// so that warning must not fire.
    /// </summary>
    [DataTestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    [DataRow(@"{""MandateId"":""{correlationGuid}""}", true)]
    [DataRow(@"{""m"":""{correlationId}"",""MandateId"":""{correlationGuid}""}", true)]
    [DataRow(@"{""MandateId"":""{CorrelationGuid}""}", false)] // case-sensitive, as with {correlationId}
    public void HasCorrelationIdPlaceholder_AlsoRecognisesTheGuidPlaceholder(string template, bool expected)
    {
        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.HasCorrelationIdPlaceholder(template);

        //------------Assert Results-------------------------
        Assert.AreEqual(expected, result);
    }

    /// <summary>Regression: adding the GUID placeholder must not disturb the original one.</summary>
    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputTemplate")]
    public void Expand_BothPlaceholdersInOneTemplate_EachSubstitutesIndependently()
    {
        //------------Setup for test--------------------------
        const string template = @"{""trace"":""{correlationId}"",""MandateId"":""{correlationGuid}""}";

        //------------Execute Test---------------------------
        var result = WorkflowInputTemplate.Expand(template, "648ddd78686d49c792c19bbf0806a1e6-000042");

        //------------Assert Results-------------------------
        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(result)!;
        Assert.AreEqual("648ddd78686d49c792c19bbf0806a1e6-000042", map["trace"], "{correlationId} must be unchanged.");
        Assert.AreEqual("648ddd78-686d-49c7-92c1-9bbf08000042", map["MandateId"]);
    }
}
