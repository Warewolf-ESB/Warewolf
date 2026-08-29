/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Common;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

/// <summary>
/// Implements the <c>edit_source</c> MCP tool: re-builds an existing connection source's
/// connection string from a fresh <c>sourceType</c> + <c>config</c> pair and overwrites its
/// <c>.bite</c> file in place, provided the caller has Contribute permission and <c>name</c>
/// already exists — the save-side mirror of <see cref="AddSourceTool"/> for a source that
/// already exists, exactly as <see cref="EditWorkflowTool"/> is to <see cref="CreateWorkflowTool"/>.
///
/// <para>
/// <b>Steps, in spec order (mirrors <see cref="AddSourceTool"/> except where noted):</b>
/// <list type="number">
/// <item><c>name</c> MUST already resolve to an existing source
/// (<see cref="WorkflowNameResolver.Resolve"/>) — the inverse of <c>add_source</c>'s "must not
/// already exist" rule.</item>
/// <item>Requires <b>Contribute</b> permission, resource-if-present else global, Public OR'd —
/// the same rule <see cref="AddSourceTool"/> applies (<see cref="ListWorkflowsTool.HasPermission"/>).</item>
/// <item><c>sourceType</c>/<c>config</c> are validated and secret-resolved identically to
/// <c>add_source</c> (see <see cref="SourceCatalog"/>, <see cref="IMcpSecretResolver"/>) — an edit
/// may also change which <c>sourceType</c> the source is, rebuilding the connection string and
/// <c>&lt;Source&gt;</c> XML shape from scratch rather than merging with what's already on disk.</item>
/// <item><b>Preserves the existing file's <c>Source ID</c></b> (its current root <c>ID</c>
/// attribute) instead of minting a new one — this is a save of the <i>same</i> resource, not a
/// new one, so its identity must not change across an edit. Falls back to a fresh GUID only if
/// the existing file's <c>ID</c> is missing/blank.</item>
/// <item>Writes <c>VersionInfo</c> with <c>Reason="Save"</c>/current UTC timestamp/
/// <c>VersionNumber</c> incremented by one from whatever the existing file currently has
/// (treated as <c>0</c> — so the write becomes <c>"1"</c> — if the existing file has no
/// parseable <c>VersionNumber</c>).</item>
/// <item>Overwrites the file at its existing on-disk path in place — <c>edit_source</c> does not
/// move or rename a source; <c>name</c> only identifies which existing source to update.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Secrets never travel as literal JSON</b> and the connection string is encrypted — both
/// exactly as documented on <see cref="AddSourceTool"/>, unchanged for an edit.
/// </para>
/// </summary>
internal static class EditSourceTool
{
    internal const string ToolName = "edit_source";

    internal static async Task<EditSourceResult> Handle(
        HostEnvironmentConfig hostConfig,
        IWorkflowAuthPolicyLoader authPolicyLoader,
        IMcpSecretResolver secretResolver,
        ClaimsPrincipal? user,
        [Description("The existing source's name — a relative path (forward slashes), no extension. Must already exist; use add_source to create a new source.")]
        string name,
        [Description("The kind of source this now is. One of: SqlDatabase, MySqlDatabase, PostgreSQL, Oracle, ODBC, Redis, Email, RabbitMQ. May differ from the source's current sourceType.")]
        string sourceType,
        [Description("The source's connection fields as a flat JSON object of name/value pairs, replacing its current configuration in full — see add_source's config parameter for the " +
            "accepted field names/types/defaults per sourceType and the \"${secret-name}\" Key Vault reference syntax for password/secret-shaped fields.")]
        JsonElement config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new McpException("`name` is required.");
        }

        if (string.IsNullOrWhiteSpace(sourceType))
        {
            throw new McpException("`sourceType` is required.");
        }

        var entry = SourceCatalog.Resolve(sourceType)
            ?? throw new McpException($"`sourceType` '{sourceType}' is not supported. Supported values: {SourceCatalog.SupportedTypesList}.");

        var principal = user as WorkflowClaimsPrincipal;
        var workflowsDirectory = hostConfig.WorkflowsDirectory;
        var relativePath = name.Replace('\\', '/').TrimStart('/');

        var existingFilePath = WorkflowNameResolver.Resolve(workflowsDirectory, relativePath);
        if (existingFilePath is null)
        {
            throw new McpException($"Source '{name}' was not found; use add_source to create a new source.");
        }

        if (!ListWorkflowsTool.HasPermission(authPolicyLoader, principal, relativePath, WorkflowPermission.Contribute))
        {
            throw new McpException($"You do not have permission to edit source '{name}'.");
        }

        var (values, resolvedSecretFields) = await AddSourceTool.ResolveConfigAsync(entry, config, secretResolver, cancellationToken).ConfigureAwait(false);
        AddSourceTool.ApplyConditionalRequirements(entry, values);

        var connectionString = AddSourceTool.BuildConnectionString(entry, values);
        var encryptedConnectionString = DpapiWrapper.Encrypt(connectionString);

        var (existingResourceId, existingVersionNumber) = ReadExistingSourceMetadata(existingFilePath);
        var resourceId = string.IsNullOrWhiteSpace(existingResourceId) ? Guid.NewGuid().ToString() : existingResourceId;
        var displayName = Path.GetFileName(relativePath.TrimEnd('/'));
        var callerIdentity = principal?.CallerIdentity is { Length: > 0 } identity ? identity : "Anonymous";

        string xmlContents;
        try
        {
            xmlContents = AddSourceTool.BuildSourceXml(entry, resourceId, displayName, encryptedConnectionString, versionNumber: existingVersionNumber + 1, timestampUtc: DateTimeOffset.UtcNow, user: callerIdentity);
        }
        catch (Exception ex)
        {
            throw new McpException($"the source could not be composed into a .bite file: {ex.Message}");
        }

        File.WriteAllText(existingFilePath, xmlContents);

        // The name/path is unchanged by an edit, but refreshing the cache entry keeps this tool
        // consistent with edit_workflow's own guarantee and costs nothing extra.
        WorkflowIndex.Instance.AddOrUpdate(workflowsDirectory, relativePath, relativePath + ".bite");

        // Without these, an instance that already loaded this source once keeps serving the OLD
        // connection details indefinitely: EnsureSourceLoaded's per-ID "already registered" flag
        // short-circuits without re-reading the file, and even if it didn't, the directory index
        // itself never rebuilds on its own. Invalidate clears the stale registration/ResourceCatalog
        // entry for this specific ID; InvalidateDirectory covers the (rarer) case where the on-disk
        // Type changed. See LightweightSourceLoader.InvalidateDirectory's XML doc.
        LightweightSourceLoader.Instance.InvalidateDirectory(workflowsDirectory);
        if (Guid.TryParse(resourceId, out var resourceGuid))
        {
            LightweightSourceLoader.Instance.Invalidate(resourceGuid);
        }

        return new EditSourceResult(name, entry.SourceType, true, resolvedSecretFields);
    }

    /// <summary>
    /// Reads the existing file's root <c>ID</c> attribute (the Source ID to preserve across the
    /// save) and its current <c>VersionInfo/@VersionNumber</c> (to increment from). Tolerant of a
    /// missing/malformed file — returns <c>(null, 0)</c> rather than failing the whole edit,
    /// since both the resourceId and versionNumber callers already have safe fallbacks for a
    /// missing value.
    /// </summary>
    static (string? resourceId, int versionNumber) ReadExistingSourceMetadata(string filePath)
    {
        try
        {
            var root = XElement.Load(filePath);
            var resourceId = root.Attribute("ID")?.Value;
            var versionNumberText = root.Element("VersionInfo")?.Attribute("VersionNumber")?.Value;
            var versionNumber = int.TryParse(versionNumberText, out var parsed) ? parsed : 0;
            return (resourceId, versionNumber);
        }
        catch
        {
            // Malformed/unreadable existing file — proceed with fresh defaults rather than
            // blocking the edit entirely.
            return (null, 0);
        }
    }
}

/// <summary>The full <c>edit_source</c> response payload. Never includes the connection string or any secret value.</summary>
internal sealed record EditSourceResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("sourceType")] string SourceType,
    [property: JsonPropertyName("updated")] bool Updated,
    [property: JsonPropertyName("resolvedSecretFields")] IReadOnlyList<string> ResolvedSecretFields);
