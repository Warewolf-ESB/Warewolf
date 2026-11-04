using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Warewolf.Execution.ServiceBusWorker.E2EHarness;

/// <summary>
/// Reads <c>--data-file</c>: a JSON array of flat workflow-input records, one message per record.
/// <para>
/// The alternative to <c>--workflow-inputs-json</c> for a run that must replay real-world data
/// rather than one templated body. The same file format <c>Publish-WwQueueBurst.ps1
/// -DataFilePath</c> reads, so one data file drives both load-test jobs. Each record is published
/// exactly as written - <see cref="WorkflowInputTemplate"/> placeholders are NOT expanded in it,
/// so a real value can never be rewritten by accident.
/// </para>
/// </summary>
internal static class WorkflowInputDataFile
{
    /// <summary>
    /// Returns each record as a compact JSON string map, in file order. Every value must be a JSON
    /// string, as the workflow-trigger message contract's <c>inputs</c> map requires.
    /// </summary>
    /// <exception cref="FileNotFoundException">The file does not exist.</exception>
    /// <exception cref="InvalidOperationException">The file is empty or a record is not a flat string map.</exception>
    internal static IReadOnlyList<string> Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"--data-file '{path}' not found.", path);
        }

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"--data-file '{path}' must be a JSON array of records.");
        }

        var records = new List<string>(doc.RootElement.GetArrayLength());
        var index = 0;
        foreach (var element in doc.RootElement.EnumerateArray())
        {
            index++;
            if (element.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException($"Record {index} in '{path}' is not a JSON object.");
            }

            var inputs = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.String)
                {
                    throw new InvalidOperationException($"Record {index} in '{path}': field '{property.Name}' is {property.Value.ValueKind}, not a string.");
                }
                inputs[property.Name] = property.Value.GetString()!;
            }
            records.Add(JsonSerializer.Serialize(inputs));
        }

        if (records.Count == 0)
        {
            throw new InvalidOperationException($"--data-file '{path}' contains no records.");
        }
        return records;
    }

    /// <summary>
    /// The message count a run over <paramref name="recordCount"/> records implies. An explicit
    /// <c>--message-count</c> that disagrees is an error rather than a silent truncation: sending
    /// part of the file would make the reconciliation report the rest as lost.
    /// </summary>
    internal static int ResolveMessageCount(int recordCount, string? explicitMessageCount)
    {
        if (!string.IsNullOrWhiteSpace(explicitMessageCount)
            && int.Parse(explicitMessageCount) != recordCount)
        {
            throw new ArgumentException($"--message-count {explicitMessageCount} disagrees with the {recordCount} record(s) in --data-file; omit --message-count, the data file sizes the run.");
        }
        return recordCount;
    }

    /// <summary>
    /// Pairs each correlationId with the record published under it (record <c>i</c> goes out as
    /// <c>correlationIds[i]</c>).
    /// </summary>
    internal static IReadOnlyDictionary<string, string> PairWithCorrelationIds(IReadOnlyList<string> records, IReadOnlyList<string> correlationIds)
    {
        if (records.Count != correlationIds.Count)
        {
            throw new ArgumentException($"{records.Count} record(s) cannot be paired with {correlationIds.Count} correlationId(s).");
        }
        return correlationIds.Select((id, i) => (id, record: records[i])).ToDictionary(p => p.id, p => p.record, StringComparer.Ordinal);
    }
}
