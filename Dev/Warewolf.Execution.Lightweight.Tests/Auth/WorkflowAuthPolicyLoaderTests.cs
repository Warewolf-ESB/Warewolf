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
    private string?      _originalEnv;
    private string?      _tempPath;

    [TestInitialize]
    public void Init()
    {
        _originalEnv = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _originalEnv);
        if (_tempPath is not null && File.Exists(_tempPath))
        {
            try { File.Delete(_tempPath); } catch { /* best effort */ }
        }
        SecureConfigLoader.Reload();
    }

    private WorkflowAuthPolicyLoader BuildLoader(SecuritySettingsTO settings)
    {
        _tempPath = SecureConfigBuilder.WriteTempConfig(settings);
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempPath);
        SecureConfigLoader.Reload();
        return new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
    }

    [TestMethod]
    public void TST08_PolicyCount_MatchesNumberOfDistinctWorkflows_WithExecute()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf1"));

        // Manually flip Execute=true via the underlying setting (PermSpec doesn't expose Execute).
        settings.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings);

        Assert.AreEqual(1, loader.PolicyCount);
        var policy = loader.GetPolicy("Wf1");
        Assert.IsNotNull(policy);
        Assert.IsTrue(policy!.AllowedGroups.Contains("TeamA"));
    }

    [TestMethod]
    public void TST08_EntriesWithoutExecute_AreFilteredOut()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("ReadOnly", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "Wf2"));
        // No Execute → should be excluded; resulting workflow has no executable group → no policy.
        var loader = BuildLoader(settings);

        Assert.AreEqual(0, loader.PolicyCount);
        Assert.IsNull(loader.GetPolicy("Wf2"));
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
        var policy = loader.GetPolicy("WfUpn");

        Assert.IsNotNull(policy);
        CollectionAssert.Contains(policy!.AllowedGroups.ToList(), "alice@contoso.com");
    }

    [TestMethod]
    public void TST08_GlobalEntry_FallsBackForUnknownWorkflow_POL10()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            SecureConfigBuilder.ServerPerm("Operators", View: true));
        settings.WindowsGroupPermissions.Last().Execute = true;

        var loader = BuildLoader(settings);
        var policy = loader.GetPolicy("AnyUnconfiguredWorkflow");

        Assert.IsNotNull(policy);
        CollectionAssert.Contains(policy!.AllowedGroups.ToList(), "Operators");
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

        // Replace config on disk with a different policy set then reload.
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
        Assert.IsNotNull(loader.GetPolicy("Wf1"));
        Assert.IsNotNull(loader.GetPolicy("Wf2"));
    }
}
