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
using System.Xml;
using ModelContextProtocol;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>validate_workflow</c> MCP tool: checks an <c>envelope</c> + <c>body</c> pair for
/// structural and semantic validity without saving or running it, against the X6 graph shape.
///
/// <para>
/// <b>Envelope shape.</b> <c>get_workflow_schema</c>'s own <c>envelope_schema</c> — the contract
/// this tool (and <c>create_workflow</c>/<c>edit_workflow</c>) exposes to an MCP caller —
/// documents the rich <c>{ kind, name, fields }</c> shape, because only that shape can
/// distinguish a <c>recordset</c> entry's declared field names, and a <c>scalar</c> entry from an
/// <c>object</c> one — required by the "recordset field references resolve to a <c>fields</c>
/// entry declared on that recordset's envelope entry" check below. This handler therefore parses
/// <c>envelope.inputs</c>/<c>outputs</c> as arrays of <see cref="EnvelopeVariable"/>, not strings.
/// <see cref="GetWorkflowDefinitionTool"/>'s <c>WorkflowEnvelope</c> used to flatten
/// inputs/outputs to bracket-notation strings instead (cheaper to derive from an existing
/// workflow's <c>DataList</c> XML, via <c>DataListTO</c>) — that divergence meant a
/// <c>get_workflow_definition</c> response could never be fed straight back into this tool or
/// <c>create_workflow</c>/<c>edit_workflow</c> (F6); it now emits the same rich shape.
/// </para>
///
/// <para>
/// <b>Checks performed, in order</b> (spec bullets, verbatim order):
/// <list type="number">
/// <item>Body must be valid JSON matching <c>X6WorkflowSaveModel</c> (<c>resourcename</c> +
/// <c>cells</c>) — a parse failure is a single structured error, never a thrown exception.</item>
/// <item>The graph must contain a node with <c>data.type == "start"</c> — mirrors
/// <c>Flowchart.StartNode</c>; missing start is a structured error (the same condition
/// <see cref="EmptyWorkflowGraphException"/> now guards in the converter itself).</item>
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
        var missingRequired = new List<string>();
        if (envelope.ValueKind is JsonValueKind.Undefined)
        {
            missingRequired.Add("`envelope`");
        }
        if (body.ValueKind is JsonValueKind.Undefined)
        {
            missingRequired.Add("`body`");
        }
        if (missingRequired.Count > 0)
        {
            var verb = missingRequired.Count > 1 ? "are" : "is";
            throw new McpException($"{string.Join(" and ", missingRequired)} {verb} required.");
        }

        // `envelope`/`body` bind as a raw JsonElement, so a caller that sends either as a JSON
        // *string* (or array/number) binds cleanly here and only fails much further down, where
        // ParseEnvelopeVariables calls TryGetProperty and JsonElement throws
        // InvalidOperationException. McpApiFunctions.Invoke does not catch that, so it reached the
        // caller as a bare HTTP 500 with an empty body — observed 2026-08-21 against
        // warewolfserver-mcp, where the MCP server's z.any() field schema (no `type`) let clients
        // send envelope/body as JSON strings. Reject a non-object up front instead, the same way
        // AddStepTool guards `step` and AddSourceTool guards `config`.
        var notObjects = new List<string>();
        if (envelope.ValueKind is not JsonValueKind.Object)
        {
            notObjects.Add($"`envelope` must be a JSON object, but a {envelope.ValueKind} was supplied");
        }
        if (body.ValueKind is not JsonValueKind.Object)
        {
            notObjects.Add($"`body` must be a JSON object, but a {body.ValueKind} was supplied");
        }
        if (notObjects.Count > 0)
        {
            throw new McpException(
                $"{string.Join("; ", notObjects)}. See get_workflow_schema for the expected shapes.");
        }

        var errors = new List<ValidationIssue>();

        var envelopeVariables = ParseEnvelopeVariables(envelope);
        ValidateEnvelopeNamesAreLegalXml(envelopeVariables, errors);

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

        var startNodes = nodes.Where(n => GetDataType(n.Cell) is { } t &&
            string.Equals(t, Constants.START, StringComparison.OrdinalIgnoreCase)).ToList();
        if (startNodes.Count == 0)
        {
            errors.Add(ValidationIssue.Error(
                "The workflow graph has no reachable start node (no cell with data.type == \"start\"); a Flowchart could not be finalised.",
                "/cells"));
        }
        else
        {
            // X6ToWorkflowConverter.BuildWorkflow always compiles the "start" cell to a
            // placeholder WriteLine activity and then discards it in favour of whatever the
            // start cell's outgoing edge points to (`startFlowNode.Next ?? startFlowNode`). If
            // the start cell has no outgoing edge, that placeholder — which is not an
            // IDev2Activity — becomes the Flowchart's literal StartNode, and ActivityParser.Parse
            // then crashes with an unhandled `ArgumentNullException("source")` at execution time
            // (WorkflowExecutor.RentPreparedWorkflow → ActivityParser.Parse). Catching this here
            // turns that into a clean validation error instead of a persisted, unexecutable
            // workflow — reproduced 2026-08-20 against a start-node-only body.
            foreach (var (index, startCell) in startNodes)
            {
                var hasOutgoingEdge = edges.Any(e => e.Source?.Id == startCell.id);
                if (!hasOutgoingEdge)
                {
                    // F7: shape:"edge" is how X6ToWorkflowConverter/this validator tell an edge
                    // apart from a node, but that requirement was undocumented — a caller who
                    // authored a connection as {source, target} with no shape got a generic
                    // "no outgoing connection" error that never hinted at the real, fixable cause.
                    var looksLikeUnshapedEdge = nodes.Any(n =>
                        string.IsNullOrEmpty(n.Cell.shape) &&
                        n.Cell.Source?.Id == startCell.id &&
                        n.Cell.Target is not null);

                    var message = looksLikeUnshapedEdge
                        ? $"The start node (cell '{startCell.id}') has no outgoing connection to another step, because a " +
                          "cell with a `source`/`target` pointing from it is missing the required `shape: \"edge\"` " +
                          "attribute (see get_workflow_schema's body_schema) and so was read as a node, not a connection."
                        : $"The start node (cell '{startCell.id}') has no outgoing connection to another step; " +
                          "a workflow must contain at least one activity reachable from its start node.";

                    errors.Add(ValidationIssue.Error(message, $"/cells/{index}"));
                }
            }
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

            ValidateCollectionFields(cell, entry, index, errors);

            foreach (var message in FindOutputMappingShapeErrors(cell.data, entry))
            {
                errors.Add(ValidationIssue.Error($"cell '{cell.id}' {message}", $"/cells/{index}"));
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

    /// <summary>
    /// Collection-valued <c>data</c> fields, keyed by <see cref="ToolCatalog.Entry.Name"/> — the
    /// fields whose activity converters read a JSON array via
    /// <see cref="CommonHelper.TryAsJArray"/>. Every other documented <c>data</c> field is a
    /// scalar string, so listing only these keeps the check narrow.
    ///
    /// <para>
    /// This exists because a value the converter could not read as an array was previously
    /// discarded in silence: <c>create_workflow</c> still returned <c>created: true</c>, the
    /// workflow still executed "successfully", and the activity simply produced nothing. Observed
    /// for Assign on <c>warewolfserver-mcp</c> (2026-08-21) when <c>fields</c> was supplied as the
    /// JSON-encoded *string* <c>get_tool_schema</c> documented at the time. That string form is
    /// now accepted by the converters, so this check fires only for values that are genuinely
    /// unusable — and it fails the call instead of losing data.
    /// </para>
    /// </summary>
    static readonly IReadOnlyDictionary<string, string[]> _collectionFieldsByToolName =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Assign"] = new[] { Constants.FIELDS, Constants.UPDATEDFIELDS },
            ["Assign Object"] = new[] { Constants.FIELDS, Constants.UPDATEDFIELDS },
            ["Create JSON"] = new[] { Constants.CREATEJSON_JSONMAPPINGS, Constants.CREATEJSON_UPDATEDJSONMAPPINGS },
            ["Data Merge"] = new[] { Constants.MERGECOLLECTION, Constants.UPDATEDMERGECOLLECTION },
            ["Data Split"] = new[] { Constants.RESULTSCOLLECTION },
            ["Base Conversion"] = new[] { Constants.CONVERTCOLLECTION, Constants.UPDATEDCONVERTCOLLECTION },
            ["XPath"] = new[] { Constants.XPATH_RESULTSCOLLECTION, Constants.XPATH_UPDATEDRESULTSCOLLECTION },
            ["Gather System Information"] = new[] { Constants.GATHERSYSINFO_SYSTEMINFOCOLLECTION },

            // F9: the remaining array-typed fields ToolSchemaCatalog documents as "array, ..." but
            // this check didn't yet cover — added mechanically by scanning that catalog for every
            // such field, no new mechanism.
            ["Service (sub-workflow)"] = new[] { Constants.WORKFLOW_INPUTS, Constants.WORKFLOW_OUTPUTS },
            ["Advanced Recordset"] = new[] { Constants.WEBMETHOD_OUTPUTS },
            ["GET Web Method"] = new[] { Constants.WEBMETHOD_HEADERS, Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["POST Web Method"] = new[] { Constants.WEBMETHOD_HEADERS, Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["PUT Web Method"] = new[] { Constants.WEBMETHOD_HEADERS, Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["DELETE Web Method"] = new[] { Constants.WEBMETHOD_HEADERS, Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["Web Request"] = new[] { Constants.WEBREQUEST_HEADERS },
            ["SQL Server Database"] = new[] { Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["PostgreSQL Database"] = new[] { Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["MySQL Database"] = new[] { Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["Oracle Database"] = new[] { Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["ODBC Database"] = new[] { Constants.WEBMETHOD_INPUTS, Constants.WEBMETHOD_OUTPUTS },
            ["SQL Bulk Insert"] = new[] { Constants.SQLBULKINSERT_INPUTMAPPINGS },
        };

    /// <summary>
    /// Reports an error for any collection field on <paramref name="node"/> whose value cannot be
    /// read as a JSON array. A field that is absent or null is left alone — whether it is required
    /// is the schema's business, not this check's.
    /// </summary>
    static void ValidateCollectionFields(Cell node, ToolCatalog.Entry entry, int index, List<ValidationIssue> errors)
    {
        if (node.data is null || !_collectionFieldsByToolName.TryGetValue(entry.Name, out var keys))
        {
            return;
        }

        foreach (var key in keys)
        {
            if (!node.data.TryGetValue(key, out var raw) || raw is null)
            {
                continue;
            }

            if (!CommonHelper.TryAsJArray(raw, out _))
            {
                errors.Add(ValidationIssue.Error(
                    $"cell '{node.id}' field '{key}' must be a JSON array, or a string containing one; "
                    + $"{DescribeUnusableValue(raw)} cannot be read as either and would be discarded silently.",
                    $"/cells/{index}/data/{key}"));
            }
        }
    }

    /// <summary>
    /// The collection-field names (from <see cref="_collectionFieldsByToolName"/>) that carry
    /// <c>ServiceOutputMapping</c>-shaped elements, i.e. every field read via
    /// <c>CommonHelper.TryGetOutputs</c> — <c>WEBMETHOD_OUTPUTS</c> (GET/POST/PUT/DELETE Web
    /// Method, Advanced Recordset, every SQL/ODBC database activity) and <c>WORKFLOW_OUTPUTS</c>
    /// (Service (sub-workflow)). Other collection fields in that table (Assign's <c>fields</c>,
    /// Data Merge's <c>mergeCollection</c>, etc.) have their own unrelated per-item shapes.
    /// </summary>
    static readonly HashSet<string> OutputsCollectionFieldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        Constants.WEBMETHOD_OUTPUTS,
        Constants.WORKFLOW_OUTPUTS,
    };

    /// <summary>The only keys <c>CommonHelper.TryGetOutputs</c> (`Dev2.Activities/WorkflowConverters/CommonHelper.cs`) actually reads off each output-mapping element.</summary>
    static readonly string[] OutputMappingKeys = { "MappedFrom", "MappedTo", "RecordSetName", "Path" };

    /// <summary>
    /// Flags output-mapping elements that match none of <see cref="OutputMappingKeys"/>.
    /// <c>CommonHelper.TryGetOutputs</c> reads each element's <c>MappedFrom</c>/<c>MappedTo</c>/
    /// <c>RecordSetName</c>/<c>Path</c> keys via <c>JObject.Value&lt;string&gt;(...)</c>, silently
    /// defaulting any key it doesn't find to <c>""</c> — so a caller-supplied shape like
    /// <c>{name, mapsTo}</c> (get_tool_schema never documented the real key names) previously wrote
    /// an all-empty mapping with no error at all (observed against warewolfserver-mcp:
    /// `[[ResponseBody]]` silently became <c>MappedFrom: ""</c>). <c>internal</c> so
    /// <see cref="AddStepTool"/> runs the identical check on the one node it appends, rather than
    /// duplicating it — the same sharing pattern as <see cref="ExtractVariableTokens"/>/
    /// <see cref="SplitReference"/>.
    /// </summary>
    internal static IEnumerable<string> FindOutputMappingShapeErrors(Dictionary<string, object>? data, ToolCatalog.Entry entry)
    {
        if (data is null || !_collectionFieldsByToolName.TryGetValue(entry.Name, out var keys))
        {
            yield break;
        }

        foreach (var key in keys)
        {
            if (!OutputsCollectionFieldNames.Contains(key) ||
                !data.TryGetValue(key, out var raw) || raw is null ||
                !CommonHelper.TryAsJArray(raw, out var array))
            {
                continue;
            }

            var itemIndex = -1;
            foreach (var child in array.Children<JObject>())
            {
                itemIndex++;
                if (OutputMappingKeys.Any(child.ContainsKey))
                {
                    continue;
                }

                var foundKeys = string.Join(", ", child.Properties().Select(p => p.Name));
                yield return $"field '{key}[{itemIndex}]' does not match the output-mapping shape " +
                    $"{{MappedFrom, MappedTo, RecordSetName, Path?}} get_tool_schema documents for '{entry.Name}'; " +
                    $"keys found were [{foundKeys}], none of which are recognised, so this mapping would be silently discarded.";
            }
        }
    }

    /// <summary>Renders an offending value for an error message without dumping a large payload.</summary>
    static string DescribeUnusableValue(object raw)
    {
        const int maxLength = 40;
        if (raw is string s)
        {
            var shown = s.Length <= maxLength ? s : s[..maxLength] + "…";
            return $"the string \"{shown}\"";
        }

        return $"a value of type {raw.GetType().Name}";
    }

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

    // ── envelope name legality (F9) ─────────────────────────────────────────────

    /// <summary>
    /// F9: <c>EnvelopeBiteWriter</c>'s rejection of an illegal XML element name
    /// (<c>envelope</c> declares a variable/field name <c>&lt;XElement&gt;</c> construction can't
    /// use) is purely incidental — there is no explicit name-legality check anywhere; it's just
    /// .NET's own <c>XElement</c>/<c>XName</c> construction throwing, caught and rewrapped. So
    /// <c>create_workflow</c> enforces this but <c>validate_workflow</c> never diagnosed it ahead
    /// of time, contradicting the tool description's claim that a payload passing
    /// <c>validate_workflow</c> is accepted by <c>create_workflow</c>. Mirrors the same
    /// construction check explicitly via <see cref="XmlConvert.VerifyName"/>.
    /// </summary>
    static void ValidateEnvelopeNamesAreLegalXml(List<EnvelopeVariable> envelopeVariables, List<ValidationIssue> errors)
    {
        foreach (var variable in envelopeVariables)
        {
            if (!IsLegalXmlElementName(variable.Name))
            {
                // The '@' object sigil belongs on the reference ([[@person.name]]), never on the
                // declaration — say so, rather than leaving the caller to guess which character
                // XmlConvert.VerifyName objected to.
                var hint = variable.Name.StartsWith(DataListUtil.ObjectStartMarker, StringComparison.Ordinal)
                    ? $" Declare it as '{variable.Name[DataListUtil.ObjectStartMarker.Length..]}' with kind \"object\"; the '{DataListUtil.ObjectStartMarker}' sigil is used only when referencing it from the body."
                    : string.Empty;

                errors.Add(ValidationIssue.Error(
                    $"envelope declares variable '{variable.Name}', which is not a legal XML element name and cannot be written to the workflow's DataList.{hint}"));
            }

            foreach (var field in variable.Fields)
            {
                if (!IsLegalXmlElementName(field))
                {
                    errors.Add(ValidationIssue.Error(
                        $"envelope declares field '{field}' on recordset '{variable.Name}', which is not a legal XML element name and cannot be written to the workflow's DataList."));
                }
            }
        }
    }

    static bool IsLegalXmlElementName(string name)
    {
        try
        {
            XmlConvert.VerifyName(name);
            return true;
        }
        catch (XmlException)
        {
            return false;
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

            var index = -1;
            foreach (var item in array.EnumerateArray())
            {
                index++;
                if (item.ValueKind != JsonValueKind.Object)
                {
                    // F6: mirrors EnvelopeBiteWriter.ParseArray's guard, so validate_workflow gets
                    // the same 400 behaviour as create_workflow/edit_workflow for a mismatched
                    // envelope shape (e.g. get_workflow_definition's old flat-string output fed
                    // straight back in), instead of an unhandled InvalidOperationException.
                    throw new McpException(
                        $"`envelope.{arrayName}[{index}]` must be a JSON object shaped {{kind, name, fields?}}, but a {item.ValueKind} was supplied.");
                }

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
                var (baseName, fieldName, isObjectReference) = SplitReference(raw);
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

                if (isObjectReference && !string.Equals(declared.Kind, "object", StringComparison.OrdinalIgnoreCase))
                {
                    // F4: the '@' object sigil (DataListUtil.ObjectStartMarker) used to be left on
                    // the base name, so it never matched a declared name at all — every object
                    // reference looked undeclared even when correctly declared as kind:"object".
                    errors.Add(ValidationIssue.Error(
                        $"cell '{cell.id}' references '[[{raw}]]' using the object sigil '@', but '{baseName}' is declared as kind '{declared.Kind}', not 'object'.",
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
    /// <c>internal</c> so <see cref="AddStepTool"/> shares this exact token extraction when
    /// auto-declaring a variable a new step references, rather than duplicating it (F5).
    /// </summary>
    internal static IEnumerable<string> ExtractVariableTokens(Dictionary<string, object>? data)
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
    /// Splits a raw <c>[[...]]</c> token body (e.g. <c>"Name"</c>, <c>"@Name"</c>, or
    /// <c>"Recordset().Field"</c>) into its base variable name, and, for a recordset field
    /// reference, the field name — using the same <see cref="DataListUtil"/> recordset-notation
    /// helpers the rest of the codebase uses (they tolerate the value with or without its own
    /// <c>[[ ]]</c> wrapper). Also strips a leading <see cref="DataListUtil.ObjectStartMarker"/>
    /// (<c>"@"</c>) and reports whether it was present: before F4, <c>[[@Response]]</c> kept the
    /// <c>@</c> on its base name, which then never matched a declared <c>Response</c> entry — the
    /// object-mode variable was undeclarable and unauthorable at the same time. <c>internal</c> so
    /// <see cref="AddStepTool"/> shares this exact reference-shape logic rather than duplicating
    /// it when auto-declaring a variable a new step references (F5).
    /// </summary>
    internal static (string BaseName, string? FieldName, bool IsObjectReference) SplitReference(string raw)
    {
        if (DataListUtil.IsValueRecordset(raw))
        {
            var recordsetName = DataListUtil.ExtractRecordsetNameFromValue(raw);
            var fieldName = DataListUtil.ExtractFieldNameFromValue(raw);
            var isObjectRecordset = recordsetName.StartsWith(DataListUtil.ObjectStartMarker, StringComparison.Ordinal);
            if (isObjectRecordset)
            {
                recordsetName = recordsetName[DataListUtil.ObjectStartMarker.Length..];
            }

            return (recordsetName, string.IsNullOrEmpty(fieldName) ? null : fieldName, isObjectRecordset);
        }

        var baseName = DataListUtil.StripBracketsFromValue(raw).Trim();
        var isObjectReference = baseName.StartsWith(DataListUtil.ObjectStartMarker, StringComparison.Ordinal);
        if (isObjectReference)
        {
            baseName = baseName[DataListUtil.ObjectStartMarker.Length..];

            // A JSON object is addressed into with dots ([[@person.address.city]]). Only the
            // first segment names the declared variable; the rest is a path inside the JSON that
            // the DataList resolves at run time and that the envelope cannot declare. Matching the
            // whole dotted string against envelope.inputs/outputs reported every such reference as
            // undeclared, which made kind:"object" variables unusable for anything but assignment.
            var pathStart = baseName.IndexOf('.', StringComparison.Ordinal);
            if (pathStart > 0)
            {
                baseName = baseName[..pathStart];
            }
        }

        return (baseName, null, isObjectReference);
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
