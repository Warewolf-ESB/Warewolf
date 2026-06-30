/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowPolicyMatcherTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WorkflowClaimsPrincipal Principal(string upn, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new(ClaimTypes.Name, upn),
            new(AuthConstants.Scope, "user_impersonation"),
        };
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));
        return new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role));
    }

    /// <summary>
    /// Test double for IWorkflowAuthPolicyLoader.
    /// GetPolicy returns the configured PolicyLookupResult; GetEffectivePermissions
    /// unions permissions from all role-policy entries that match the caller's roles
    /// (including the "Public" entry unconditionally), mirroring loader semantics.
    /// </summary>
    private sealed class StaticLoader : IWorkflowAuthPolicyLoader
    {
        private readonly PolicyLookupResult _lookupResult;
        private readonly WorkflowPermission _superAdminOverride;
        private readonly bool _useSuperAdminOverride;

        /// <summary>Normal policy result.</summary>
        public StaticLoader(PolicyLookupResult lookupResult)
            => _lookupResult = lookupResult;

        /// <summary>Inject a fixed GetEffectivePermissions return value (super-admin tests).</summary>
        public StaticLoader(PolicyLookupResult lookupResult, WorkflowPermission effectiveOverride)
        {
            _lookupResult        = lookupResult;
            _superAdminOverride  = effectiveOverride;
            _useSuperAdminOverride = true;
        }

        public PolicyLookupResult GetPolicy(string workflowName) => _lookupResult;

        public WorkflowPermission GetEffectivePermissions(
            string workflowName, IEnumerable<string> callerRoles)
        {
            if (_useSuperAdminOverride)
                return _superAdminOverride;

            if (_lookupResult.Value is null)
                return WorkflowPermission.None;

            var roles  = callerRoles.ToList();
            var result = WorkflowPermission.None;

            foreach (var entry in _lookupResult.Value.RolePolicies)
            {
                if (entry.IsPublic ||
                    roles.Any(r => string.Equals(r, entry.GroupName, StringComparison.OrdinalIgnoreCase)))
                    result |= entry.EffectivePermissions;
            }

            return result;
        }

        public int  PolicyCount      => _lookupResult.Value is null ? 0 : 1;
        public bool IsConfigEffective => _lookupResult.HasPolicyScope;
        public void Reload() { }
    }

    private static WorkflowAuthPolicy MakePolicy(
        string workflow,
        params (string group, WorkflowPermission perms)[] entries)
    {
        var rolePolicies = entries
            .Select(e => ResolvedRolePolicy.Create(e.group, e.perms))
            .ToList();
        return WorkflowAuthPolicy.Create(
            workflow, rolePolicies, WorkflowPermission.View | WorkflowPermission.Execute);
    }

    // ── Original tests — adjusted for new signatures ──────────────────────────

    [TestMethod]
    public void TST01_Allow_When_GroupMatches_AndPermissionsSufficient()
    {
        var policy  = MakePolicy("hello", ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public void TST02_DenyGroup_When_NotInAnyAllowedGroup()
    {
        var policy  = MakePolicy("hello", ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("bob@x.com", "TeamB"));

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "no matching role");
    }

    [TestMethod]
    public void TST03_DenyPermission_When_GroupMatches_ButMissingFlag()
    {
        // TeamA has only DeployTo — no overlap with the default required
        // View|Execute set (OR semantics).  DenyPermission is expected.
        var policy  = MakePolicy("hello", ("TeamA", WorkflowPermission.DeployTo));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        Assert.IsNotNull(result.MatchedEntry);
        StringAssert.Contains(result.DenialReason ?? "", "resolved permissions");
    }

    [TestMethod]
    public void TST04_NoPolicyFound_When_BypassActive()
    {
        // Bypass = config not effective but BYPASS_SECURE_CONFIG=true
        var loader  = new StaticLoader(PolicyLookupResult.Bypass());
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("missing", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.NoPolicyFound, result.Outcome);
    }

    // ── New tests ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void TST05_ConfigMissingDeny_When_ConfigNotEffective_AndBypassNotSet()
    {
        var loader  = new StaticLoader(PolicyLookupResult.ConfigMissing());
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.ConfigMissingDeny, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "secure.config");
    }

    [TestMethod]
    public void TST06_Forbidden_When_WorkflowUnconfigured_PolicyNull()
    {
        // Config effective but workflow has no entries → Policy(null)
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(null));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("unconfigured", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "no entries in secure.config");
    }

    [TestMethod]
    public void TST07_Allow_PublicRole_AnyPrincipal_NoRoleMatch()
    {
        // Public entry grants Execute; no role match needed for Public
        var policy  = MakePolicy("hello",
            ("Public", WorkflowPermission.View | WorkflowPermission.Execute),
            ("DevOps", WorkflowPermission.View));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        // Principal has completely unrelated role — still allowed via Public
        var result = matcher.Evaluate("hello", Principal("anon@x.com", "SomeOtherRole"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public void TST08_Allow_MultiRole_Union_CoversRequiredPermissions()
    {
        // DevOps has View, Developers has Execute — union = View|Execute → allowed
        var policy  = MakePolicy("hello",
            ("DevOps",     WorkflowPermission.View),
            ("Developers", WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "DevOps", "Developers"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public void TST09_SetResolvedPermissions_Stamped_OnPrincipal()
    {
        var policy    = MakePolicy("hello", ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader    = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher   = new WorkflowPolicyMatcher(loader);
        var principal = Principal("alice@x.com", "TeamA");

        Assert.AreEqual(WorkflowPermission.None, principal.Permissions);

        matcher.Evaluate("hello", principal);

        Assert.AreEqual(WorkflowPermission.View | WorkflowPermission.Execute, principal.Permissions);
    }

    [TestMethod]
    public void TST10_DenyPermission_WhenResolvedFlagsInsufficientForRequired()
    {
        // Public gives only Contribute — no overlap with required View|Execute
        // under OR semantics, so the matcher must DenyPermission.
        var policy  = MakePolicy("hello", ("Public", WorkflowPermission.Contribute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("anon@x.com"),
            WorkflowPermission.View | WorkflowPermission.Execute);

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "resolved permissions");
    }

    [TestMethod]
    public void TST11_SuperAdmin_Allow_WhenLoaderReturnsAll()
    {
        // Simulate super-admin: loader returns All regardless of workflow entries
        var policy  = MakePolicy("hello", ("DevOps", WorkflowPermission.View));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy), WorkflowPermission.All);
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("admin@x.com", "Administrators"),
            WorkflowPermission.Administrator);

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public void TST12_UPNEntry_MatchedByUserName()
    {
        // UPN-style entry in secure.config (email address as group name)
        var policy  = MakePolicy("hello",
            ("alice@x.com", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        // alice's UPN matches the group entry directly
        var result = matcher.Evaluate("hello", Principal("alice@x.com"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    // ── Authenticated-but-roleless rejection (WOLF-8469) ──────────────────────
    // Requirement: an Entra user who is authenticated but holds NO app-role
    // (empty `roles` claim ⇒ empty Groups) must be rejected on /secure & /services
    // when no permission resolves for them from the secure.config group→permission
    // map.  The two deliberately-retained escape hatches (Public-OR and UPN-direct)
    // still grant access — see TST15 and TST12 — matching the agreed behaviour
    // ("keep both, no behaviour change").

    [TestMethod]
    public void TST13_DenyGroup_AuthenticatedCaller_NoRoles_NoPublic_NoUpn()
    {
        // Caller is authenticated (valid token) but carries zero role claims and
        // is not named directly; the workflow only grants the "TeamA" group.
        // No permission resolves ⇒ the matcher must reject (Forbidden / 500).
        var policy  = MakePolicy("hello",
            ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var principal = Principal("roleless@x.com"); // no roles passed ⇒ Groups empty
        Assert.AreEqual(0, principal.Groups.Count, "precondition: principal has no roles");

        var result = matcher.Evaluate("hello", principal);

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "no matching role");
    }

    [TestMethod]
    public void TST14_DenyGroup_AuthenticatedCaller_NoRoles_PublicViewOnly_RequiresExecute()
    {
        // Roleless caller; Public grants View only. Required View|Execute is not
        // satisfied, so even with the Public-OR escape hatch the caller is rejected.
        var policy  = MakePolicy("hello", ("Public", WorkflowPermission.View));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("roleless@x.com"),
            WorkflowPermission.View | WorkflowPermission.Execute);

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
    }

    [TestMethod]
    public void TST15_Allow_AuthenticatedCaller_NoRoles_PublicGrantsRequired()
    {
        // Retained exception: a roleless authenticated caller IS allowed when the
        // Public group (always OR'd into the active scope) grants the required
        // permissions. This documents the deliberate "keep Public-OR" decision.
        var policy  = MakePolicy("hello",
            ("Public", WorkflowPermission.View | WorkflowPermission.Execute));
        var loader  = new StaticLoader(PolicyLookupResult.FromPolicy(policy));
        var matcher = new WorkflowPolicyMatcher(loader);

        var result = matcher.Evaluate("hello", Principal("roleless@x.com"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }
}

