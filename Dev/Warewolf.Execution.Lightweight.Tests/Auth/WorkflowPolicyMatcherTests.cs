/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowPolicyMatcherTests
{
    private static WorkflowClaimsPrincipal Principal(string upn, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new(ClaimTypes.Name, upn),
            new(AuthConstants.Scope, "user_impersonation"),
        };
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        return new WorkflowClaimsPrincipal(new ClaimsIdentity(claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role));
    }

    private sealed class StaticLoader : IWorkflowAuthPolicyLoader
    {
        private readonly WorkflowAuthPolicy? _policy;
        public StaticLoader(WorkflowAuthPolicy? policy) { _policy = policy; }
        public WorkflowAuthPolicy? GetPolicy(string workflowName) => _policy;
        public int PolicyCount => _policy is null ? 0 : 1;
        public void Reload() { }
    }

    private static WorkflowAuthPolicy MakePolicy(string workflow, params (string group, WorkflowPermission perms)[] entries)
    {
        var groupEntries = new List<WorkflowGroupEntry>();
        foreach (var (g, p) in entries) groupEntries.Add(new WorkflowGroupEntry(g, p));
        return WorkflowAuthPolicy.Create(workflow, groupEntries, WorkflowPermission.View | WorkflowPermission.Execute);
    }

    [TestMethod]
    public void TST01_Allow_When_GroupMatches_AndPermissionsSufficient()
    {
        var policy = MakePolicy("hello", ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var matcher = new WorkflowPolicyMatcher(new StaticLoader(policy));

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public void TST02_DenyGroup_When_NotInAnyAllowedGroup()
    {
        var policy = MakePolicy("hello", ("TeamA", WorkflowPermission.View | WorkflowPermission.Execute));
        var matcher = new WorkflowPolicyMatcher(new StaticLoader(policy));

        var result = matcher.Evaluate("hello", Principal("bob@x.com", "TeamB"));

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        StringAssert.Contains(result.DenialReason ?? "", "not in any allowed group");
    }

    [TestMethod]
    public void TST03_DenyPermission_When_GroupMatches_ButMissingFlag()
    {
        var policy = MakePolicy("hello", ("TeamA", WorkflowPermission.View));
        var matcher = new WorkflowPolicyMatcher(new StaticLoader(policy));

        var result = matcher.Evaluate("hello", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
        Assert.IsNotNull(result.MatchedGroup);
        StringAssert.Contains(result.DenialReason ?? "", "holds permissions");
    }

    [TestMethod]
    public void TST04_NoPolicyFound_When_LoaderHasNoEntry()
    {
        var matcher = new WorkflowPolicyMatcher(new StaticLoader(null));

        var result = matcher.Evaluate("missing", Principal("alice@x.com", "TeamA"));

        Assert.AreEqual(PolicyMatchOutcome.NoPolicyFound, result.Outcome);
    }
}
