/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Pins --data-file in the ShovelBridge E2E harness: one message per record, each record
 *  published exactly as written, and the file (not --message-count) sizing the run.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.ServiceBusWorker.E2EHarness;

namespace Warewolf.Execution.ServiceBusWorker.Tests;

/// <summary>
/// Covers <see cref="WorkflowInputDataFile"/>. A wrong record count or a rewritten value would not
/// fail loudly in the load test: the reconciliation would simply report the difference as lost
/// messages.
/// </summary>
[TestClass]
public class WorkflowInputDataFileTests
{
    string _dir = null!;

    [TestInitialize]
    public void Setup()
    {
        _dir = Path.Combine(Path.GetTempPath(), "wwdatafile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }
    }

    string Write(string json)
    {
        var path = Path.Combine(_dir, Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, json);
        return path;
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_GivenRecords_ReturnsOneCompactStringMapPerRecordInFileOrder()
    {
        var path = Write(@"[ { ""MandateId"": ""B"", ""ForPaymentCycle"": ""202610"" }, { ""MandateId"": ""A"", ""ForPaymentCycle"": ""202611"" } ]");

        var records = WorkflowInputDataFile.Load(path);

        Assert.AreEqual(2, records.Count);
        Assert.AreEqual(@"{""MandateId"":""B"",""ForPaymentCycle"":""202610""}", records[0]);
        Assert.AreEqual(@"{""MandateId"":""A"",""ForPaymentCycle"":""202611""}", records[1]);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_KeepsEmptyStringValues()
    {
        // An empty RelatedMandates is real data; dropping the field would change what binds.
        var path = Write(@"[{""MandateId"":""A"",""RelatedMandates"":"""",""FASGroup"":""""}]");

        var inputs = JsonSerializer.Deserialize<Dictionary<string, string>>(WorkflowInputDataFile.Load(path)[0])!;

        Assert.AreEqual(3, inputs.Count);
        Assert.AreEqual(string.Empty, inputs["RelatedMandates"]);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_DoesNotExpandPlaceholdersInRecordValues()
    {
        // Expansion is a template feature; a real value that happens to contain a placeholder
        // token must reach the workflow untouched.
        var path = Write(@"[{""Note"":""keep {correlationId} and {correlationGuid}""}]");

        var inputs = JsonSerializer.Deserialize<Dictionary<string, string>>(WorkflowInputDataFile.Load(path)[0])!;

        Assert.AreEqual("keep {correlationId} and {correlationGuid}", inputs["Note"]);
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    [ExpectedException(typeof(FileNotFoundException))]
    public void Load_GivenMissingFile_Throws()
    {
        WorkflowInputDataFile.Load(Path.Combine(_dir, "absent.json"));
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_GivenEmptyArray_Throws()
    {
        var ex = Assert.ThrowsException<InvalidOperationException>(() => WorkflowInputDataFile.Load(Write("[]")));
        StringAssert.Contains(ex.Message, "contains no records");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_GivenNonArrayRoot_Throws()
    {
        var ex = Assert.ThrowsException<InvalidOperationException>(() => WorkflowInputDataFile.Load(Write(@"{""MandateId"":""A""}")));
        StringAssert.Contains(ex.Message, "must be a JSON array");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_GivenNonObjectElement_NamesTheRecord()
    {
        var ex = Assert.ThrowsException<InvalidOperationException>(() => WorkflowInputDataFile.Load(Write(@"[{""MandateId"":""A""},""oops""]")));
        StringAssert.Contains(ex.Message, "Record 2");
        StringAssert.Contains(ex.Message, "not a JSON object");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void Load_GivenNonStringValue_NamesTheRecordAndField()
    {
        // The workflow-trigger contract's inputs map is string -> string; a number would be
        // rejected by the deserialiser mid-publish, after earlier messages had already gone out.
        var ex = Assert.ThrowsException<InvalidOperationException>(() => WorkflowInputDataFile.Load(Write(@"[{""MandateId"":""A""},{""MandateId"":""B"",""SourcePaymentId"":54733063}]")));
        StringAssert.Contains(ex.Message, "Record 2");
        StringAssert.Contains(ex.Message, "SourcePaymentId");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void ResolveMessageCount_WithoutExplicitCount_ReturnsTheRecordCount()
    {
        Assert.AreEqual(20000, WorkflowInputDataFile.ResolveMessageCount(20000, null));
        Assert.AreEqual(20000, WorkflowInputDataFile.ResolveMessageCount(20000, "  "));
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void ResolveMessageCount_WithMatchingExplicitCount_ReturnsIt()
    {
        Assert.AreEqual(3, WorkflowInputDataFile.ResolveMessageCount(3, "3"));
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void ResolveMessageCount_WithDisagreeingExplicitCount_Throws()
    {
        var ex = Assert.ThrowsException<ArgumentException>(() => WorkflowInputDataFile.ResolveMessageCount(3, "2"));
        StringAssert.Contains(ex.Message, "disagrees");
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void PairWithCorrelationIds_PairsRecordIWithCorrelationIdI()
    {
        var records = new[] { "r0", "r1", "r2" };
        var ids = new[] { "p-000000", "p-000001", "p-000002" };

        var pairs = WorkflowInputDataFile.PairWithCorrelationIds(records, ids);

        Assert.AreEqual("r0", pairs["p-000000"]);
        Assert.AreEqual("r2", pairs["p-000002"]);
        CollectionAssert.AreEqual(ids, pairs.Keys.ToArray());
    }

    [TestMethod]
    [Owner("Ashley Lewis")]
    [TestCategory("WorkflowInputDataFile")]
    public void PairWithCorrelationIds_GivenMismatchedCounts_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => WorkflowInputDataFile.PairWithCorrelationIds(new[] { "r0" }, new[] { "a", "b" }));
    }
}
