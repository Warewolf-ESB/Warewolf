using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Corpus.Extractor;

/// <summary>
/// Maps an X6 cell's <c>data.type</c> to a tool name with the engine's own <see cref="ToolCatalog"/>, aliases
/// included, then falls back to the legacy activities older workflows still hold. The X6 converter has no shape
/// for those, so their <c>data.type</c> is the assembly-qualified CLR type and their public properties sit in
/// <c>data.properties</c> (<c>WorkflowToX6Converter.CreateActivityNode</c>).
/// </summary>
public static class EngineToolResolver
{
    /// <summary>Legacy activity → its modern catalog tool, where the replacement is direct.</summary>
    static readonly Dictionary<string, string> Legacy = new(StringComparer.Ordinal)
    {
        ["DsfMultiAssignActivity"] = "Assign",
        ["DsfMultiAssignObjectActivity"] = "Assign Object",
        ["DsfWebPostActivity"] = "POST Web Method",
        ["WebPostActivity"] = "POST Web Method",
        // DsfWebGetRequestWithTimeoutActivity (Web Request) is this activity's direct successor.
        ["DsfWebGetRequestActivity"] = "Web Request",
        ["DsfCountRecordsetActivity"] = "Count Records",
        ["DsfRecordsetLengthActivity"] = "Length",
    };

    public static ResolvedTool? Resolve(string dataType, JObject? properties)
    {
        if (ToolCatalog.Resolve(dataType) is { } entry)
        {
            return new ResolvedTool(entry.Name, KindOf(entry.Name));
        }

        var clrName = ClrTypeName(dataType);
        if (clrName == "DsfActivity")
        {
            // The generic resource call; every one in the corpus calls a workflow. Other resource types stay unknown.
            // Its Type property is not serialized into data.properties, IsWorkflow is.
            return properties?["IsWorkflow"]?.Value<string>() == "True" ? new ResolvedTool("Service (sub-workflow)", BranchKind.None) : null;
        }

        return Legacy.TryGetValue(clrName, out var name) ? new ResolvedTool(name, BranchKind.None) : null;
    }

    /// <summary><c>Ns.Type, Assembly, Version=…</c> → <c>Type</c>.</summary>
    static string ClrTypeName(string assemblyQualifiedName)
    {
        var fullName = assemblyQualifiedName.Split(',', 2)[0];
        return fullName[(fullName.LastIndexOf('.') + 1)..];
    }

    static BranchKind KindOf(string name) => name switch
    {
        "Decision" or "Decision (legacy)" => BranchKind.Decision,
        "Switch" => BranchKind.Switch,
        _ => BranchKind.None,
    };
}
