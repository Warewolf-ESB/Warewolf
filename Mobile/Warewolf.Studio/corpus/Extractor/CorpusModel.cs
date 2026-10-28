using System.Text.Json.Serialization;

namespace Warewolf.Corpus.Extractor;

/// <summary>How a placed tool changes the frontier; mirrors the Kotlin generator's <c>BranchKind</c>.</summary>
public enum BranchKind { None, Decision, Switch }

/// <summary>A tool name and how it branches, as <see cref="EngineToolResolver"/> resolves an X6 data type.</summary>
public sealed record ResolvedTool(string Name, BranchKind BranchKind);

/// <summary>
/// One System 1 decision: on a branch whose tools so far are <see cref="Path"/> (on <see cref="Arm"/> of the
/// last one, when it forks), the workflow's author placed <see cref="Next"/> — a tool name, <see cref="End"/>,
/// or <c>?&lt;dataType&gt;</c> for an activity the catalog does not know.
/// </summary>
public sealed record Example(
    [property: JsonPropertyName("path")] IReadOnlyList<string> Path,
    [property: JsonPropertyName("arm")] string? Arm,
    [property: JsonPropertyName("next")] string Next)
{
    public const string End = "END";
    public const string UnknownPrefix = "?";
}

/// <summary>What one workflow's graph produced, plus what the walk could not represent.</summary>
public sealed record GraphExamples(
    IReadOnlyList<Example> Examples,
    /// <summary>X6 data types with no <c>ToolCatalog</c> entry.</summary>
    IReadOnlyList<string> UnknownTypes,
    /// <summary>Activities inside containers (ForEach, Sequence, ...); System 1 places top-level steps only.</summary>
    int NestedSkipped,
    /// <summary>Edges into a step already placed on another branch; add_step cannot merge branches, so the branch ends there.</summary>
    int Merges);

/// <summary>A workflow resource as read from its .bite envelope.</summary>
public sealed record WorkflowFile(
    string Set,
    string FilePath,
    string Id,
    string Name,
    string Comment,
    int VersionNumber,
    DateTimeOffset VersionTimestamp,
    string XamlDefinition);
