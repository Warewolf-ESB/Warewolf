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
///
/// Builds and caches two role maps from <see cref="SecureConfigLoader.Config"/> at
/// startup (and on every <see cref="Reload"/> call):
///
/// <list type="bullet">
///   <item>
///     <b>Global role map</b> — flags unioned per <c>WindowsGroup</c> across all
///     <c>IsServer=true</c> entries.  Used as the active scope when a workflow has
///     no resource-specific entries.
///   </item>
///   <item>
///     <b>Resource role map</b> — flags unioned per <c>WindowsGroup</c> within each
///     <c>ResourceName</c>.  When entries exist for the requested workflow, this scope
///     is used exclusively (global entries are discarded for that workflow).
///   </item>
/// </list>
///
/// <para>
/// <b>secure.config is mandatory.</b>  When the config is absent or blank and
/// <c>BYPASS_SECURE_CONFIG</c> is not <c>true</c>, <see cref="GetPolicy"/> returns
/// <see cref="PolicyLookupResult.ConfigMissing"/> and every request is denied with
/// 503 Service Unavailable.  Set <c>BYPASS_SECURE_CONFIG=true</c> to opt in to
/// open-access mode explicitly (development only).
/// </para>
///
/// <para>
/// <b>Super-admin bypass.</b>  When <c>WAREWOLF_SUPER_ADMIN_ENABLED=true</c> and the
/// caller has any role that holds the <see cref="WorkflowPermission.Administrator"/>
/// flag in the <i>global</i> role map, <see cref="GetEffectivePermissions"/> returns
/// <see cref="WorkflowPermission.All"/> regardless of the active scope.  The env-var
/// is read on every call to support hot-toggle without restart.
/// </para>
/// </summary>
internal sealed class WorkflowAuthPolicyLoader : IWorkflowAuthPolicyLoader
{
    // ── Environment variable names ─────────────────────────────────────────────

    private const string BypassEnvVar      = "BYPASS_SECURE_CONFIG";
    private const string SuperAdminEnvVar  = "WAREWOLF_SUPER_ADMIN_ENABLED";

    // ── State (swapped atomically on Reload) ──────────────────────────────────

    /// <summary>Pre-built resource-scope policies keyed by workflow name (lowercase).</summary>
    private volatile IReadOnlyDictionary<string, WorkflowAuthPolicy> _policies;

    /// <summary>Global (server-wide) role → unioned flags map.</summary>
    private volatile IReadOnlyDictionary<string, WorkflowPermission> _globalRoleMap;

    /// <summary>Resource-specific role → unioned flags map, keyed by workflow name (lowercase).</summary>
    private volatile IReadOnlyDictionary<string, IReadOnlyDictionary<string, WorkflowPermission>> _resourceRoleMap;

    /// <summary>
    /// <c>true</c> when <c>secure.config</c> is loaded AND has at least one permission entry.
    /// </summary>
    private volatile bool _isConfigEffective;

    private readonly ILogger<WorkflowAuthPolicyLoader> _logger;

    // ── IWorkflowAuthPolicyLoader ─────────────────────────────────────────────

    /// <inheritdoc/>
    public int PolicyCount => _policies.Count;

    /// <inheritdoc/>
    public bool IsConfigEffective => _isConfigEffective;

    // ── Constructor ───────────────────────────────────────────────────────────

    public WorkflowAuthPolicyLoader(ILogger<WorkflowAuthPolicyLoader> logger)
    {
        _logger = logger;
        (_policies, _globalRoleMap, _resourceRoleMap, _isConfigEffective) =
            Build(SecureConfigLoader.Config);

        _logger.LogInformation(
            "WorkflowAuthPolicyLoader initialised — effective={Effective} " +
            "resourcePolicies={Count} globalRoles={Globals}.",
            _isConfigEffective, _policies.Count, _globalRoleMap.Count);

        ValidateForLockout();
    }

    // ── GetPolicy ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public PolicyLookupResult GetPolicy(string workflowName)
    {
        if (!_isConfigEffective)
        {
            var bypass = string.Equals(
                Environment.GetEnvironmentVariable(BypassEnvVar),
                "true", StringComparison.OrdinalIgnoreCase);

            return bypass
                ? PolicyLookupResult.Bypass()
                : PolicyLookupResult.ConfigMissing();
        }

        var key = workflowName.Replace('\\', '/').ToLowerInvariant();

        // ── Resource scope takes priority ─────────────────────────────────────
        if (_policies.TryGetValue(key, out var resourcePolicy))
            return PolicyLookupResult.FromPolicy(resourcePolicy);

        // ── Fall back to global scope ─────────────────────────────────────────
        if (_globalRoleMap.Count > 0)
        {
            var globalEntries = _globalRoleMap
                .Select(kvp => ResolvedRolePolicy.Create(kvp.Key, kvp.Value))
                .ToList();

            var globalPolicy = WorkflowAuthPolicy.Create(
                key,
                globalEntries,
                WorkflowPermission.View | WorkflowPermission.Execute);

            return PolicyLookupResult.FromPolicy(globalPolicy);
        }

        // ── Workflow unconfigured — deny (403) ────────────────────────────────
        return PolicyLookupResult.FromPolicy(null);
    }

    // ── GetEffectivePermissions ───────────────────────────────────────────────

    /// <inheritdoc/>
    public WorkflowPermission GetEffectivePermissions(
        string workflowName, IEnumerable<string> callerRoles)
    {
        var roles = callerRoles.ToList();

        // ── Super-admin pre-check (hot-read env-var) ──────────────────────────
        var superAdminEnabled = string.Equals(
            Environment.GetEnvironmentVariable(SuperAdminEnvVar),
            "true", StringComparison.OrdinalIgnoreCase);

        if (superAdminEnabled)
        {
            foreach (var role in roles)
            {
                if (_globalRoleMap.TryGetValue(role, out var globalPerms) &&
                    globalPerms.HasFlag(WorkflowPermission.Administrator))
                {
                    _logger.LogDebug(
                        "Super-admin bypass for role '{Role}' on workflow '{Workflow}'.",
                        role, workflowName);
                    return WorkflowPermission.All;
                }
            }
        }

        // ── Determine active scope ─────────────────────────────────────────────
        var key = workflowName.Replace('\\', '/').ToLowerInvariant();
        IReadOnlyDictionary<string, WorkflowPermission> activeScope;

        if (_resourceRoleMap.TryGetValue(key, out var resourceMap))
            activeScope = resourceMap;
        else
            activeScope = _globalRoleMap;

        // ── Collect permissions ───────────────────────────────────────────────
        var combined = WorkflowPermission.None;

        // Public is always included (no role match needed)
        foreach (var kvp in activeScope)
        {
            if (string.Equals(kvp.Key, "Public", StringComparison.OrdinalIgnoreCase))
            {
                combined |= kvp.Value;
                break;
            }
        }

        // Matched roles contribute their permissions
        foreach (var role in roles)
        {
            if (activeScope.TryGetValue(role, out var rolePerms))
                combined |= rolePerms;
        }

        return combined;
    }

    // ── Reload ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void Reload()
    {
        var (policies, globalMap, resourceMap, effective) = Build(SecureConfigLoader.Config);
        _policies        = policies;
        _globalRoleMap   = globalMap;
        _resourceRoleMap = resourceMap;
        _isConfigEffective = effective;

        _logger.LogInformation(
            "WorkflowAuthPolicyLoader reloaded — effective={Effective} " +
            "resourcePolicies={Count} globalRoles={Globals}.",
            effective, policies.Count, globalMap.Count);

        ValidateForLockout();
    }

    // ── Private build helpers ─────────────────────────────────────────────────

    private (IReadOnlyDictionary<string, WorkflowAuthPolicy> policies,
             IReadOnlyDictionary<string, WorkflowPermission> globalRoleMap,
             IReadOnlyDictionary<string, IReadOnlyDictionary<string, WorkflowPermission>> resourceRoleMap,
             bool isConfigEffective)
        Build(SecureConfigData config)
    {
        var isEffective = config.IsLoaded && config.Permissions.Count > 0;

        if (!isEffective)
        {
            _logger.LogError(
                "secure.config is absent or contains no permission entries. " +
                "All requests will be denied unless BYPASS_SECURE_CONFIG=true is set.");

            return (
                new Dictionary<string, WorkflowAuthPolicy>(),
                new Dictionary<string, WorkflowPermission>(StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, IReadOnlyDictionary<string, WorkflowPermission>>(StringComparer.OrdinalIgnoreCase),
                false);
        }

        var globalMap   = BuildGlobalRoleMap(config);
        var resourceMap = BuildResourceRoleMap(config);
        var policies    = BuildPolicies(resourceMap);

        return (policies, globalMap, resourceMap, true);
    }

    /// <summary>
    /// Groups all <c>IsGlobal</c> permission entries by <c>GroupName</c> and unions
    /// the <see cref="WorkflowPermission"/> flags within each group.
    /// </summary>
    private static IReadOnlyDictionary<string, WorkflowPermission> BuildGlobalRoleMap(
        SecureConfigData config)
    {
        var map = new Dictionary<string, WorkflowPermission>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in config.Permissions)
        {
            if (!p.IsGlobal || string.IsNullOrWhiteSpace(p.GroupName))
                continue;

            var flags = ToFlags(p);
            map[p.GroupName] = map.TryGetValue(p.GroupName, out var existing)
                ? existing | flags
                : flags;
        }

        return map;
    }

    /// <summary>
    /// Groups all resource-specific (non-global) permission entries by workflow name
    /// (lowercase) then by <c>GroupName</c>, unioning flags within each role group.
    /// </summary>
    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, WorkflowPermission>>
        BuildResourceRoleMap(SecureConfigData config)
    {
        var outer = new Dictionary<string, Dictionary<string, WorkflowPermission>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var p in config.Permissions)
        {
            if (p.IsGlobal || string.IsNullOrWhiteSpace(p.ResourceName) ||
                string.IsNullOrWhiteSpace(p.GroupName))
                continue;

            var wfKey = p.ResourceName.Replace('\\', '/').ToLowerInvariant();

            if (!outer.TryGetValue(wfKey, out var inner))
            {
                inner = new Dictionary<string, WorkflowPermission>(StringComparer.OrdinalIgnoreCase);
                outer[wfKey] = inner;
            }

            var flags = ToFlags(p);
            inner[p.GroupName] = inner.TryGetValue(p.GroupName, out var existing)
                ? existing | flags
                : flags;
        }

        // Seal inner dicts as read-only
        var result = new Dictionary<string, IReadOnlyDictionary<string, WorkflowPermission>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var kvp in outer)
            result[kvp.Key] = kvp.Value;

        return result;
    }

    /// <summary>
    /// Converts the resource role map into pre-built <see cref="WorkflowAuthPolicy"/>
    /// instances keyed by workflow name.  Only workflows that have at least one
    /// entry with the Execute flag set produce a policy.
    /// </summary>
    private IReadOnlyDictionary<string, WorkflowAuthPolicy> BuildPolicies(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, WorkflowPermission>> resourceMap)
    {
        var dict = new Dictionary<string, WorkflowAuthPolicy>(StringComparer.OrdinalIgnoreCase);

        foreach (var (workflowKey, roleMap) in resourceMap)
        {
            var rolePolicies = roleMap
                .Select(kvp => ResolvedRolePolicy.Create(kvp.Key, kvp.Value))
                .ToList();

            var policy = WorkflowAuthPolicy.Create(
                workflowKey,
                rolePolicies,
                WorkflowPermission.View | WorkflowPermission.Execute);

            dict[workflowKey] = policy;

            _logger.LogDebug(
                "Resource policy built: workflow='{Workflow}' roles=[{Roles}]",
                workflowKey,
                string.Join(", ", rolePolicies.Select(e => e.GroupName)));
        }

        return dict;
    }

    // ── Validation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Warns at startup/reload when configuration is likely to lock out all callers.
    /// </summary>
    private void ValidateForLockout()
    {
        if (!_isConfigEffective) return;

        // Global scope: no Execute entry → all unconfigured workflows are denied
        var globalHasExecute = _globalRoleMap.Values
            .Any(p => p.HasFlag(WorkflowPermission.Execute));
        if (!globalHasExecute)
        {
            _logger.LogWarning(
                "secure.config global entries contain no role with Execute=true. " +
                "All workflows without resource-specific entries will deny every caller.");
        }

        // Resource scope: warn per-workflow where no Execute entry exists
        foreach (var (workflow, roleMap) in _resourceRoleMap)
        {
            var wfHasExecute = roleMap.Values.Any(p => p.HasFlag(WorkflowPermission.Execute));
            if (!wfHasExecute)
            {
                _logger.LogWarning(
                    "secure.config resource entries for workflow '{Workflow}' contain " +
                    "no role with Execute=true — all callers will be denied for this workflow.",
                    workflow);
            }
        }
    }

    // ── ToFlags helper ────────────────────────────────────────────────────────

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



