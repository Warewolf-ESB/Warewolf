/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests for SecureConfigWatcher (POL-08).
 *  Covers:
 *  - StartAsync when no config file exists → logs info, returns without error
 *  - StartAsync when WAREWOLF_SECURE_CONFIG points to a real file → starts watcher
 *  - StopAsync → calls Dispose (watcher clean-up), no exception
 *  - Dispose → idempotent (calling twice never throws)
 *  - SafeReload success path → calls IWorkflowAuthPolicyLoader.Reload()
 *  - SafeReload failure path → exception from policy loader is caught and logged,
 *      not propagated
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads / writes environment variables and static SecureConfigLoader state
public class SecureConfigWatcherTests
{
    private const string ConfigEnvVar = "WAREWOLF_SECURE_CONFIG";

    private string? _savedEnvVar;
    private string? _tempConfigPath;

    [TestInitialize]
    public void SaveState()
    {
        _savedEnvVar = Environment.GetEnvironmentVariable(ConfigEnvVar);
    }

    [TestCleanup]
    public void RestoreState()
    {
        Environment.SetEnvironmentVariable(ConfigEnvVar, _savedEnvVar);

        if (_tempConfigPath is not null && File.Exists(_tempConfigPath))
        {
            try { File.Delete(_tempConfigPath); } catch { /* best effort */ }
        }

        // Reset the static loader so later tests aren't affected
        SecureConfigLoader.Reload();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SecureConfigWatcher Build(IWorkflowAuthPolicyLoader? loader = null) =>
        new(
            loader ?? new NullPolicyLoader(),
            NullLogger<SecureConfigWatcher>.Instance);

    private static string WriteTempFile(string content = "placeholder")
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Invokes the private <c>SafeReload</c> method via reflection so we can
    /// exercise the reload / exception paths without touching the file-watcher timer.
    /// </summary>
    private static void InvokeSafeReload(SecureConfigWatcher watcher)
    {
        var method = typeof(SecureConfigWatcher)
            .GetMethod("SafeReload", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("SafeReload method not found");
        method.Invoke(watcher, null);
    }

    // ── StartAsync — no config file ──────────────────────────────────────────

    [TestMethod]
    public async Task StartAsync_NoEnvVar_NoFileInBinDir_CompletesWithoutException()
    {
        // Ensure neither the env var nor a bin-dir secure.config is present.
        Environment.SetEnvironmentVariable(ConfigEnvVar, null);

        using var watcher = Build();

        // Should log an info message and return Task.CompletedTask with no error.
        await watcher.StartAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task StartAsync_EnvVarPointsToNonExistentFile_CompletesWithoutException()
    {
        Environment.SetEnvironmentVariable(ConfigEnvVar, @"C:\does-not-exist\secure.config");

        using var watcher = Build();
        await watcher.StartAsync(CancellationToken.None);
    }

    // ── StartAsync — config file present ────────────────────────────────────

    [TestMethod]
    public async Task StartAsync_EnvVarPointsToExistingFile_DoesNotThrow()
    {
        _tempConfigPath = WriteTempFile();
        Environment.SetEnvironmentVariable(ConfigEnvVar, _tempConfigPath);

        using var watcher = Build();
        await watcher.StartAsync(CancellationToken.None);
        // If we get here without exception the watcher started successfully.
    }

    // ── StopAsync ────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task StopAsync_WithoutPriorStart_DoesNotThrow()
    {
        using var watcher = Build();
        await watcher.StopAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task StopAsync_AfterStart_DoesNotThrow()
    {
        _tempConfigPath = WriteTempFile();
        Environment.SetEnvironmentVariable(ConfigEnvVar, _tempConfigPath);

        using var watcher = Build();
        await watcher.StartAsync(CancellationToken.None);
        await watcher.StopAsync(CancellationToken.None);
    }

    // ── Dispose — idempotency ────────────────────────────────────────────────

    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var watcher = Build();
        watcher.Dispose();
        watcher.Dispose(); // second call must be a no-op
    }

    [TestMethod]
    public async Task Dispose_AfterStartWithFile_DoesNotThrow()
    {
        _tempConfigPath = WriteTempFile();
        Environment.SetEnvironmentVariable(ConfigEnvVar, _tempConfigPath);

        var watcher = Build();
        await watcher.StartAsync(CancellationToken.None);
        watcher.Dispose();
        watcher.Dispose(); // idempotent
    }

    // ── SafeReload — success path ────────────────────────────────────────────

    [TestMethod]
    public void SafeReload_CallsPolicyLoaderReload()
    {
        // Arrange a valid config so SecureConfigLoader.Reload() doesn't throw.
        _tempConfigPath = WriteTempFile();
        Environment.SetEnvironmentVariable(ConfigEnvVar, _tempConfigPath);

        var trackingLoader = new TrackingPolicyLoader();
        using var watcher  = Build(trackingLoader);

        // Act
        InvokeSafeReload(watcher);

        // Assert
        Assert.IsTrue(trackingLoader.ReloadCallCount >= 1,
            "IWorkflowAuthPolicyLoader.Reload() must be called on a successful SafeReload");
    }

    // ── SafeReload — exception path ──────────────────────────────────────────

    [TestMethod]
    public void SafeReload_PolicyLoaderThrows_ExceptionIsCaughtNotPropagated()
    {
        _tempConfigPath = WriteTempFile();
        Environment.SetEnvironmentVariable(ConfigEnvVar, _tempConfigPath);

        var throwingLoader = new ThrowingPolicyLoader();
        using var watcher  = Build(throwingLoader);

        // Should NOT throw — SafeReload catches all exceptions and logs them.
        InvokeSafeReload(watcher);
    }

    // ── StartAsync — file arrives after watcher start ────────────────────────

    /// <summary>
    /// Regression test for the CI scenario where the security-config directory
    /// is created before the function host starts but the secure.config file is
    /// written later by the test runner.  Prior to the fix, the watcher would
    /// short-circuit at startup and never reload — leaving the engine running
    /// with the unloaded AllowAll fallback and causing GetSecureFilter to deny
    /// every workflow in apis.json.
    /// </summary>
    [TestMethod]
    public async Task StartAsync_DirectoryExistsButFileMissing_ReloadsWhenFileAppears()
    {
        var dir  = Path.Combine(Path.GetTempPath(), "swcfg_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "secure.config");
        _tempConfigPath = path;
        Environment.SetEnvironmentVariable(ConfigEnvVar, path);

        var trackingLoader = new TrackingPolicyLoader();
        using var watcher  = Build(trackingLoader);

        await watcher.StartAsync(CancellationToken.None);

        // File doesn't exist yet — no reload should have happened.
        Assert.AreEqual(0, trackingLoader.ReloadCallCount,
            "Reload must not fire before the file is created");

        // Create the file — Created event should fire and trigger a debounced reload.
        File.WriteAllText(path, "placeholder");

        // Poll up to 3s for the debounced reload (500ms debounce + FSW latency).
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (DateTime.UtcNow < deadline && trackingLoader.ReloadCallCount == 0)
            await Task.Delay(50);

        Assert.IsTrue(trackingLoader.ReloadCallCount >= 1,
            "Reload must fire after a late-arriving secure.config is created");

        try { Directory.Delete(dir, recursive: true); } catch { /* best effort */ }
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class NullPolicyLoader : IWorkflowAuthPolicyLoader
    {
        public int   PolicyCount         => 0;
        public bool  IsConfigEffective   => false;
        public void  Reload()            { }
        public PolicyLookupResult GetPolicy(string workflowName)
            => PolicyLookupResult.ConfigMissing();
        public WorkflowPermission GetEffectivePermissions(string workflowName, System.Collections.Generic.IEnumerable<string> callerRoles)
            => WorkflowPermission.None;
    }

    private sealed class TrackingPolicyLoader : IWorkflowAuthPolicyLoader
    {
        public int  ReloadCallCount    { get; private set; }
        public int  PolicyCount        => 0;
        public bool IsConfigEffective  => false;
        public void Reload()           => ReloadCallCount++;
        public PolicyLookupResult GetPolicy(string workflowName)
            => PolicyLookupResult.ConfigMissing();
        public WorkflowPermission GetEffectivePermissions(string workflowName, System.Collections.Generic.IEnumerable<string> callerRoles)
            => WorkflowPermission.None;
    }

    private sealed class ThrowingPolicyLoader : IWorkflowAuthPolicyLoader
    {
        public int  PolicyCount       => 0;
        public bool IsConfigEffective => false;
        public void Reload()          => throw new InvalidOperationException("Simulated reload failure");
        public PolicyLookupResult GetPolicy(string workflowName)
            => PolicyLookupResult.ConfigMissing();
        public WorkflowPermission GetEffectivePermissions(string workflowName, System.Collections.Generic.IEnumerable<string> callerRoles)
            => WorkflowPermission.None;
    }
}
