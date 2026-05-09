/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Gap tests for WorkflowClaimsPrincipal — covering IsInGroup,
 *  GetPermissionSummary, ToString, SetResolvedPermissions, UserId/UserName
 *  fallback claim resolution, and duplicate-role de-duplication.
 */

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class WorkflowClaimsPrincipalGapTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WorkflowClaimsPrincipal Build(
        string userId    = "oid-123",
        string userName  = "alice@x.com",
        bool   userToken = true,
        params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userName),
        };
        if (userToken) claims.Add(new Claim(AuthConstants.Scope, "user_impersonation"));
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        return new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }

    // ── IsInGroup ─────────────────────────────────────────────────────────────

    [TestMethod]
    public void IsInGroup_ExactMatch_ReturnsTrue()
    {
        var p = Build(roles: "TeamA");
        Assert.IsTrue(p.IsInGroup("TeamA"));
    }

    [TestMethod]
    public void IsInGroup_CaseInsensitiveMatch_ReturnsTrue()
    {
        var p = Build(roles: "TeamA");
        Assert.IsTrue(p.IsInGroup("teama"));
        Assert.IsTrue(p.IsInGroup("TEAMA"));
    }

    [TestMethod]
    public void IsInGroup_NonMember_ReturnsFalse()
    {
        var p = Build(roles: "TeamA");
        Assert.IsFalse(p.IsInGroup("TeamB"));
    }

    // ── GetPermissionSummary ──────────────────────────────────────────────────

    [TestMethod]
    public void GetPermissionSummary_ContainsAllKnownPermissionKeys()
    {
        var p = Build();
        p.SetResolvedPermissions(WorkflowPermission.View | WorkflowPermission.Execute);

        var summary = p.GetPermissionSummary();

        Assert.IsTrue(summary.ContainsKey("Permission.View"));
        Assert.IsTrue(summary.ContainsKey("Permission.Execute"));
        Assert.IsTrue(summary.ContainsKey("Permission.Contribute"));
        Assert.IsTrue(summary.ContainsKey("Permission.DeployTo"));
        Assert.IsTrue(summary.ContainsKey("Permission.DeployFrom"));
        Assert.IsTrue(summary.ContainsKey("Permission.Administrator"));
    }

    [TestMethod]
    public void GetPermissionSummary_ReflectsResolvedPermissions_CorrectTrueFalse()
    {
        var p = Build();
        p.SetResolvedPermissions(WorkflowPermission.View | WorkflowPermission.Execute);

        var summary = p.GetPermissionSummary();

        Assert.IsTrue(summary["Permission.View"]);
        Assert.IsTrue(summary["Permission.Execute"]);
        Assert.IsFalse(summary["Permission.Contribute"]);
        Assert.IsFalse(summary["Permission.Administrator"]);
    }

    [TestMethod]
    public void GetPermissionSummary_NonePermissions_AllFalse()
    {
        var p = Build();
        // Permissions default to None until SetResolvedPermissions is called
        var summary = p.GetPermissionSummary();

        Assert.IsTrue(summary.Values.All(v => !v));
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void ToString_ContainsUserNameAndGroups()
    {
        var p = Build(userName: "bob@x.com", roles: new[] { "TeamA", "TeamB" });
        p.SetResolvedPermissions(WorkflowPermission.View);

        var str = p.ToString();

        StringAssert.Contains(str, "bob@x.com");
        StringAssert.Contains(str, "TeamA");
        StringAssert.Contains(str, "TeamB");
    }

    [TestMethod]
    public void ToString_ContainsPermissions()
    {
        var p = Build();
        p.SetResolvedPermissions(WorkflowPermission.View | WorkflowPermission.Execute);

        var str = p.ToString();

        Assert.IsNotNull(str);
        Assert.IsTrue(str.Length > 0);
        // Format is User:{name}|Groups:{...}|Perms:{flags}
        StringAssert.Contains(str, "User:");
        StringAssert.Contains(str, "Groups:");
        StringAssert.Contains(str, "Perms:");
    }

    // ── SetResolvedPermissions ────────────────────────────────────────────────

    [TestMethod]
    public void SetResolvedPermissions_UpdatesPermissionsProperty()
    {
        var p = Build();
        Assert.AreEqual(WorkflowPermission.None, p.Permissions);

        p.SetResolvedPermissions(WorkflowPermission.View | WorkflowPermission.Contribute);

        Assert.AreEqual(WorkflowPermission.View | WorkflowPermission.Contribute, p.Permissions);
    }

    [TestMethod]
    public void SetResolvedPermissions_CanBeCalledMultipleTimes_LastValueWins()
    {
        var p = Build();
        p.SetResolvedPermissions(WorkflowPermission.View);
        p.SetResolvedPermissions(WorkflowPermission.Administrator);

        Assert.AreEqual(WorkflowPermission.Administrator, p.Permissions);
    }

    // ── UserId / UserName fallback claims ─────────────────────────────────────

    [TestMethod]
    public void UserId_FallsBackToObjectIdentifierClaim()
    {
        // When only the objectidentifier URI claim is present (not NameIdentifier)
        var claims = new List<Claim>
        {
            new(AuthConstants.ObjectIdentifier, "oid-from-uri"),
            new(AuthConstants.Scope, "user_impersonation"),
        };
        var p = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));

        Assert.AreEqual("oid-from-uri", p.UserId);
    }

    [TestMethod]
    public void UserName_FallsBackToPreferredUsernameClaim()
    {
        // When only preferred_username is present (no ClaimTypes.Name)
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "oid-abc"),
            new(AuthConstants.PreferredUsername, "charlie@contoso.com"),
            new(AuthConstants.Scope, "user_impersonation"),
        };
        var p = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));

        Assert.AreEqual("charlie@contoso.com", p.UserName);
    }

    // ── Duplicate role de-duplication ─────────────────────────────────────────

    [TestMethod]
    public void Groups_DuplicateRoles_AreDeduplicated()
    {
        var p = Build(roles: new[] { "TeamA", "TeamA", "TeamB" });
        // Should contain each group only once
        Assert.AreEqual(2, p.Groups.Count);
        Assert.IsTrue(p.Groups.Contains("TeamA"));
        Assert.IsTrue(p.Groups.Contains("TeamB"));
    }

    [TestMethod]
    public void Groups_DuplicateRolesDifferentCase_AreDeduplicated()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "oid"),
            new(ClaimTypes.Name, "user@x.com"),
            new(AuthConstants.Roles, "TeamA"),
            new(AuthConstants.Roles, "teama"), // same group, different case
        };
        var p = new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));

        Assert.AreEqual(1, p.Groups.Count);
    }

    // ── PermissionFlags alias ─────────────────────────────────────────────────

    [TestMethod]
    public void PermissionFlags_IsAliasForPermissions()
    {
        var p = Build();
        p.SetResolvedPermissions(WorkflowPermission.DeployTo | WorkflowPermission.DeployFrom);

        Assert.AreEqual(p.Permissions, p.PermissionFlags);
    }
}
