using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Warewolf.Bite;
using Warewolf.Corpus.Extractor;

// Usage: Warewolf.Corpus.Extractor <outDir> <set>=<folder> [<set>=<folder> ...]
// Writes <outDir>/workflows.jsonl (one row per kept workflow) and <outDir>/examples.jsonl (one row per decision).
if (args.Length < 2 || args.Skip(1).Any(a => !a.Contains('=')))
{
    Console.Error.WriteLine("usage: Warewolf.Corpus.Extractor <outDir> <set>=<folder> [<set>=<folder> ...]");
    return 2;
}

var outDir = Directory.CreateDirectory(args[0]).FullName;
var sets = args.Skip(1).Select(a => a.Split('=', 2)).Select(p => (Set: p[0], Folder: p[1])).ToList();

var read = new List<WorkflowFile>();
var unreadable = new List<string>();
foreach (var (set, folder) in sets)
{
    foreach (var file in Directory.EnumerateFiles(folder, "*.bite", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
    {
        try
        {
            if (CorpusReader.Read(set, file) is { } workflow)
            {
                read.Add(workflow);
            }
        }
        catch (Exception ex)
        {
            unreadable.Add($"{file}: {ex.Message}");
        }
    }
}

var kept = CorpusReader.Deduplicate(read);
var extractor = new BranchExtractor(EngineToolResolver.Resolve);
var json = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never };
var unknownCounts = new Dictionary<string, int>(StringComparer.Ordinal);
var failed = 0;
var exampleCount = 0;

using (var workflowsOut = new StreamWriter(Path.Combine(outDir, "workflows.jsonl"), false, new UTF8Encoding(false)))
using (var examplesOut = new StreamWriter(Path.Combine(outDir, "examples.jsonl"), false, new UTF8Encoding(false)))
{
    foreach (var workflow in kept)
    {
        GraphExamples? graph = null;
        string? error = null;
        try
        {
            graph = extractor.Extract(BiteFormat.XamlToX6(workflow.XamlDefinition, workflow.Id));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            failed++;
        }

        // Workflows with the same branch structure form one family, so near-copies never straddle the train/eval split.
        var structure = graph is null ? "" : string.Join("\n", graph.Examples.Select(e => $"{string.Join(">", e.Path)}|{e.Arm}|{e.Next}"));
        var family = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(structure)))[..12];

        workflowsOut.WriteLine(JsonSerializer.Serialize(new
        {
            set = workflow.Set,
            id = workflow.Id,
            name = workflow.Name,
            comment = workflow.Comment,
            version = workflow.VersionNumber,
            file = workflow.FilePath,
            family,
            steps = graph?.Examples.Count(e => e.Next != Example.End) ?? 0,
            unknownTypes = graph?.UnknownTypes ?? [],
            nestedSkipped = graph?.NestedSkipped ?? 0,
            merges = graph?.Merges ?? 0,
            error,
        }, json));

        foreach (var example in graph?.Examples ?? [])
        {
            examplesOut.WriteLine(JsonSerializer.Serialize(new { set = workflow.Set, id = workflow.Id, family, example.Path, example.Arm, example.Next }, json));
            exampleCount++;
        }

        foreach (var type in graph?.UnknownTypes ?? [])
        {
            unknownCounts[type] = unknownCounts.GetValueOrDefault(type) + 1;
        }
    }
}

// The catalog System 1 chooses from, as list_tools would report it, so the harness and the corpus agree on names.
File.WriteAllText(Path.Combine(outDir, "tools.json"), JsonSerializer.Serialize(
    Warewolf.Execution.Lightweight.Mcp.ToolCatalog.Entries.Select(entry => new
    {
        name = entry.Name,
        category = entry.Category,
        description = entry.Description,
        branchKind = (EngineToolResolver.Resolve(entry.DataTypes[0], null)?.BranchKind ?? BranchKind.None).ToString().ToUpperInvariant(),
    }),
    new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine($"read {read.Count} workflows, kept {kept.Count} after de-duplication by Service ID");
Console.WriteLine($"converted {kept.Count - failed}, failed {failed}, unreadable files {unreadable.Count}");
Console.WriteLine($"examples {exampleCount}");
foreach (var (type, count) in unknownCounts.OrderByDescending(kv => kv.Value))
{
    Console.WriteLine($"unknown type {type}: {count} workflow(s)");
}

foreach (var line in unreadable)
{
    Console.Error.WriteLine($"unreadable {line}");
}

return 0;
