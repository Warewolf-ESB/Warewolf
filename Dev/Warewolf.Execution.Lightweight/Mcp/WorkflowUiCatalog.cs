/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ModelContextProtocol;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Persistence and validation for workflow forms: the declarative UI the web Studio renders at
/// <c>/form/{workflowId}</c> to collect a workflow's inputs and run it. One form per workflow, kept
/// next to it the way <see cref="TestCatalog"/> keeps tests:
/// <code>{WorkflowsDirectory}/{relativePath}.ui.json</code>
///
/// <para>
/// A form is data, never code: the Studio maps each <c>control</c> onto one of its own fixed
/// Angular components, so nothing an author (or a model) writes here runs in the browser. That
/// is why the control set is closed and unknown properties are rejected rather than ignored —
/// a misspelt property would otherwise be silently dropped and the author never told.
/// </para>
/// </summary>
internal static class WorkflowUiCatalog
{
    internal const string FileSuffix = ".ui.json";

    internal static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Controls per envelope input kind; the first is the default when <c>control</c> is omitted.</summary>
    internal static readonly IReadOnlyDictionary<string, string[]> ControlsByKind = new Dictionary<string, string[]>
    {
        ["scalar"] = new[] { "text", "textarea", "number", "email", "url", "password", "date", "datetime", "time", "checkbox", "select", "radio" },
        ["object"] = new[] { "json" },
        ["recordset"] = new[] { "table" },
    };

    /// <summary>Controls a <c>table</c> column can use (one recordset field each).</summary>
    internal static readonly string[] ColumnControls = { "text", "number", "date", "checkbox", "select" };

    internal static readonly string[] OutputDisplays = { "text", "json", "table" };

    internal static string FilePath(string workflowsDirectory, string workflowRelativePath) =>
        Path.Combine(workflowsDirectory, workflowRelativePath.Replace('/', Path.DirectorySeparatorChar) + FileSuffix);

    internal static bool Exists(string workflowsDirectory, string workflowRelativePath) =>
        File.Exists(FilePath(workflowsDirectory, workflowRelativePath));

    internal static void Save(string workflowsDirectory, string workflowRelativePath, string jsonContents)
    {
        var path = FilePath(workflowsDirectory, workflowRelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, jsonContents);
    }

    /// <summary>The stored form, or <c>null</c> when the workflow has none.</summary>
    internal static JsonElement? Load(string workflowsDirectory, string workflowRelativePath)
    {
        var path = FilePath(workflowsDirectory, workflowRelativePath);
        if (!File.Exists(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.Clone();
    }

    /// <summary>Deletes the workflow's form; <c>false</c> when it had none.</summary>
    internal static bool Delete(string workflowsDirectory, string workflowRelativePath)
    {
        var path = FilePath(workflowsDirectory, workflowRelativePath);
        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }

    /// <summary>
    /// The workflow's ResourceID, or <c>null</c> when its file has none. The web Studio's form link
    /// names a workflow by id, which survives a rename or a deploy to another engine.
    /// </summary>
    internal static string? ReadResourceId(string workflowFilePath)
    {
        try
        {
            return Guid.TryParse(XElement.Load(workflowFilePath).Attribute("ID")?.Value, out var id) ? id.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    internal static WorkflowFormModel Parse(JsonElement form)
    {
        if (form.ValueKind != JsonValueKind.Object)
        {
            throw new McpException("`form` must be a JSON object (see get_ui_schema).");
        }

        try
        {
            return JsonSerializer.Deserialize<WorkflowFormModel>(form.GetRawText(), SerializerOptions)
                   ?? throw new McpException("`form` must be a JSON object (see get_ui_schema).");
        }
        catch (JsonException ex)
        {
            throw new McpException($"`form` could not be parsed: {ex.Message} (see get_ui_schema).");
        }
    }

    /// <summary>
    /// Checks <paramref name="form"/> against the workflow's own envelope — every field names one of
    /// its inputs, with a control that suits that input's kind; every table column one of that
    /// recordset's input fields; every output one of its outputs — and returns it serialized for
    /// <see cref="Save"/>. All problems are reported together, so one round trip fixes them all.
    /// </summary>
    internal static string ValidateAndSerialize(
        WorkflowFormModel form, IReadOnlyList<EnvelopeVariableDto> inputs, IReadOnlyList<EnvelopeVariableDto> outputs)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(form.Title))
        {
            errors.Add("`form.title` is required");
        }

        var inputsByName = inputs.ToDictionary(i => i.Name, StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var fields = form.Fields ?? new List<FormFieldModel>();
        if (fields.Count == 0 && inputs.Count > 0)
        {
            errors.Add("`form.fields` must list at least one of the workflow's inputs");
        }

        for (var i = 0; i < fields.Count; i++)
        {
            var at = $"`form.fields[{i}]`";
            var field = fields[i];
            if (string.IsNullOrWhiteSpace(field.Input) || !inputsByName.TryGetValue(field.Input, out var input))
            {
                errors.Add($"{at}.input ('{field.Input}') is not one of the workflow's inputs: {NameList(inputs)}");
                continue;
            }

            if (!seen.Add(field.Input))
            {
                errors.Add($"{at}.input ('{field.Input}') appears in more than one field");
            }

            var allowed = ControlsByKind[input.Kind];
            var control = field.Control ?? allowed[0];
            if (!allowed.Contains(control))
            {
                errors.Add($"{at}.control ('{control}') does not suit {input.Kind} input '{input.Name}'; use one of: {string.Join(", ", allowed)}");
                continue;
            }

            if (control is "select" or "radio")
            {
                ValidateOptions(field.Options, at, errors);
            }
            else if (field.Options is not null)
            {
                errors.Add($"{at}.options only applies to a select or radio control");
            }

            if (field.Pattern is { Length: > 0 } pattern && !IsValidPattern(pattern))
            {
                errors.Add($"{at}.pattern ('{pattern}') is not a valid regular expression");
            }

            if (control == "table")
            {
                ValidateColumns(field.Columns, input, at, errors);
            }
            else if (field.Columns is not null)
            {
                errors.Add($"{at}.columns only applies to a table control");
            }
        }

        var outputNames = outputs.Select(o => o.Name).ToHashSet(StringComparer.Ordinal);
        var outputList = form.Outputs ?? new List<FormOutputModel>();
        for (var i = 0; i < outputList.Count; i++)
        {
            var at = $"`form.outputs[{i}]`";
            var output = outputList[i];
            if (string.IsNullOrWhiteSpace(output.Output) || !outputNames.Contains(output.Output))
            {
                errors.Add($"{at}.output ('{output.Output}') is not one of the workflow's outputs: {NameList(outputs)}");
            }

            if (output.Display is { } display && !OutputDisplays.Contains(display))
            {
                errors.Add($"{at}.display ('{display}') must be one of: {string.Join(", ", OutputDisplays)}");
            }
        }

        if (errors.Count > 0)
        {
            throw new McpException($"`form` failed validation: {string.Join("; ", errors)}.");
        }

        return JsonSerializer.Serialize(form, SerializerOptions);
    }

    static void ValidateOptions(List<FormOptionModel>? options, string at, List<string> errors)
    {
        if (options is null || options.Count == 0)
        {
            errors.Add($"{at}.options must list the choices for a select or radio control");
            return;
        }

        if (options.Any(o => o.Value is null))
        {
            errors.Add($"{at}.options entries each need a `value`");
        }
    }

    static void ValidateColumns(List<FormColumnModel>? columns, EnvelopeVariableDto input, string at, List<string> errors)
    {
        if (columns is null)
        {
            return; // every input field of the recordset, as text
        }

        var fieldNames = input.Fields ?? Array.Empty<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var c = 0; c < columns.Count; c++)
        {
            var column = columns[c];
            var columnAt = $"{at}.columns[{c}]";
            if (string.IsNullOrWhiteSpace(column.Field) || !fieldNames.Contains(column.Field))
            {
                errors.Add($"{columnAt}.field ('{column.Field}') is not an input field of recordset '{input.Name}': {string.Join(", ", fieldNames)}");
                continue;
            }

            if (!seen.Add(column.Field))
            {
                errors.Add($"{columnAt}.field ('{column.Field}') appears in more than one column");
            }

            var control = column.Control ?? ColumnControls[0];
            if (!ColumnControls.Contains(control))
            {
                errors.Add($"{columnAt}.control ('{control}') must be one of: {string.Join(", ", ColumnControls)}");
            }
            else if (control == "select")
            {
                ValidateOptions(column.Options, columnAt, errors);
            }
        }
    }

    static bool IsValidPattern(string pattern)
    {
        try
        {
            _ = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    static string NameList(IReadOnlyList<EnvelopeVariableDto> variables) =>
        variables.Count == 0 ? "(none)" : string.Join(", ", variables.Select(v => v.Name));
}

/// <summary>A workflow form, per <c>get_ui_schema</c>'s <c>form_schema</c>.</summary>
internal sealed record WorkflowFormModel(
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("submitLabel")] string? SubmitLabel = null,
    [property: JsonPropertyName("fields")] List<FormFieldModel>? Fields = null,
    [property: JsonPropertyName("outputs")] List<FormOutputModel>? Outputs = null);

internal sealed record FormFieldModel(
    [property: JsonPropertyName("input")] string? Input = null,
    [property: JsonPropertyName("label")] string? Label = null,
    [property: JsonPropertyName("control")] string? Control = null,
    [property: JsonPropertyName("required")] bool? Required = null,
    [property: JsonPropertyName("placeholder")] string? Placeholder = null,
    [property: JsonPropertyName("help")] string? Help = null,
    [property: JsonPropertyName("default")] JsonElement? Default = null,
    [property: JsonPropertyName("options")] List<FormOptionModel>? Options = null,
    [property: JsonPropertyName("min")] JsonElement? Min = null,
    [property: JsonPropertyName("max")] JsonElement? Max = null,
    [property: JsonPropertyName("step")] double? Step = null,
    [property: JsonPropertyName("pattern")] string? Pattern = null,
    [property: JsonPropertyName("maxLength")] int? MaxLength = null,
    [property: JsonPropertyName("rows")] int? Rows = null,
    [property: JsonPropertyName("columns")] List<FormColumnModel>? Columns = null);

internal sealed record FormOptionModel(
    [property: JsonPropertyName("value")] string? Value = null,
    [property: JsonPropertyName("label")] string? Label = null);

internal sealed record FormColumnModel(
    [property: JsonPropertyName("field")] string? Field = null,
    [property: JsonPropertyName("label")] string? Label = null,
    [property: JsonPropertyName("control")] string? Control = null,
    [property: JsonPropertyName("required")] bool? Required = null,
    [property: JsonPropertyName("options")] List<FormOptionModel>? Options = null);

internal sealed record FormOutputModel(
    [property: JsonPropertyName("output")] string? Output = null,
    [property: JsonPropertyName("label")] string? Label = null,
    [property: JsonPropertyName("display")] string? Display = null);
