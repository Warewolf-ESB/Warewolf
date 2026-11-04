/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Checks the secure.config the Cloud pipeline ships to warewolfserver
 *  (Scripts/secure.config.cloud.json). warewolfserver accepts sign-ins from any Microsoft
 *  account (tenantId "common"), so 'Authenticated Users' means anyone: they may view
 *  workflows, but not execute or change anything, and not view the existing sources.
 *  Anonymous callers (Public) may only view and run Hello World.
 */

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // mutates SecureConfigLoader env var + singleton state
public class CloudSecureConfigTests
{
    const string ConfigPathEnvVar = "WAREWOLF_SECURE_CONFIG";
    static readonly string[] AuthenticatedUser = { "Authenticated Users", "someone@example.com" };
    static readonly string[] Administrator = { "Authenticated Users", "Warewolf Administrators" };

    static readonly string[] ExistingSources =
    {
        "Azure Exchange Source",
        "Local Server Source",
        "NewSqlServerSource",
        "Warewolf DevOps RabbitMQ Source",
    };

    string? _originalEnv;
    WorkflowAuthPolicyLoader _loader = null!;

    [TestInitialize]
    public void Init()
    {
        _originalEnv = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
        var path = Path.Combine(AppContext.BaseDirectory, "CloudSecureConfig", "secure.config.cloud.json");
        Assert.IsTrue(File.Exists(path), $"Linked secure.config.cloud.json not found at {path}");
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, path);
        SecureConfigLoader.Reload();
        Assert.IsTrue(SecureConfigLoader.Config.IsLoaded, "secure.config.cloud.json did not load");
        _loader = new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
    }

    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _originalEnv);
        SecureConfigLoader.Reload();
    }

    [TestMethod]
    public void Anonymous_CanViewAndRunHelloWorld_AndNothingElse()
    {
        var helloWorld = _loader.GetEffectivePermissions("Hello World", Array.Empty<string>());
        Assert.IsTrue(helloWorld.HasFlag(WorkflowPermission.View | WorkflowPermission.Execute));

        Assert.AreEqual(WorkflowPermission.None, _loader.GetEffectivePermissions("Simple MSSQL Test", Array.Empty<string>()));
        foreach (var source in ExistingSources)
        {
            Assert.AreEqual(WorkflowPermission.None, _loader.GetEffectivePermissions(source, Array.Empty<string>()), source);
        }
    }

    [TestMethod]
    public void AuthenticatedUser_CanViewAWorkflow_ButNotExecuteOrChangeIt()
    {
        var permissions = _loader.GetEffectivePermissions("Simple MSSQL Test", AuthenticatedUser);

        Assert.IsTrue(permissions.HasFlag(WorkflowPermission.View));
        Assert.IsFalse(permissions.HasFlag(WorkflowPermission.Execute));
        Assert.IsFalse(permissions.HasFlag(WorkflowPermission.Contribute));
    }

    [TestMethod]
    public void AshleyLewis_CanViewExecuteAndContribute_OnAnyNewWorkflow()
    {
        var ashley = new[] { "Authenticated Users", "ashley.lewis@theunlimited.co.za" };

        var permissions = _loader.GetEffectivePermissions("A Brand New Workflow", ashley);

        Assert.IsTrue(permissions.HasFlag(WorkflowPermission.View | WorkflowPermission.Execute | WorkflowPermission.Contribute));
    }

    [TestMethod]
    public void AuthenticatedUsersRow_GrantsNoExecute()
    {
        foreach (var entry in SecureConfigLoader.Config.Permissions)
        {
            if (string.Equals(entry.GroupName, "Authenticated Users", StringComparison.OrdinalIgnoreCase))
            {
                Assert.IsFalse(entry.Execute, $"Authenticated Users has Execute on '{(entry.IsGlobal ? "(server)" : entry.ResourceName)}'");
                Assert.IsFalse(entry.Contribute, $"Authenticated Users has Contribute on '{(entry.IsGlobal ? "(server)" : entry.ResourceName)}'");
            }
        }
    }

    [TestMethod]
    public void AuthenticatedUser_CannotViewTheExistingSources()
    {
        foreach (var source in ExistingSources)
        {
            Assert.AreEqual(WorkflowPermission.None, _loader.GetEffectivePermissions(source, AuthenticatedUser), source);
        }
    }

    [TestMethod]
    public void Administrators_KeepFullAccessToTheExistingSources()
    {
        foreach (var source in ExistingSources)
        {
            var permissions = _loader.GetEffectivePermissions(source, Administrator);
            Assert.IsTrue(permissions.HasFlag(WorkflowPermission.View | WorkflowPermission.Contribute), source);
        }
    }
}
