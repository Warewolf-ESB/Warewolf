/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
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
    private readonly IReadOnlyDictionary<string, WorkflowAuthPolicy> _policies;

    /// <inheritdoc/>
    public int PolicyCount => _policies.Count;

    public WorkflowAuthPolicyLoader(ILogger<WorkflowAuthPolicyLoader> logger)
    {
        _policies = BuildPolicies(SecureConfigLoader.Config);
        Dev2Logger.Info($"WorkflowAuthPolicyLoader initialised with {_policies.Count} workflow policies.", "WorkflowAuthPolicyLoader");
    }

    /// <inheritdoc/>
    public WorkflowAuthPolicy? GetPolicy(string workflowName) =>
        _policies.TryGetValue(workflowName.ToLowerInvariant(), out var policy) ? policy : null;

    // ── Private ───────────────────────────────────────────────────────────────

    private IReadOnlyDictionary<string, WorkflowAuthPolicy> BuildPolicies(SecureConfigData config)
    {
        if (!config.IsLoaded)
        {
            Dev2Logger.Warn(
                "secure.config not loaded — WorkflowAuthPolicyLoader has no policies. " +
                "All /secure/* routes will be evaluated without group-level enforcement.", "WorkflowAuthPolicyLoader");
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

            Dev2Logger.Debug(
                $"Policy built: workflow={group.Key} groups=[{string.Join(", ", policy.AllowedGroups)}] required={policy.RequiredPermissions}",
                "WorkflowAuthPolicyLoader");
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
}
