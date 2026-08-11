/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Builds a <c>.bite</c> file's XML text from an MCP <c>envelope</c> + a compiled XAML
/// definition — the "envelope-wrapping step" <c>warewolf-lee-mcp-v3-spec.md</c>'s
/// <c>create_workflow</c> section describes as "new Lightweight-local code (mirrors
/// <c>Hello World.bite</c>'s structure)". Shared by <see cref="ToolHandlers.CreateWorkflowTool"/>
/// now and intended for <c>edit_workflow</c> (not yet implemented) to reuse later, since both
/// tools produce the exact same <c>&lt;Service&gt;</c> shape — only the mutable pieces
/// (<c>Service ID</c>/<c>VersionNumber</c>/<c>IsNewWorkflow</c>) differ between "first save" and
/// "subsequent save", which their respective callers control via this class's parameters rather
/// than this class guessing at that policy itself.
///
/// <para>
/// <b>Deliberately does not reuse <c>Dev2.Runtime.Services</c>'s
/// <c>Workflow.ToServiceDefinition()</c>/<c>ResourceCatalog</c></b> — per spec,
/// <c>Warewolf.Execution.Lightweight</c> doesn't reference that project, and pulling it in would
/// be a much heavier dependency than this ~80 lines of <see cref="XElement"/> construction.
/// </para>
/// </summary>
internal static class EnvelopeBiteWriter
{
    /// <summary>
    /// Builds the full <c>&lt;Service&gt;</c> XML text for a <c>.bite</c> file.
    /// </summary>
    /// <param name="serviceId">The <c>Service ID</c>/<c>VersionInfo</c> <c>ResourceId</c>/<c>VersionId</c> GUID.</param>
    /// <param name="displayName">Root <c>Name</c> attribute and <c>DisplayName</c>/<c>Category</c> element text.</param>
    /// <param name="description">Envelope description — becomes the <c>&lt;Comment&gt;</c> element text.</param>
    /// <param name="envelope">The <c>envelope</c> input, per <c>get_workflow_schema</c>'s <c>envelope_schema</c>
    /// (<c>inputs</c>/<c>outputs</c> arrays of <c>{ kind, name, fields? }</c>).</param>
    /// <param name="xamlDefinition">The compiled, namespace-fixed-up XAML text (already run through
    /// <c>X6ToWorkflowConverter.AddReplaceNameSpace</c>) — written raw; <see cref="XElement"/> escapes it automatically.</param>
    /// <param name="versionNumber">The <c>VersionInfo</c> <c>VersionNumber</c> — <c>"1"</c> for a first save.</param>
    /// <param name="timestampUtc">The <c>VersionInfo</c> <c>DateTimeStamp</c> (current UTC time).</param>
    /// <param name="user">The <c>VersionInfo</c> <c>User</c> — the calling principal's identity, or a system fallback.</param>
    internal static string BuildBiteFileContents(
        string serviceId,
        string displayName,
        string description,
        JsonElement envelope,
        string xamlDefinition,
        int versionNumber,
        DateTimeOffset timestampUtc,
        string user)
    {
        var dataList = BuildDataListElement(envelope);

        var service = new XElement("Service",
            new XAttribute("ID", serviceId),
            new XAttribute("Version", "1.0"),
            // No Studio server identity exists for this MCP-authored save — Guid.Empty mirrors
            // the same placeholder already used by other server-authored resources in this repo
            // (e.g. "Azure SharePoint Server Source.bite"'s ServerID) rather than inventing a
            // fictitious non-zero GUID.
            new XAttribute("ServerID", Guid.Empty.ToString()),
            new XAttribute("Name", displayName),
            new XAttribute("ResourceType", "WorkflowService"),
            new XAttribute("IsValid", "true"),
            new XAttribute("ServerVersion", typeof(EnvelopeBiteWriter).Assembly.GetName().Version?.ToString() ?? "0.0.0.0"),
            new XElement("DisplayName", displayName),
            new XElement("Category", displayName),
            new XElement("IsNewWorkflow", "false"),
            new XElement("AuthorRoles", string.Empty),
            new XElement("Comment", description ?? string.Empty),
            new XElement("Tags", string.Empty),
            new XElement("HelpLink", string.Empty),
            new XElement("UnitTestTargetWorkflowService", string.Empty),
            dataList,
            new XElement("Action",
                new XAttribute("Name", "InvokeWorkflow"),
                new XAttribute("Type", "Workflow"),
                new XElement("XamlDefinition", xamlDefinition)),
            new XElement("ErrorMessages"),
            new XElement("VersionInfo",
                new XAttribute("DateTimeStamp", timestampUtc.UtcDateTime.ToString("O")),
                new XAttribute("Reason", "Save"),
                new XAttribute("User", user),
                new XAttribute("VersionNumber", versionNumber.ToString()),
                new XAttribute("ResourceId", serviceId),
                new XAttribute("VersionId", serviceId)));

        return service.ToString(SaveOptions.DisableFormatting);
    }

    // ── envelope.inputs/outputs → <DataList> ──────────────────────────────────

    /// <summary>
    /// Builds the <c>&lt;DataList&gt;</c> element from <c>envelope.inputs</c>/<c>outputs</c>,
    /// merging entries that appear in both arrays (or whose recordset fields are split across
    /// both) into a single <c>ColumnIODirection="Both"</c> declaration — the same shape
    /// <see cref="Dev2.Data.DataListTO"/> reads back on the next <c>get_workflow_definition</c>
    /// call. Throws <see cref="ModelContextProtocol.McpException"/> (not a raw
    /// <see cref="XmlException"/>/<see cref="ArgumentException"/>) when a variable or field name
    /// is not a legal XML element name, since <c>validate_workflow</c> does not itself check that.
    /// </summary>
    internal static XElement BuildDataListElement(JsonElement envelope)
    {
        var scalars = new Dictionary<string, ScalarEntry>(StringComparer.Ordinal);
        var recordsets = new Dictionary<string, RecordsetEntry>(StringComparer.Ordinal);

        ParseArray(envelope, "inputs", isInput: true, isOutput: false, scalars, recordsets);
        ParseArray(envelope, "outputs", isInput: false, isOutput: true, scalars, recordsets);

        var root = new XElement("DataList");

        try
        {
            foreach (var scalar in scalars.Values)
            {
                root.Add(BuildScalarElement(scalar));
            }

            foreach (var recordset in recordsets.Values)
            {
                root.Add(BuildRecordsetElement(recordset));
            }
        }
        catch (Exception ex) when (ex is XmlException or ArgumentException)
        {
            throw new ModelContextProtocol.McpException(
                $"`envelope` declares a variable or field name that is not a valid XML element name: {ex.Message}");
        }

        return root;
    }

    static XElement BuildScalarElement(ScalarEntry scalar)
    {
        var element = new XElement(scalar.Name,
            new XAttribute("Description", string.Empty),
            new XAttribute("IsEditable", "True"),
            new XAttribute("ColumnIODirection", Direction(scalar.IsInput, scalar.IsOutput)));

        if (string.Equals(scalar.Kind, "object", StringComparison.OrdinalIgnoreCase))
        {
            // Mirrors TC065_PostJsonBody_IsObject_Returns_FullResponse.bite's committed fixture:
            // IsJson="True" plus a CDATA body is what DataListTO's IsJson branch requires to
            // still classify a HasElements-free node as scalar-shaped (an empty JSON object
            // literal, not a JSON *value*, since the field itself just needs to exist for the
            // ColumnIODirection scan — the actual runtime value is populated at execution time).
            element.SetAttributeValue("IsJson", "True");
            element.Add(new XCData("{}"));
        }

        return element;
    }

    static XElement BuildRecordsetElement(RecordsetEntry recordset)
    {
        var element = new XElement(recordset.Name);
        foreach (var field in recordset.Fields)
        {
            element.Add(new XElement(field.Key,
                new XAttribute("ColumnIODirection", Direction(field.Value.IsInput, field.Value.IsOutput))));
        }

        return element;
    }

    static string Direction(bool isInput, bool isOutput) =>
        isInput && isOutput ? "Both" : isInput ? "Input" : "Output";

    static void ParseArray(
        JsonElement envelope,
        string arrayName,
        bool isInput,
        bool isOutput,
        Dictionary<string, ScalarEntry> scalars,
        Dictionary<string, RecordsetEntry> recordsets)
    {
        if (!envelope.TryGetProperty(arrayName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var item in array.EnumerateArray())
        {
            var name = item.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var kind = item.TryGetProperty("kind", out var kindEl) ? kindEl.GetString() : "scalar";

            if (string.Equals(kind, "recordset", StringComparison.OrdinalIgnoreCase))
            {
                if (!recordsets.TryGetValue(name, out var recordset))
                {
                    recordset = new RecordsetEntry(name);
                    recordsets[name] = recordset;
                }

                if (item.TryGetProperty("fields", out var fieldsEl) && fieldsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var fieldEl in fieldsEl.EnumerateArray())
                    {
                        if (fieldEl.ValueKind != JsonValueKind.String)
                        {
                            continue;
                        }

                        var fieldName = fieldEl.GetString();
                        if (string.IsNullOrWhiteSpace(fieldName))
                        {
                            continue;
                        }

                        recordset.Fields.TryGetValue(fieldName, out var existing);
                        recordset.Fields[fieldName] = (
                            existing.IsInput || isInput,
                            existing.IsOutput || isOutput);
                    }
                }
            }
            else
            {
                if (!scalars.TryGetValue(name, out var scalar))
                {
                    scalar = new ScalarEntry(name, kind ?? "scalar");
                    scalars[name] = scalar;
                }

                scalar.IsInput |= isInput;
                scalar.IsOutput |= isOutput;
            }
        }
    }

    sealed class ScalarEntry(string name, string kind)
    {
        internal string Name { get; } = name;
        internal string Kind { get; } = kind;
        internal bool IsInput { get; set; }
        internal bool IsOutput { get; set; }
    }

    sealed class RecordsetEntry(string name)
    {
        internal string Name { get; } = name;
        internal Dictionary<string, (bool IsInput, bool IsOutput)> Fields { get; } = new(StringComparer.Ordinal);
    }
}
