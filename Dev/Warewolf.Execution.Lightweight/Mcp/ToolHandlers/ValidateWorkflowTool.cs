/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;
using Dev2.Data.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ModelContextProtocol;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>validate_workflow</c> MCP tool (<c>warewolf-lee-mcp-v3-spec.md</c>,
/// "Tools" § <c>validate_workflow</c>): checks an <c>envelope</c> + <c>body</c> pair for
/// structural and semantic validity without saving or running it, against the X6 graph shape.
///
/// <para>
/// <b>Envelope shape (deliberate divergence from <see cref="GetWorkflowDefinitionTool"/>'s
/// output).</b> <see cref="GetWorkflowDefinitionTool"/>'s <c>WorkflowEnvelope</c> flattens
/// inputs/outputs to bracket-notation strings (e.g. <c>"[[Recordset(*).Field]]"</c>, per
/// <c>DataListTO</c>) because that is what's cheap to derive from an existing workflow's
/// <c>DataList</c> XML. <c>get_workflow_schema</c>'s own <c>envelope_schema</c> — the contract
/// this tool (and, later, <c>create_workflow</c>/<c>edit_workflow</c>) actually exposes to an
/// MCP caller — documents the richer <c>{ kind, name, fields }</c> shape instead, because only
/// that shape can distinguish a <c>recordset</c> entry's declared field names from a plain
/// <c>scalar</c>/<c>object</c> entry — required by the "recordset field references resolve to a
/// <c>fields</c> entry declared on that recordset's envelope entry" check below. This handler
/// therefore parses <c>envelope.inputs</c>/<c>outputs</c> as arrays of <see cref="EnvelopeVariable"/>,
/// not strings.
/// </para>
///
/// <para>
/// <b>Checks performed, in order</b> (spec bullets, verbatim order):
/// <list type="number">
/// <item>Body must be valid JSON matching <c>X6WorkflowSaveModel</c> (<c>resourcename</c> +
/// <c>cells</c>) — a parse failure is a single structured error, never a thrown exception.</item>
/// <item>The graph must contain a node with <c>data.type == "start"</c> — mirrors
/// <c>Flowchart.StartNode</c>; missing start is a structured error (the same condition
/// <see cref="EmptyWorkflowGraphException"/> now guards in the converter itself, see
/// <c>warewolf-lee-mcp-v3-addendum-a.md</c> §1).</item>
/// <item>Every non-edge, non-start cell's <c>data.type</c> must resolve via
/// <see cref="ToolCatalog.Resolve"/> — an unresolved type is a hard error naming the cell
/// (closes the converter's former silent "Unknown type" fallback, per spec).</item>
/// <item>Decision (<c>flowdecision</c>/<c>dsfdecision</c>) nodes must have at least one outgoing
/// edge flagged <c>isDecisionArm</c>+<c>isTrue</c> and at least one flagged
/// <c>isDecisionArm</c>+not-<c>isTrue</c> — the exact flags
/// <c>X6ToWorkflowConverter.HandleDecisionConnection</c> reads back (not edge <c>label</c> text,
/// which that method ignores for Decision). Switch (<c>dsfflowswitchactivity</c>/<c>flowswitch</c>)
/// nodes must have at least one outgoing edge with a resolvable case key
/// (<c>data.caseKey</c>, falling back to <c>label</c>, exactly as
/// <c>X6ToWorkflowConverter.HandleSwitchConnection</c> resolves it).</item>
/// <item>Every <c>[[...]]</c>-bracketed reference found anywhere inside a node's <c>data</c>
/// must resolve to a declared <c>envelope.inputs</c>/<c>outputs</c> entry (undeclared reference
/// = error); a recordset field reference (<c>[[Recordset().Field]]</c>) must further resolve to
/// a <c>fields</c> entry on that recordset's envelope entry (unresolved field = error); a
/// declared input never referenced anywhere in <c>body</c> = warning, not an error (doesn't
/// flip <c>valid</c> to <c>false</c>).</item>
/// <item>Finally — only when every check above found zero hard errors, so a graph already known
/// to be broken isn't also run through the (potentially slower, less specific) real compiler —
/// attempts the actual <see cref="X6ToWorkflowConverter.X6JsonToWorkflow"/> compile, catching any
/// exception (e.g. <see cref="UnsupportedActivityTypeException"/> for the "Calculate" gap
/// documented on <see cref="ToolCatalog"/>, or <see cref="EmptyWorkflowGraphException"/>) and
/// reporting it as a structured error rather than letting it propagate to the MCP caller.</item>
/// </list>
/// </para>
///
/// <para>
/// <b><c>errors[].severity</c> (minor, additive extension of the spec's <c>{ message, path? }</c>
/// shape).</b> The spec text distinguishes "unused input → warning" from "undeclared reference →
/// error" but only defines one output array, <c>errors</c>. Both severities are reported in that
/// one array with an explicit <c>severity</c> field (<c>"error"</c> default, <c>"warning"</c> for
/// the unused-input case) so a caller can filter without a second array; <c>valid</c> reflects
/// only <c>"error"</c>-severity entries.
/// </para>
///
/// <para>
/// Needs no permission check — per spec's authorization table, <c>validate_workflow</c> names no
/// workflow in its input, so it is "always allowed."
/// </para>
/// </summary>
internal static class ValidateWorkflowTool
{
    internal const string ToolName = "validate_workflow";

    static readonly Regex VariableReferencePattern = new(@"\[\[([^\[\]]+)\]\]", RegexOptions.Compiled);

    internal static ValidateWorkflowResult Handle(
        [Description("The workflow envelope — { name?, description?, inputs: EnvelopeVariable[], outputs: EnvelopeVariable[] } per get_workflow_schema's envelope_schema.")]
        JsonElement envelope,
        [Description("The workflow body — the X6 graph { resourcename, cells[] } per get_workflow_schema's body_schema.")]
        JsonElement body)
    {
        if (envelope.ValueKind is JsonValueKind.Undefined)
        {
            throw new McpException("`envelope` is required.");
        }
        if (body.ValueKind is JsonValueKind.Undefined)
        {
            throw new McpException("`body` is required.");
        }

        var errors = new List<ValidationIssue>();

        var envelopeVariables = ParseEnvelopeVariables(envelope);

        X6WorkflowSaveModel? graph;
        string bodyJson;
        try
        {
            bodyJson = body.GetRawText();
            var settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                FloatParseHandling = FloatParseHandling.Decimal,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            graph = JsonConvert.DeserializeObject<X6WorkflowSaveModel>(bodyJson, settings);
        }
        catch (Exception ex)
        {
            errors.Add(ValidationIssue.Error($"`body` is not a valid X6 graph: {ex.Message}"));
            return new ValidateWorkflowResult(false, errors);
        }

        if (graph?.Cells is null)
        {
            errors.Add(ValidationIssue.Error("`body` has no `cells` array."));
            return new ValidateWorkflowResult(false, errors);
        }

        var nodes = new List<(int Index, Cell Cell)>();
        var edges = new List<Cell>();
        for (var i = 0; i < graph.Cells.Count; i++)
        {
            var cell = graph.Cells[i];
            if (string.Equals(cell.shape, "edge", StringComparison.OrdinalIgnoreCase))
            {
                edges.Add(cell);
            }
            else
            {
                nodes.Add((i, cell));
            }
        }

        var hasStartNode = nodes.Any(n => GetDataType(n.Cell) is { } t &&
            string.Equals(t, Constants.START, StringComparison.OrdinalIgnoreCase));
        if (!hasStartNode)
        {
            errors.Add(ValidationIssue.Error(
                "The workflow graph has no reachable start node (no cell with data.type == \"start\"); a Flowchart could not be finalised.",
                "/cells"));
        }

        foreach (var (index, cell) in nodes)
        {
            var dataType = GetDataType(cell);
            if (string.IsNullOrWhiteSpace(dataType) ||
                string.Equals(dataType, Constants.START, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var entry = ToolCatalog.Resolve(dataType);
            if (entry is null)
            {
                errors.Add(ValidationIssue.Error(
                    $"cell '{cell.id}' has an unrecognised activity type '{dataType}'; call list_tools for the supported set.",
                    $"/cells/{index}"));
                continue;
            }

            if (IsDecisionEntry(entry))
            {
                ValidateDecisionBranches(cell, edges, index, errors);
            }
            else if (IsSwitchEntry(entry))
            {
                ValidateSwitchCases(cell, edges, index, errors);
            }
        }

        ValidateVariableReferences(nodes, envelopeVariables, errors);

        var hasHardError = errors.Any(e => e.Severity == "error");
        if (!hasHardError)
        {
            try
            {
                _ = new X6ToWorkflowConverter().X6JsonToWorkflow(bodyJson);
            }
            catch (Exception ex)
            {
                errors.Add(ValidationIssue.Error($"the graph failed to compile: {ex.Message}"));
            }
        }

        var valid = errors.All(e => e.Severity != "error");
        return new ValidateWorkflowResult(valid, errors);
    }

    // ── data.type / start-node helpers ────────────────────────────────────────

    static string? GetDataType(Cell cell) =>
        cell.data != null && cell.data.TryGetValue(Constants.TYPE, out var typeObj) && typeObj is string type
            ? type
            : null;

    static bool IsDecisionEntry(ToolCatalog.Entry entry) =>
        string.Equals(entry.Name, "Decision", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(entry.Name, "Decision (legacy)", StringComparison.OrdinalIgnoreCase);

    static bool IsSwitchEntry(ToolCatalog.Entry entry) =>
        string.Equals(entry.Name, "Switch", StringComparison.OrdinalIgnoreCase);

    // ── Decision / Switch branch checks ───────────────────────────────────────

    /// <summary>
    /// Mirrors <c>X6ToWorkflowConverter.HandleDecisionConnection</c> exactly: a Decision edge is
    /// only recognised via <c>data.isDecisionArm == true</c> + <c>data.isTrue</c>, never via edge
    /// <c>label</c> text (the converter ignores label entirely for Decision edges).
    /// </summary>
    static void ValidateDecisionBranches(Cell node, List<Cell> edges, int index, List<ValidationIssue> errors)
    {
        var outgoing = edges.Where(e => e.Source?.Id == node.id).ToList();

        var hasTrue = outgoing.Any(e => IsDecisionArm(e, true));
        var hasFalse = outgoing.Any(e => IsDecisionArm(e, false));

        if (!hasTrue)
        {
            errors.Add(ValidationIssue.Error(
                $"Decision node '{node.id}' has no outgoing edge for its True branch (an edge with data.isDecisionArm=true, data.isTrue=true).",
                $"/cells/{index}"));
        }
        if (!hasFalse)
        {
            errors.Add(ValidationIssue.Error(
                $"Decision node '{node.id}' has no outgoing edge for its False branch (an edge with data.isDecisionArm=true, data.isTrue=false).",
                $"/cells/{index}"));
        }
    }

    static bool IsDecisionArm(Cell edge, bool wantTrue)
    {
        if (edge.data is null)
        {
            return false;
        }

        if (!CommonHelper.TryGetBool(edge.data, Constants.ISDECISIONARM, out var isDecisionArm) || !isDecisionArm)
        {
            return false;
        }

        CommonHelper.TryGetBool(edge.data, Constants.ISTRUEARM, out var isTrue);
        return isTrue == wantTrue;
    }

    /// <summary>
    /// Mirrors <c>X6ToWorkflowConverter.HandleSwitchConnection</c>: a Switch case key comes from
    /// <c>data.caseKey</c>, falling back to edge <c>label</c> when <c>caseKey</c> is absent.
    /// </summary>
    static void ValidateSwitchCases(Cell node, List<Cell> edges, int index, List<ValidationIssue> errors)
    {
        var outgoing = edges.Where(e => e.Source?.Id == node.id).ToList();

        var hasCase = outgoing.Any(e =>
            (e.data != null && e.data.TryGetValue("caseKey", out var caseKeyObj) && caseKeyObj is string s && !string.IsNullOrEmpty(s)) ||
            !string.IsNullOrEmpty(e.label));

        if (!hasCase)
        {
            errors.Add(ValidationIssue.Error(
                $"Switch node '{node.id}' has no labeled case edges (an edge with data.caseKey or a non-empty label).",
                $"/cells/{index}"));
        }
    }

    // ── envelope.inputs/outputs ⇄ body [[...]] reference cross-check ──────────

    static List<EnvelopeVariable> ParseEnvelopeVariables(JsonElement envelope)
    {
        var result = new List<EnvelopeVariable>();
        foreach (var arrayName in new[] { "inputs", "outputs" })
        {
            if (!envelope.TryGetProperty(arrayName, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var item in array.EnumerateArray())
            {
                var name = item.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                var kind = item.TryGetProperty("kind", out var kindEl) ? kindEl.GetString() : "scalar";
                var fields = new List<string>();
                if (item.TryGetProperty("fields", out var fieldsEl) && fieldsEl.ValueKind == JsonValueKind.Array)
                {
                    fields.AddRange(fieldsEl.EnumerateArray()
                        .Where(f => f.ValueKind == JsonValueKind.String)
                        .Select(f => f.GetString()!));
                }

                result.Add(new EnvelopeVariable(name!, kind ?? "scalar", fields));
            }
        }

        return result;
    }

    static void ValidateVariableReferences(
        List<(int Index, Cell Cell)> nodes,
        List<EnvelopeVariable> envelopeVariables,
        List<ValidationIssue> errors)
    {
        var byName = envelopeVariables
            .GroupBy(v => v.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var referencedNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (index, cell) in nodes)
        {
            foreach (var raw in ExtractVariableTokens(cell.data))
            {
                var (baseName, fieldName) = SplitReference(raw);
                if (string.IsNullOrWhiteSpace(baseName))
                {
                    continue;
                }

                referencedNames.Add(baseName);

                if (!byName.TryGetValue(baseName, out var declared))
                {
                    errors.Add(ValidationIssue.Error(
                        $"cell '{cell.id}' references undeclared variable '[[{raw}]]' — '{baseName}' is not declared in envelope.inputs/outputs.",
                        $"/cells/{index}"));
                    continue;
                }

                if (fieldName is null)
                {
                    continue;
                }

                if (!string.Equals(declared.Kind, "recordset", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(ValidationIssue.Error(
                        $"cell '{cell.id}' references field '{fieldName}' on '{baseName}', but '{baseName}' is declared as '{declared.Kind}', not 'recordset'.",
                        $"/cells/{index}"));
                }
                else if (!declared.Fields.Contains(fieldName, StringComparer.Ordinal))
                {
                    errors.Add(ValidationIssue.Error(
                        $"cell '{cell.id}' references field '{fieldName}' on recordset '{baseName}', which has no such field declared (declared fields: {string.Join(", ", declared.Fields)}).",
                        $"/cells/{index}"));
                }
            }
        }

        for (var i = 0; i < envelopeVariables.Count; i++)
        {
            var variable = envelopeVariables[i];
            if (!referencedNames.Contains(variable.Name))
            {
                errors.Add(ValidationIssue.Warning(
                    $"declared variable '{variable.Name}' is never referenced anywhere in body."));
            }
        }
    }

    /// <summary>
    /// Recursively walks every <c>[[...]]</c>-bracketed token out of a node's <c>data</c>
    /// dictionary. Top-level dictionary values deserialize to raw CLR types (string/bool/etc.)
    /// or <see cref="JObject"/>/<see cref="JArray"/> for nested structures (per
    /// <c>JsonConvert.DeserializeObject&lt;X6WorkflowSaveModel&gt;</c>'s handling of
    /// <c>Dictionary&lt;string, object&gt;</c> values); nested values inside those containers are
    /// <see cref="JToken"/>s. Both shapes are walked so a reference is found whether it sits in a
    /// plain string field (e.g. Assign's <c>fields</c>, which some activities store as a raw
    /// JSON-encoded string) or inside a real nested array/object (e.g. Assign's <c>fields</c> as
    /// authored per <c>body_schema</c>'s own example, a <c>JArray</c> of field objects).
    /// </summary>
    static IEnumerable<string> ExtractVariableTokens(Dictionary<string, object>? data)
    {
        if (data is null)
        {
            yield break;
        }

        foreach (var value in data.Values)
        {
            foreach (var s in ExtractStrings(value))
            {
                foreach (Match match in VariableReferencePattern.Matches(s))
                {
                    yield return match.Groups[1].Value;
                }
            }
        }
    }

    static IEnumerable<string> ExtractStrings(object? value)
    {
        switch (value)
        {
            case null:
                yield break;
            case string s:
                yield return s;
                yield break;
            case JObject jObject:
                foreach (var property in jObject.Properties())
                {
                    foreach (var s in ExtractStrings(property.Value))
                    {
                        yield return s;
                    }
                }
                yield break;
            case JArray jArray:
                foreach (var item in jArray)
                {
                    foreach (var s in ExtractStrings(item))
                    {
                        yield return s;
                    }
                }
                yield break;
            case JValue jValue when jValue.Type == JTokenType.String:
                yield return jValue.Value<string>() ?? string.Empty;
                yield break;
        }
    }

    /// <summary>
    /// Splits a raw <c>[[...]]</c> token body (e.g. <c>"Name"</c> or <c>"Recordset().Field"</c>)
    /// into its base variable name and, for a recordset field reference, the field name — using
    /// the same <see cref="DataListUtil"/> recordset-notation helpers the rest of the codebase
    /// uses (they tolerate the value with or without its own <c>[[ ]]</c> wrapper).
    /// </summary>
    static (string BaseName, string? FieldName) SplitReference(string raw)
    {
        if (DataListUtil.IsValueRecordset(raw))
        {
            var recordsetName = DataListUtil.ExtractRecordsetNameFromValue(raw);
            var fieldName = DataListUtil.ExtractFieldNameFromValue(raw);
            return (recordsetName, string.IsNullOrEmpty(fieldName) ? null : fieldName);
        }

        return (DataListUtil.StripBracketsFromValue(raw).Trim(), null);
    }
}

/// <summary>One <c>envelope.inputs</c>/<c>outputs</c> entry, per <c>envelope_schema</c>.</summary>
internal sealed record EnvelopeVariable(string Name, string Kind, List<string> Fields);

/// <summary>One <c>validate_workflow</c> finding — spec's <c>{ message, path? }</c> plus an
/// additive <c>severity</c> (see <see cref="ValidateWorkflowTool"/> remarks).</summary>
internal sealed record ValidationIssue(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("path")] string? Path,
    [property: JsonPropertyName("severity")] string Severity)
{
    internal static ValidationIssue Error(string message, string? path = null) => new(message, path, "error");
    internal static ValidationIssue Warning(string message, string? path = null) => new(message, path, "warning");
}

/// <summary>The full <c>validate_workflow</c> response payload.</summary>
internal sealed record ValidateWorkflowResult(
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("errors")] IReadOnlyList<ValidationIssue> Errors);
