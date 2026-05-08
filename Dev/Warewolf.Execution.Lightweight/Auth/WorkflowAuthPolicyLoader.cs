/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Auth;

/// <summary>
/// Singleton implementation of <see cref="IWorkflowAuthPolicyLoader"/>.
/// Converts <see cref="PermissionEntry"/> records from the loaded
/// <see cref="SecureConfigData"/> into <see cref="WorkflowAuthPolicy"/> instances
/// that the middleware pipeline can query.
///
/// Policies are built once at startup from <see cref="SecureConfigLoader.Config"/>
/// and cached in an immutable dictionary keyed by workflow resource name (lowercase).
///
/// When no <c>secure.config</c> is loaded (<see cref="SecureConfigData.IsLoaded"/> ==
/// <c>false</c>), the loader returns <c>null</c> for every workflow and the middleware
/// falls back to open-access mode.
/// </summary>
internal sealed class WorkflowAuthPolicyLoader : IWorkflowAuthPolicyLoader
{
    private volatile IReadOnlyDictionary<string, WorkflowAuthPolicy> _policies;
    private volatile IReadOnlyList<WorkflowGroupEntry> _globalEntries;
    private readonly ILogger<WorkflowAuthPolicyLoader> _logger;

    /// <inheritdoc/>
    public int PolicyCount => _policies.Count;

    public WorkflowAuthPolicyLoader(ILogger<WorkflowAuthPolicyLoader> logger)
    {
        _logger        = logger;
        _policies      = BuildPolicies(SecureConfigLoader.Config);
        _globalEntries = BuildGlobalEntries(SecureConfigLoader.Config);
        _logger.LogInformation(
            "WorkflowAuthPolicyLoader initialised with {Count} workflow policies and {Globals} global entries.",
            _policies.Count, _globalEntries.Count);
        ValidateForLockout(_policies);
    }

    /// <inheritdoc/>
    public WorkflowAuthPolicy? GetPolicy(string workflowName)
    {
        var key = workflowName.ToLowerInvariant();
        if (_policies.TryGetValue(key, out var policy))
            return policy;

        // (POL-10) Fall back to global/server-wide entries when no per-workflow
        // policy exists.  Returning a synthetic per-workflow policy lets the
        // matcher apply the standard group OR / permission AND logic uniformly.
        if (_globalEntries.Count == 0)
            return null;

        var executable = _globalEntries
            .Where(e => e.Permissions.HasFlag(WorkflowPermission.Execute))
            .ToList();
        if (executable.Count == 0)
            return null;

        return WorkflowAuthPolicy.Create(
            key, executable, WorkflowPermission.View | WorkflowPermission.Execute);
    }

    /// <inheritdoc/>
    public void Reload()
    {
        var rebuilt = BuildPolicies(SecureConfigLoader.Config);
        var globals = BuildGlobalEntries(SecureConfigLoader.Config);
        _policies      = rebuilt;
        _globalEntries = globals;
        _logger.LogInformation(
            "WorkflowAuthPolicyLoader reloaded — {Count} workflow policies, {Globals} global entries.",
            rebuilt.Count, globals.Count);
        ValidateForLockout(rebuilt);
    }

    /// <summary>
    /// (CFG-06) Warn when no entry has Execute=true — that configuration locks
    /// out every caller from /secure/* and /services/* routes.
    /// </summary>
    private void ValidateForLockout(IReadOnlyDictionary<string, WorkflowAuthPolicy> policies)
    {
        if (policies.Count == 0) return;
        var anyExecutable = policies.Values
            .SelectMany(p => p.GroupEntries)
            .Any(e => e.Permissions.HasFlag(WorkflowPermission.Execute));
        if (!anyExecutable)
        {
            _logger.LogWarning(
                "secure.config contains policies but NO group entry has Execute=true — " +
                "all /secure/* and /services/* requests will be denied. " +
                "Add Execute permission to at least one group to avoid lockout.");
        }
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private IReadOnlyDictionary<string, WorkflowAuthPolicy> BuildPolicies(SecureConfigData config)
    {
        if (!config.IsLoaded)
        {
            _logger.LogWarning(
                "secure.config not loaded — WorkflowAuthPolicyLoader has no policies. " +
                "All /secure/* routes will be evaluated without group-level enforcement.");
            return new Dictionary<string, WorkflowAuthPolicy>();
        }

        // Group resource-specific entries by normalised workflow name.
        // Global (IsGlobal == true) entries apply to all workflows and are not stored
        // per-workflow — the middleware checks IsLoaded to decide whether to enforce.
        var byWorkflow = config.Permissions
            .Where(p => !p.IsGlobal && !string.IsNullOrWhiteSpace(p.ResourceName))
            .GroupBy(p => p.ResourceName.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);

        var dict = new Dictionary<string, WorkflowAuthPolicy>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in byWorkflow)
        {
            var entries = group
                .Select(p => new WorkflowGroupEntry(p.GroupName, ToFlags(p)))
                .ToList();

            // The minimum required permissions for /secure/* access: View + Execute.
            // Groups that lack Execute are excluded from the allowed list.
            var executableEntries = entries.Where(e => e.Permissions.HasFlag(WorkflowPermission.Execute)).ToList();
            if (executableEntries.Count == 0)
                continue;

            var policy = WorkflowAuthPolicy.Create(
                group.Key,
                executableEntries,
                WorkflowPermission.View | WorkflowPermission.Execute);

            dict[group.Key] = policy;

            _logger.LogDebug(
                "Policy built: workflow={Workflow} groups=[{Groups}] required={Perms}",
                group.Key,
                string.Join(", ", policy.AllowedGroups),
                policy.RequiredPermissions);
        }

        return dict;
    }

    private static WorkflowPermission ToFlags(PermissionEntry p)
    {
        var flags = WorkflowPermission.None;
        if (p.View)          flags |= WorkflowPermission.View;
        if (p.Execute)       flags |= WorkflowPermission.Execute;
        if (p.Contribute)    flags |= WorkflowPermission.Contribute;
        if (p.DeployTo)      flags |= WorkflowPermission.DeployTo;
        if (p.DeployFrom)    flags |= WorkflowPermission.DeployFrom;
        if (p.Administrator) flags |= WorkflowPermission.Administrator;
        return flags;
    }

    /// <summary>(POL-10) Returns server-wide group entries that act as a fallback.</summary>
    private static IReadOnlyList<WorkflowGroupEntry> BuildGlobalEntries(SecureConfigData config)
    {
        if (!config.IsLoaded) return Array.Empty<WorkflowGroupEntry>();

        return config.Permissions
            .Where(p => p.IsGlobal && !string.IsNullOrWhiteSpace(p.GroupName))
            .Select(p => new WorkflowGroupEntry(p.GroupName, ToFlags(p)))
            .ToList();
    }
}
