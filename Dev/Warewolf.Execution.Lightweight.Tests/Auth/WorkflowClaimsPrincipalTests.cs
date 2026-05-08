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
public class WorkflowClaimsPrincipalTests
{
    private static WorkflowClaimsPrincipal Build(bool isUserToken, string userName, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000abc"),
            new(ClaimTypes.Name, userName),
        };
        if (isUserToken) claims.Add(new Claim(AuthConstants.Scope, "user_impersonation"));
        foreach (var r in roles) claims.Add(new Claim(ClaimTypes.Role, r));
        return new WorkflowClaimsPrincipal(
            new ClaimsIdentity(claims, "Test", ClaimTypes.Name, ClaimTypes.Role));
    }

    [TestMethod]
    public void TST09_IsInAnyGroup_MatchesUpnAsGroupName()
    {
        var p = Build(isUserToken: true, "alice@contoso.com");

        Assert.IsTrue(p.IsInAnyGroup(new[] { "alice@contoso.com" }));
        Assert.IsTrue(p.IsInAnyGroup(new[] { "ALICE@CONTOSO.COM" }));
        Assert.IsFalse(p.IsInAnyGroup(new[] { "bob@contoso.com" }));
    }

    [TestMethod]
    public void TST09_IsAppOnlyToken_DetectedWhenScopeAbsent()
    {
        var p = Build(isUserToken: false, userName: "");
        Assert.IsTrue(p.IsAppOnlyToken);
        Assert.IsFalse(p.IsUserToken);
        StringAssert.StartsWith(p.CallerIdentity, "app:");
    }

    [TestMethod]
    public void TST09_HasPermissionFlag_AndLogic()
    {
        // Permissions are resolved from secure.config at request time via SetResolvedPermissions.
        var p = Build(isUserToken: true, "alice@x.com");
        p.SetResolvedPermissions(WorkflowPermission.View | WorkflowPermission.Execute);

        Assert.IsTrue(p.HasPermissionFlag(WorkflowPermission.View));
        Assert.IsTrue(p.HasPermissionFlag(WorkflowPermission.View | WorkflowPermission.Execute));
        Assert.IsFalse(p.HasPermissionFlag(WorkflowPermission.View | WorkflowPermission.Administrator));
    }

    [TestMethod]
    public void TST09_PermissionFlags_ProjectsClaimsToFlagsCorrectly()
    {
        var p = Build(isUserToken: true, "alice@x.com");
        p.SetResolvedPermissions(
            WorkflowPermission.View | WorkflowPermission.Execute | WorkflowPermission.Contribute);

        var flags = p.PermissionFlags;
        Assert.IsTrue(flags.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(flags.HasFlag(WorkflowPermission.Execute));
        Assert.IsTrue(flags.HasFlag(WorkflowPermission.Contribute));
        Assert.IsFalse(flags.HasFlag(WorkflowPermission.Administrator));
    }

    [TestMethod]
    public void TST09_None_Permission_AlwaysSatisfied()
    {
        var p = Build(isUserToken: true, "alice@x.com");
        Assert.IsTrue(p.HasPermissionFlag(WorkflowPermission.None));
    }

    [TestMethod]
    public void TST09_Anonymous_HasNoIdentity()
    {
        var anon = WorkflowClaimsPrincipal.Anonymous();
        Assert.IsFalse(anon.Identity?.IsAuthenticated ?? false);
        Assert.AreEqual(0, anon.Groups.Count);
        Assert.AreEqual(WorkflowPermission.None, anon.Permissions);
    }
}
