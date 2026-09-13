/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using ModelContextProtocol;
using System;
using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>deploy_workflow</c> MCP tool: writes a caller-supplied, already-complete
/// <c>.bite</c> file's XML content to disk as a workflow — the "bring your own file" counterpart
/// to <see cref="CreateWorkflowTool"/>/<see cref="EditWorkflowTool"/>, which instead accept an
/// <c>envelope</c> + <c>body</c> pair and compose the <c>.bite</c> file themselves.
///
/// <para>
/// <b>Why this exists.</b> <c>create_workflow</c>/<c>edit_workflow</c> require the caller to
/// describe a workflow via the X6-graph <c>body</c> shape, which is the natural authoring format
/// for a caller building a workflow step-by-step. Some callers instead already hold a complete,
/// exported <c>.bite</c> document (e.g. from Warewolf Studio, source control, or another
/// Warewolf instance) and simply want it deployed byte-for-byte — <c>deploy_workflow</c> serves
/// that case without forcing a round-trip through the X6 graph shape.
/// </para>
///
/// <para>
/// <b>Steps, in order:</b>
/// <list type="number">
/// <item><c>name</c> and <c>biteContent</c> are required.</item>
/// <item><c>biteContent</c> must be well-formed XML whose root is a
/// <c>&lt;Service ResourceType="WorkflowService"&gt;</c> element with a compilable
/// <c>Action/XamlDefinition</c> — the same structural shape every other tool in this namespace
/// writes (<see cref="EnvelopeBiteWriter.BuildBiteFileContents(string,string,string,System.Text.Json.JsonElement,string,int,DateTimeOffset,string)"/>)
/// and reads (<see cref="ListWorkflowsTool.TryReadWorkflowHeader"/>). Rejected without writing
/// on any structural or compile failure.</item>
/// <item><c>name</c>'s existing-workflow state gates on <paramref name="overwrite"/>: if a
/// workflow already resolves at <c>name</c> (<see cref="WorkflowNameResolver.Resolve"/>) and
/// <c>overwrite</c> is <c>false</c> (the default), the call is rejected rather than silently
/// replacing the existing file — a caller must explicitly opt in to overwrite an existing
/// deployment. If no workflow exists yet at <c>name</c>, <c>overwrite</c> is irrelevant and the
/// file is created.</item>
/// <item>Requires <b>Contribute</b> permission, resource-if-present else global, Public OR'd —
/// the same rule <see cref="CreateWorkflowTool"/>/<see cref="EditWorkflowTool"/> already apply
/// (<see cref="ListWorkflowsTool.HasPermission"/>).</item>
/// <item>Writes <paramref name="biteContent"/> to disk verbatim at <c>name</c>'s resolved path
/// (the existing file's path when overwriting, otherwise a fresh <c>{name}.bite</c> under the
/// workflows directory, creating any missing intermediate folders) — unlike
/// <c>create_workflow</c>/<c>edit_workflow</c>, this tool does not mint a Service ID, stamp
/// <c>VersionInfo</c>, or otherwise rewrite the document: the caller supplied a complete file
/// and it is deployed as-is.</item>
/// <item>Refreshes the relevant <see cref="WorkflowIndex"/> cache entry via
/// <see cref="WorkflowIndex.AddOrUpdate"/> so the deployed workflow is immediately resolvable by
/// name without waiting for a process restart — matching <c>create_workflow</c>/<c>edit_workflow</c>'s
/// own cache-freshness guarantee.</item>
/// </list>
/// </para>
/// </summary>
internal static class DeployWorkflowTool
{
    internal const string ToolName = "deploy_workflow";

    internal static DeployWorkflowResult Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        ClaimsPrincipal? user,
        [Description("The workflow's name — a relative path (forward slashes), no extension. Identifies where the .bite file is written.")]
        string name,
        [Description("The full contents of a .bite file — a <Service ResourceType=\"WorkflowService\"> XML document with a compilable Action/XamlDefinition. Written to disk verbatim.")]
        string biteContent,
        [Description("When false (the default) and a workflow already exists at `name`, the call is rejected. Set true to replace the existing workflow's .bite file in place.")]
        bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        if (string.IsNullOrWhiteSpace(biteContent))
        {
            throw new McpException("`biteContent` is required.");
        }

        var xamlDefinition = ValidateBiteContent(biteContent);

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var existingFilePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        var alreadyExists = existingFilePath is not null;

        if (alreadyExists && !overwrite)
        {
            throw new McpException($"A workflow named '{name}' already exists; pass overwrite: true to replace it.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to deploy workflow '{name}'.");
        }

        // Defensive net: validated above, but a malformed/uncompilable XamlDefinition should
        // never actually reach disk even if ValidateBiteContent's checks are ever loosened.
        _ = xamlDefinition;

        var fullPath = existingFilePath ?? Path.Combine(workflowsDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar) + ".bite");
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, biteContent);
        // Drop any pooled compilation of this workflow so the next execution picks the new
        // definition up. WorkflowExecutor's pool key already includes the file's timestamp+length,
        // so this is belt-and-braces for the one case that cannot see: a rewrite of identical
        // length landing within the filesystem's timestamp granularity - which is exactly what an
        // agent making rapid successive edits produces.
        WorkflowExecutor.EvictWorkflow(fullPath);

        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        return new DeployWorkflowResult(name, true, alreadyExists);
    }

    /// <summary>
    /// Validates that <paramref name="biteContent"/> is well-formed XML shaped like a
    /// Warewolf <c>WorkflowService</c> resource with a compilable <c>XamlDefinition</c>, and
    /// returns that XAML text. Throws <see cref="McpException"/> (never a raw
    /// <see cref="XmlException"/> or converter exception) describing exactly what is wrong,
    /// mirroring how <see cref="CreateWorkflowTool"/>/<see cref="EditWorkflowTool"/> report a
    /// failed <see cref="ValidateWorkflowTool"/> check.
    /// </summary>
    static StringBuilder ValidateBiteContent(string biteContent)
    {
        XElement root;
        try
        {
            root = XElement.Parse(biteContent);
        }
        catch (Exception ex) when (ex is XmlException or ArgumentException)
        {
            throw new McpException($"`biteContent` is not well-formed XML: {ex.Message}");
        }

        var resourceType = root.Attribute("ResourceType")?.Value;
        if (!string.Equals(resourceType, "WorkflowService", StringComparison.OrdinalIgnoreCase))
        {
            throw new McpException("`biteContent` must be a <Service ResourceType=\"WorkflowService\"> resource.");
        }

        var fileContents = new StringBuilder(biteContent);
        StringBuilder? xamlDefinition;
        try
        {
            (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
        }
        catch (Exception ex)
        {
            throw new McpException($"`biteContent` could not be parsed: {ex.Message}");
        }

        if (xamlDefinition is null || xamlDefinition.Length == 0)
        {
            throw new McpException("`biteContent` has no Action/XamlDefinition to deploy.");
        }

        System.Activities.ActivityBuilder? activityBuilder;
        try
        {
            activityBuilder = XamlActivityBuilderLoader.Load(xamlDefinition);
        }
        catch (Exception ex)
        {
            throw new McpException($"`biteContent`'s XamlDefinition could not be compiled: {ex.Message}");
        }

        if (activityBuilder is null)
        {
            throw new McpException("`biteContent`'s XamlDefinition could not be compiled into an ActivityBuilder.");
        }

        return xamlDefinition;
    }
}

/// <summary>The full <c>deploy_workflow</c> response payload.</summary>
internal sealed record DeployWorkflowResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("deployed")] bool Deployed,
    [property: JsonPropertyName("overwritten")] bool Overwritten);
