/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.IO;
using System.Linq;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Resolves an MCP tool's <c>name</c> input (an extension-free, forward-slash relative path —
/// exactly the shape <see cref="ToolHandlers.ListWorkflowsTool"/>'s <c>path</c> output uses) to
/// an absolute file path on disk, so <c>get_workflow_definition</c> resolves the same name
/// <c>list_workflows</c> produced, the same way it was found.
///
/// <para>
/// Mirrors <see cref="WorkflowFunctionHelper"/>.<c>ResolveFilePath</c>'s two-tier strategy
/// (compile-time <see cref="WorkflowIndex"/> lookup, then a case-insensitive on-disk fallback)
/// without reusing that method directly — it is <c>private</c>, tightly coupled to
/// <see cref="Models.WorkflowExecutionRequest"/>, and additionally handles callers that already
/// supply a full file path/extension, which no valid MCP <c>name</c> ever does.
/// </para>
/// </summary>
internal static class WorkflowNameResolver
{
    /// <summary>
    /// Returns the absolute path of the workflow file matching <paramref name="name"/> under
    /// <paramref name="workflowsDirectory"/>, or <c>null</c> when it cannot be found.
    /// </summary>
    internal static string? Resolve(string workflowsDirectory, string name)
    {
        if (string.IsNullOrWhiteSpace(workflowsDirectory) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var normalized = name.Replace('\\', '/').TrimStart('/');

        // Fast path: compile-time index, keyed exactly like list_workflows' RelativePath.
        var indexed = WorkflowIndex.Instance.Resolve(workflowsDirectory, normalized);
        if (indexed is not null && File.Exists(indexed))
        {
            return indexed;
        }

        if (!Directory.Exists(workflowsDirectory))
        {
            return null;
        }

        var relativeDir = Path.GetDirectoryName(normalized.Replace('/', Path.DirectorySeparatorChar)) ?? string.Empty;
        var searchDir = string.IsNullOrEmpty(relativeDir)
            ? workflowsDirectory
            : Path.Combine(workflowsDirectory, relativeDir);

        if (!Directory.Exists(searchDir))
        {
            return null;
        }

        var baseName = Path.GetFileName(normalized);

        // .bite is the primary format; fall back to the legacy .xml, matching
        // ListWorkflowsTool.EnumerateWorkflows' own pattern precedence.
        return FindFileCaseInsensitive(searchDir, baseName + ".bite")
            ?? FindFileCaseInsensitive(searchDir, baseName + ".xml");
    }

    // Reused per call — avoids allocating a new EnumerationOptions on every lookup.
    static readonly EnumerationOptions _caseInsensitiveOptions = new()
    {
        MatchCasing = MatchCasing.CaseInsensitive,
        RecurseSubdirectories = false,
    };

    static string? FindFileCaseInsensitive(string directory, string fileName) =>
        Directory.EnumerateFiles(directory, fileName, _caseInsensitiveOptions).FirstOrDefault();
}
