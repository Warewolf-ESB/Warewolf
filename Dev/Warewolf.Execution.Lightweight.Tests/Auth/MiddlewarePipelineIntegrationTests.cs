/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  TST-12 — Middleware pipeline integration: fake Easy Auth header → parser
 *  chain → policy matcher → Allowed.
 *
 *  This test exercises the same units that the worker pipeline composes:
 *    EasyAuthPrincipalParser  ⟶  WorkflowAuthPolicyLoader  ⟶  WorkflowPolicyMatcher
 *  ensuring the end-to-end identity-to-decision flow works without requiring
 *  the Functions worker host.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize]
public class MiddlewarePipelineIntegrationTests
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

    private static string EncodePrincipal(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

    [TestMethod]
    public async Task TST12_EasyAuthHeader_PolicyAllowed_EndToEnd()
    {
        // ── Arrange config: TeamA has View+Execute on workflow "hello"
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "hello"));
        settings.WindowsGroupPermissions[1].Execute = true;

        _tempPath = SecureConfigBuilder.WriteTempConfig(settings);
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempPath);
        SecureConfigLoader.Reload();

        var loader  = new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
        var matcher = new WorkflowPolicyMatcher(loader);

        // ── Build principal via real EasyAuthPrincipalParser
        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "val": "alice@x.com" },
            { "typ": "roles", "val": "TeamA" },
            { "typ": "roles", "val": "Permission.View" },
            { "typ": "roles", "val": "Permission.Execute" }
          ]
        }
        """;
        var req = new FakeHttpRequestData(new TestFunctionContext(), new Uri("https://x.test/secure/hello"));
        req.AddHeader(AuthConstants.ClientPrincipalHeader, EncodePrincipal(json));

        var parser    = new EasyAuthPrincipalParser(NullLogger<EasyAuthPrincipalParser>.Instance);
        var principal = await parser.TryParseAsync(req, CancellationToken.None);

        Assert.IsNotNull(principal);

        // ── Act: policy matcher decision
        var result = matcher.Evaluate("hello", principal!);

        // ── Assert
        Assert.AreEqual(PolicyMatchOutcome.Allowed, result.Outcome);
    }

    [TestMethod]
    public async Task TST12_EasyAuthHeader_NoMatchingGroup_DeniedEndToEnd()
    {
        var settings = SecureConfigBuilder.Build(
            SecureConfigBuilder.NewSecretKey(),
            SecureConfigBuilder.Admin(View: true),
            new PermSpec("TeamA", IsServer: false, View: true,
                ResourceId: Guid.NewGuid(), ResourceName: "hello"));
        settings.WindowsGroupPermissions[1].Execute = true;

        _tempPath = SecureConfigBuilder.WriteTempConfig(settings);
        Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempPath);
        SecureConfigLoader.Reload();

        var loader  = new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
        var matcher = new WorkflowPolicyMatcher(loader);

        var json = """
        {
          "auth_typ": "aad",
          "claims": [
            { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "val": "bob@x.com" },
            { "typ": "roles", "val": "TeamB" }
          ]
        }
        """;
        var req = new FakeHttpRequestData(new TestFunctionContext(), new Uri("https://x.test/secure/hello"));
        req.AddHeader(AuthConstants.ClientPrincipalHeader, EncodePrincipal(json));

        var parser    = new EasyAuthPrincipalParser(NullLogger<EasyAuthPrincipalParser>.Instance);
        var principal = await parser.TryParseAsync(req, CancellationToken.None);

        var result = matcher.Evaluate("hello", principal!);

        Assert.AreEqual(PolicyMatchOutcome.Forbidden, result.Outcome);
    }
}
