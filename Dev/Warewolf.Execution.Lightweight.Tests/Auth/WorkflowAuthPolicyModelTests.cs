/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Gap tests for WorkflowAuthPolicy, WorkflowGroupEntry and ResolvedRolePolicy
 *  models — covering HasPublicRole, AllowedGroups, GroupEntries, Create()
 *  overloads, and IsPublic case-insensitive derivation.
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowAuthPolicyModelTests
{
    // ── ResolvedRolePolicy ───────────────────────────────────────────────────

    [TestMethod]
    public void ResolvedRolePolicy_Create_SetsIsPublicTrue_WhenGroupNameIsPublic()
    {
        var policy = ResolvedRolePolicy.Create("Public", WorkflowPermission.View);
        Assert.IsTrue(policy.IsPublic);
        Assert.AreEqual("Public", policy.GroupName);
        Assert.AreEqual(WorkflowPermission.View, policy.EffectivePermissions);
    }

    [TestMethod]
    public void ResolvedRolePolicy_Create_IsPublicCaseInsensitive_Uppercase()
    {
        var policy = ResolvedRolePolicy.Create("PUBLIC", WorkflowPermission.View);
        Assert.IsTrue(policy.IsPublic);
    }

    [TestMethod]
    public void ResolvedRolePolicy_Create_IsPublicCaseInsensitive_Lowercase()
    {
        var policy = ResolvedRolePolicy.Create("public", WorkflowPermission.Execute);
        Assert.IsTrue(policy.IsPublic);
    }

    [TestMethod]
    public void ResolvedRolePolicy_Create_SetsIsPublicFalse_ForOtherGroups()
    {
        var policy = ResolvedRolePolicy.Create("TeamA", WorkflowPermission.View | WorkflowPermission.Execute);
        Assert.IsFalse(policy.IsPublic);
        Assert.AreEqual("TeamA", policy.GroupName);
    }

    // ── WorkflowAuthPolicy.HasPublicRole ─────────────────────────────────────

    [TestMethod]
    public void WorkflowAuthPolicy_HasPublicRole_TrueWhenPublicEntryPresent()
    {
        var policy = WorkflowAuthPolicy.Create(
            "test-workflow",
            new[]
            {
                ResolvedRolePolicy.Create("TeamA",  WorkflowPermission.View),
                ResolvedRolePolicy.Create("Public", WorkflowPermission.View),
            },
            WorkflowPermission.View);

        Assert.IsTrue(policy.HasPublicRole);
    }

    [TestMethod]
    public void WorkflowAuthPolicy_HasPublicRole_FalseWhenNoPublicEntry()
    {
        var policy = WorkflowAuthPolicy.Create(
            "test-workflow",
            new[]
            {
                ResolvedRolePolicy.Create("TeamA", WorkflowPermission.View),
                ResolvedRolePolicy.Create("TeamB", WorkflowPermission.Execute),
            },
            WorkflowPermission.View);

        Assert.IsFalse(policy.HasPublicRole);
    }

    // ── WorkflowAuthPolicy.AllowedGroups ─────────────────────────────────────

    [TestMethod]
    public void WorkflowAuthPolicy_AllowedGroups_ReturnsDistinctGroupNames()
    {
        var policy = WorkflowAuthPolicy.Create(
            "wf",
            new[]
            {
                ResolvedRolePolicy.Create("TeamA", WorkflowPermission.View),
                ResolvedRolePolicy.Create("TeamB", WorkflowPermission.Execute),
                ResolvedRolePolicy.Create("TeamA", WorkflowPermission.Contribute), // duplicate name
            },
            WorkflowPermission.View);

        var groups = policy.AllowedGroups;
        Assert.AreEqual(2, groups.Count);
        CollectionAssert.Contains((System.Collections.ICollection)groups, "TeamA");
        CollectionAssert.Contains((System.Collections.ICollection)groups, "TeamB");
    }

    // ── WorkflowAuthPolicy.GroupEntries ──────────────────────────────────────

    [TestMethod]
    public void WorkflowAuthPolicy_GroupEntries_ProjectsToWorkflowGroupEntry()
    {
        var roles = new[]
        {
            ResolvedRolePolicy.Create("TeamA", WorkflowPermission.View | WorkflowPermission.Execute),
            ResolvedRolePolicy.Create("Public", WorkflowPermission.View),
        };
        var policy = WorkflowAuthPolicy.Create("wf", roles, WorkflowPermission.View);

        var entries = policy.GroupEntries;

        Assert.AreEqual(2, entries.Count);
        Assert.IsTrue(entries.Any(e => e.GroupName == "TeamA"
            && e.Permissions == (WorkflowPermission.View | WorkflowPermission.Execute)));
        Assert.IsTrue(entries.Any(e => e.GroupName == "Public"
            && e.Permissions == WorkflowPermission.View));
    }

    // ── WorkflowAuthPolicy.Create(IEnumerable<WorkflowGroupEntry>) factory ───

    [TestMethod]
    public void WorkflowAuthPolicy_Create_WorkflowGroupEntry_BuildsCorrectPolicy()
    {
        var entries = new[]
        {
            new WorkflowGroupEntry("TeamA",  WorkflowPermission.View | WorkflowPermission.Execute),
            new WorkflowGroupEntry("Public", WorkflowPermission.View),
        };

        var policy = WorkflowAuthPolicy.Create("workflow-x", entries, WorkflowPermission.View);

        Assert.AreEqual("workflow-x", policy.WorkflowName);
        Assert.AreEqual(WorkflowPermission.View, policy.RequiredPermissions);
        Assert.AreEqual(2, policy.RolePolicies.Count);
        Assert.IsTrue(policy.HasPublicRole);
        Assert.IsTrue(policy.RolePolicies.Any(r => r.GroupName == "TeamA"
            && r.EffectivePermissions == (WorkflowPermission.View | WorkflowPermission.Execute)));
    }

    [TestMethod]
    public void WorkflowAuthPolicy_Create_WorkflowGroupEntry_IsPublicDerivedCorrectly()
    {
        var entries = new[] { new WorkflowGroupEntry("Public", WorkflowPermission.View) };
        var policy  = WorkflowAuthPolicy.Create("wf", entries, WorkflowPermission.View);

        Assert.IsTrue(policy.RolePolicies.Single().IsPublic);
    }
}
