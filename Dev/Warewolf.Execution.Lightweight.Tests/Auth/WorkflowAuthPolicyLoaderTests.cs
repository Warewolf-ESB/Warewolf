/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.IO;
using System.Linq;
using Dev2.Services.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // mutates SecureConfigLoader env var + singleton state
public class WorkflowAuthPolicyLoaderTests
{
    private const string ConfigPathEnvVar = "WAREWOLF_SECURE_CONFIG";
    private const string BypassEnvVar     = "BYPASS_SECURE_CONFIG";
    private const string SuperAdminEnvVar = "WAREWOLF_SUPER_ADMIN_ENABLED";

    private string? _originalEnv;
    private string? _originalBypass;
    private string? _originalSuperAdmin;
    private string? _tempPath;

    [TestInitialize]
    public void Init()
    {
        _originalEnv        = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
        _originalBypass     = Environment.GetEnvironmentVariable(BypassEnvVar);
        _originalSuperAdmin = Environment.GetEnvironmentVariable(SuperAdminEnvVar);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(ConfigPathEnvVar,  _originalEnv);
        Environment.SetEnvironmentVariable(BypassEnvVar,      _originalBypass);
        Environment.SetEnvironmentVariable(SuperAdminEnvVar,  _originalSuperAdmin);

        if (_tempPath is not null && File.Exists(_tempPath))
            try { File.Delete(_tempPath); } catch { /* best effort */ }

        SecureConfigLoader.Reload();
    }

    // ── Setup helpers ─────────────────────────────────────────────────────────

    private WorkflowAuthPolicyLoader BuildLoader(SecuritySettingsTO settings)
    {
        _tempPath = SecureConfigBuilder.WriteTempConfig(settings);
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempPath);
        SecureConfigLoader.Reload();
        return new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
    }

    private WorkflowAuthPolicyLoader BuildLoaderNoConfig()
    {
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, "nonexistent_path_does_not_exist.config");
        SecureConfigLoader.Reload();
        return new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
    }

    // ── Adjusted original tests ───────────────────────────────────────────────

    [TestMethod]
    public void TST08_PolicyCount_MatchesNumberOfDistinctWorkflows_WithExecute()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf1"));
        settings.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings);

        Assert.AreEqual(1, loader.PolicyCount);
        var lookup = loader.GetPolicy("Wf1");
        Assert.IsTrue(lookup.HasPolicyScope);
        Assert.IsNotNull(lookup.Value);
        Assert.IsTrue(lookup.Value!.AllowedGroups.Contains("TeamA"));
    }

    [TestMethod]
    public void TST08_ViewOnlyResourceEntry_ResourcePolicyBuilt()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("ReadOnly", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf2"));
        // View-only resource entry builds a resource policy so that the resource
        // scope (not the global fallback) is used for this workflow.
        var loader = BuildLoader(settings);

        Assert.AreEqual(1, loader.PolicyCount);

        var lookup = loader.GetPolicy("Wf2");
        Assert.IsTrue(lookup.HasPolicyScope);
        Assert.IsNotNull(lookup.Value);
        Assert.IsTrue(lookup.Value!.AllowedGroups.Contains("ReadOnly"));
    }

    [TestMethod]
    public void TST08_UpnStyleEntry_IsSurfacedAsAllowedGroup()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("alice@contoso.com", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "WfUpn"));
        settings.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings);
        var lookup = loader.GetPolicy("WfUpn");

        Assert.IsTrue(lookup.HasPolicyScope);
        Assert.IsNotNull(lookup.Value);
        CollectionAssert.Contains(lookup.Value!.AllowedGroups.ToList(), "alice@contoso.com");
    }

    [TestMethod]
    public void TST08_GlobalEntry_UsedForWorkflowWithNoResourceEntries()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            SecureConfigBuilder.ServerPerm("Operators", View: true));
        settings.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings);
        var lookup = loader.GetPolicy("AnyUnconfiguredWorkflow");

        Assert.IsTrue(lookup.HasPolicyScope);
        Assert.IsNotNull(lookup.Value);
        CollectionAssert.Contains(lookup.Value!.AllowedGroups.ToList(), "Operators");
    }

    [TestMethod]
    public void TST08_Reload_AtomicallySwapsPolicySet()
    {
        var initial = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf1"));
        initial.WindowsGroupPermissions.Last().Execute = true;
        var loader = BuildLoader(initial);
        Assert.AreEqual(1, loader.PolicyCount);

        var replacement = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf1"),
            new PermSpec("TeamB", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf2"));
        replacement.WindowsGroupPermissions[1].Execute = true;
        replacement.WindowsGroupPermissions[2].Execute = true;
        File.WriteAllText(_tempPath!, SecureConfigBuilder.Encrypt(replacement));

        SecureConfigLoader.Reload();
        loader.Reload();

        Assert.AreEqual(2, loader.PolicyCount);
        Assert.IsTrue(loader.GetPolicy("Wf1").HasPolicyScope);
        Assert.IsNotNull(loader.GetPolicy("Wf1").Value);
        Assert.IsTrue(loader.GetPolicy("Wf2").HasPolicyScope);
        Assert.IsNotNull(loader.GetPolicy("Wf2").Value);
    }

    // ── New tests — config-missing / bypass ───────────────────────────────────

    [TestMethod]
    public void TST_ConfigMissing_ReturnsConfigMissing_WhenBypassNotSet()
    {
        Environment.SetEnvironmentVariable(BypassEnvVar, null);
        var loader = BuildLoaderNoConfig();

        Assert.IsFalse(loader.IsConfigEffective);
        var lookup = loader.GetPolicy("AnyWorkflow");
        Assert.IsTrue(lookup.IsConfigMissing);
        Assert.IsFalse(lookup.IsBypass);
    }

    [TestMethod]
    public void TST_ConfigMissing_ReturnsBypass_WhenBypassTrue()
    {
        Environment.SetEnvironmentVariable(BypassEnvVar, "true");
        var loader = BuildLoaderNoConfig();

        Assert.IsFalse(loader.IsConfigEffective);
        var lookup = loader.GetPolicy("AnyWorkflow");
        Assert.IsTrue(lookup.IsBypass);
        Assert.IsFalse(lookup.IsConfigMissing);
    }

    [TestMethod]
    public void TST_BlankConfig_TreatedSameAs_ConfigMissing()
    {
        // Blank config = file present but zero permission entries
        var settings = new SecuritySettingsTO { SecretKey = SecureConfigBuilder.NewSecretKey() };
        // No permissions added → Permissions.Count == 0
        Environment.SetEnvironmentVariable(BypassEnvVar, null);
        var loader = BuildLoader(settings);

        Assert.IsFalse(loader.IsConfigEffective);
        var lookup = loader.GetPolicy("AnyWorkflow");
        Assert.IsTrue(lookup.IsConfigMissing);
    }

    // ── New tests — permission resolution ────────────────────────────────────

    [TestMethod]
    public void TST_GlobalOnly_SameRole_MultipleEntries_Unioned()
    {
        // Two global entries for Developers — union should give View|Execute|Contribute
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("Developers", View: true),
            SecureConfigBuilder.ServerPerm("Developers", View: false));

        settings.WindowsGroupPermissions[0].Execute   = true;  // first entry: View+Execute
        settings.WindowsGroupPermissions[1].Contribute = true; // second entry: Contribute

        var loader = BuildLoader(settings);
        Assert.IsTrue(loader.IsConfigEffective);

        // No resource entries — global scope used
        var perms = loader.GetEffectivePermissions("AnyWorkflow", new[] { "Developers" });
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Contribute));
    }

    [TestMethod]
    public void TST_ResourceScope_DiscardsGlobal_WhenResourceEntriesExist()
    {
        // Global: Developers has View|Execute|Contribute
        // Resource (Hello World): Public has Execute, DevOps has View
        // Principal: Developers — NOT in resource scope → should get only Public Execute
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("Developers", View: true),   // [0] global
            new PermSpec("Public", IsServer: false, View: false,
                ResourceId: Guid.NewGuid(), ResourceName: "Hello World"),  // [1] resource
            new PermSpec("DevOps", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Hello World")); // [2] resource

        settings.WindowsGroupPermissions[0].Execute   = true;  // Developers global: View+Execute
        settings.WindowsGroupPermissions[0].Contribute = true; // Developers global: +Contribute
        settings.WindowsGroupPermissions[1].Execute   = true;  // Public resource: Execute

        var loader = BuildLoader(settings);

        // Resource scope for Hello World: Public(Execute) + DevOps(View)
        // Principal is Developers — not in resource scope → only Public auto-applies
        var perms = loader.GetEffectivePermissions("Hello World", new[] { "Developers" });
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));     // from Public
        Assert.IsFalse(perms.HasFlag(WorkflowPermission.View));       // DevOps not matched
        Assert.IsFalse(perms.HasFlag(WorkflowPermission.Contribute)); // global discarded
    }

    [TestMethod]
    public void TST_Public_AutoApplies_InResourceScope()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("Public", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Hello World"));
        settings.WindowsGroupPermissions.Last().Execute = true; // Public: View+Execute

        var loader = BuildLoader(settings);

        // Anonymous caller (no roles) → Public auto-applies
        var perms = loader.GetEffectivePermissions("Hello World", Array.Empty<string>());
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));
    }

    [TestMethod]
    public void TST_MultiRole_Union_InGlobalScope()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("DevOps",     View: false),
            SecureConfigBuilder.ServerPerm("Developers", View: true));

        settings.WindowsGroupPermissions[0].DeployTo   = true;
        settings.WindowsGroupPermissions[0].DeployFrom = true;
        settings.WindowsGroupPermissions[1].Execute    = true;
        settings.WindowsGroupPermissions[1].Contribute = true;

        var loader = BuildLoader(settings);

        var perms = loader.GetEffectivePermissions("NewWorld", new[] { "DevOps", "Developers" });
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Contribute));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.DeployTo));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.DeployFrom));
    }

    [TestMethod]
    public void TST_ResourceScope_DevOpsAndPublic_PrincipalGetsUnion()
    {
        // Hello World resource: Public→Execute, DevOps→View
        // Principal: DevOps → gets View(DevOps) | Execute(Public)
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("Public", IsServer: false, View: false,
                ResourceId: Guid.NewGuid(), ResourceName: "Hello World"),
            new PermSpec("DevOps", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Hello World"));

        settings.WindowsGroupPermissions[1].Execute = true; // Public.Execute = true

        var loader = BuildLoader(settings);
        var perms  = loader.GetEffectivePermissions("Hello World", new[] { "DevOps" });

        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute)); // Public
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));    // DevOps
    }

    [TestMethod]
    public void TST_UnconfiguredWorkflow_Returns_PolicyNull()
    {
        // Config has one resource entry for "OtherWorkflow" (TeamA) and no global
        // entries — but SecureConfigLoader always auto-injects "Warewolf Administrators"
        // as a global entry.  Therefore GetPolicy("Missing") returns a non-null global
        // policy (the auto-injected admin scope).
        //
        // The correct assertion is that TeamA — which is scoped to OtherWorkflow only —
        // has NO effective permissions on "Missing", because "Missing" has no resource
        // entries and TeamA is not in the global (admin) scope.
        var settings2 = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "OtherWorkflow"));
        settings2.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings2);

        // "Missing" has no resource entries; falls back to global scope (auto-injected admins).
        var lookup = loader.GetPolicy("Missing");
        Assert.IsTrue(lookup.HasPolicyScope, "Lookup must be in policy scope (global admin exists)");

        // TeamA is resource-scoped to OtherWorkflow only — it must not appear in the
        // global (admin) policy for "Missing".
        var perms = loader.GetEffectivePermissions("Missing", new[] { "TeamA" });
        Assert.AreEqual(WorkflowPermission.None, perms,
            "TeamA (resource-only for OtherWorkflow) must have no permissions on an unconfigured workflow");
    }

    // ── Super-admin tests ─────────────────────────────────────────────────────

    [TestMethod]
    public void TST_SuperAdmin_Bypass_WhenEnvVarEnabled()
    {
        Environment.SetEnvironmentVariable(SuperAdminEnvVar, "true");

        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("Administrators", View: true));
        settings.WindowsGroupPermissions[0].Execute       = true;
        settings.WindowsGroupPermissions[0].Administrator = true;

        var loader = BuildLoader(settings);
        var perms  = loader.GetEffectivePermissions("AnyWorkflow", new[] { "Administrators" });

        Assert.AreEqual(WorkflowPermission.All, perms);
    }

    [TestMethod]
    public void TST_SuperAdmin_NoBypass_WhenEnvVarFalse()
    {
        Environment.SetEnvironmentVariable(SuperAdminEnvVar, "false");

        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("Administrators", View: true));
        settings.WindowsGroupPermissions[0].Execute       = true;
        settings.WindowsGroupPermissions[0].Administrator = true;

        var loader = BuildLoader(settings);
        var perms  = loader.GetEffectivePermissions("AnyWorkflow", new[] { "Administrators" });

        // Not All — normal resolution applies
        Assert.AreNotEqual(WorkflowPermission.All, perms);
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Administrator));
    }

    [TestMethod]
    public void TST_SuperAdmin_OnlyTriggered_ByAdministratorFlagInGlobalMap()
    {
        Environment.SetEnvironmentVariable(SuperAdminEnvVar, "true");

        // Administrators role exists in global map but WITHOUT Administrator flag
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.ServerPerm("Administrators", View: true));
        settings.WindowsGroupPermissions[0].Execute = true;
        // Administrator flag deliberately NOT set → super-admin bypass must NOT fire

        var loader = BuildLoader(settings);
        var perms  = loader.GetEffectivePermissions("AnyWorkflow", new[] { "Administrators" });

        Assert.AreNotEqual(WorkflowPermission.All, perms);
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.View));
        Assert.IsTrue(perms.HasFlag(WorkflowPermission.Execute));
    }
}
