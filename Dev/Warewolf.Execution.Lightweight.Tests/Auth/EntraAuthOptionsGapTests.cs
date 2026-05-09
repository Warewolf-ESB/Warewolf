/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Gap tests for EntraAuthOptions — covering FromEnvironment(), IsEnabled
 *  edge cases, and ValidAudiences behaviour when both Audience and ClientId
 *  are provided.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads / writes environment variables
public class EntraAuthOptionsGapTests
{
    private const string TenantEnv   = "WAREWOLF_ENTRA_TENANT_ID";
    private const string AudienceEnv = "WAREWOLF_ENTRA_AUDIENCE";
    private const string ClientEnv   = "WAREWOLF_ENTRA_CLIENT_ID";

    private string? _origTenant, _origAudience, _origClient;

    [TestInitialize]
    public void SaveEnv()
    {
        _origTenant   = Environment.GetEnvironmentVariable(TenantEnv);
        _origAudience = Environment.GetEnvironmentVariable(AudienceEnv);
        _origClient   = Environment.GetEnvironmentVariable(ClientEnv);
    }

    [TestCleanup]
    public void RestoreEnv()
    {
        Environment.SetEnvironmentVariable(TenantEnv,   _origTenant);
        Environment.SetEnvironmentVariable(AudienceEnv, _origAudience);
        Environment.SetEnvironmentVariable(ClientEnv,   _origClient);
    }

    // ── FromEnvironment ──────────────────────────────────────────────────────

    [TestMethod]
    public void FromEnvironment_ReadsAllThreeVariables()
    {
        Environment.SetEnvironmentVariable(TenantEnv,   "tenant-guid");
        Environment.SetEnvironmentVariable(AudienceEnv, "api://my-app");
        Environment.SetEnvironmentVariable(ClientEnv,   "client-guid");

        var opts = EntraAuthOptions.FromEnvironment();

        Assert.AreEqual("tenant-guid",   opts.TenantId);
        Assert.AreEqual("api://my-app",  opts.Audience);
        Assert.AreEqual("client-guid",   opts.ClientId);
    }

    [TestMethod]
    public void FromEnvironment_MissingVars_PropertiesAreNull()
    {
        Environment.SetEnvironmentVariable(TenantEnv,   null);
        Environment.SetEnvironmentVariable(AudienceEnv, null);
        Environment.SetEnvironmentVariable(ClientEnv,   null);

        var opts = EntraAuthOptions.FromEnvironment();

        Assert.IsNull(opts.TenantId);
        Assert.IsNull(opts.Audience);
        Assert.IsNull(opts.ClientId);
    }

    // ── IsEnabled edge cases ─────────────────────────────────────────────────

    [TestMethod]
    public void IsEnabled_TenantPresentAudienceNull_ClientIdNull_ReturnsFalse()
    {
        var opts = new EntraAuthOptions { TenantId = "some-tenant" };
        Assert.IsFalse(opts.IsEnabled);
    }

    [TestMethod]
    public void IsEnabled_TenantPresentAudienceSet_ClientIdNull_ReturnsTrue()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "api://app" };
        Assert.IsTrue(opts.IsEnabled);
    }

    [TestMethod]
    public void IsEnabled_TenantPresentAudienceNull_ClientIdSet_ReturnsTrue()
    {
        var opts = new EntraAuthOptions { TenantId = "t", ClientId = "client-guid" };
        Assert.IsTrue(opts.IsEnabled);
    }

    [TestMethod]
    public void IsEnabled_WhitespaceTenantId_ReturnsFalse()
    {
        var opts = new EntraAuthOptions { TenantId = "   ", Audience = "api://app" };
        Assert.IsFalse(opts.IsEnabled);
    }

    // ── ValidAudiences ───────────────────────────────────────────────────────

    [TestMethod]
    public void ValidAudiences_BothAudienceAndClientId_ContainsBoth()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "api://app", ClientId = "client-guid" };
        var audiences = opts.ValidAudiences;
        CollectionAssert.Contains((System.Collections.ICollection)audiences, "api://app");
        CollectionAssert.Contains((System.Collections.ICollection)audiences, "client-guid");
        Assert.AreEqual(2, audiences.Count);
    }

    [TestMethod]
    public void ValidAudiences_OnlyClientId_ContainsOnlyClientId()
    {
        var opts = new EntraAuthOptions { TenantId = "t", ClientId = "only-client" };
        var audiences = opts.ValidAudiences;
        CollectionAssert.Contains((System.Collections.ICollection)audiences, "only-client");
        Assert.AreEqual(1, audiences.Count);
    }

    [TestMethod]
    public void ValidAudiences_OnlyAudience_ContainsOnlyAudience()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "api://only" };
        var audiences = opts.ValidAudiences;
        CollectionAssert.Contains((System.Collections.ICollection)audiences, "api://only");
        Assert.AreEqual(1, audiences.Count);
    }

    // ── MetadataAddress ──────────────────────────────────────────────────────

    [TestMethod]
    public void MetadataAddress_ContainsTenantId()
    {
        var opts = new EntraAuthOptions { TenantId = "my-tenant-id" };
        StringAssert.Contains(opts.MetadataAddress, "my-tenant-id");
        StringAssert.Contains(opts.MetadataAddress, "openid-configuration");
    }
}
