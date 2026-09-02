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
    // WOLF-8516: WAREWOLF_ENTRA_TENANT_ID / WAREWOLF_ENTRA_AUDIENCE / WAREWOLF_ENTRA_CLIENT_ID
    // were merged into one JSON app setting — see EntraIdentityOptions.
    private const string ConfigEnv = EntraIdentityOptions.EnvVar;

    private string? _origConfig;

    [TestInitialize]
    public void SaveEnv()
    {
        _origConfig = Environment.GetEnvironmentVariable(ConfigEnv);
    }

    [TestCleanup]
    public void RestoreEnv()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, _origConfig);
    }

    // ── FromEnvironment ──────────────────────────────────────────────────────

    [TestMethod]
    public void FromEnvironment_ReadsAllThreeVariables()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"tenant-guid\",\"audience\":\"api://my-app\",\"clientId\":\"client-guid\"}");

        var opts = EntraAuthOptions.FromEnvironment();

        Assert.AreEqual("tenant-guid",   opts.TenantId);
        Assert.AreEqual("api://my-app",  opts.Audience);
        Assert.AreEqual("client-guid",   opts.ClientId);
    }

    [TestMethod]
    public void FromEnvironment_MissingVars_PropertiesAreNull()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, null);

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

    // ── Audience normalization ───────────────────────────────────────────────
    // A misconfigured WAREWOLF_ENTRA_AUDIENCE holding a bare client GUID (no URI
    // scheme) must be auto-prefixed with "api://" so it still matches the `aud`
    // claim Entra issues for an api://{clientId}/.default scope request, instead
    // of rejecting every caller with 401 (regression guard for 8511-correct-id:
    // pipeline-CLOUD.yml previously pointed the QueueProcessor at the wrong
    // engine app id and every delivery was dead-lettered with a 401).

    [TestMethod]
    public void Audience_BareGuid_IsPrefixedWithApiScheme()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "05794411-b275-4801-97ac-8b078ed7196c" };
        Assert.AreEqual("api://05794411-b275-4801-97ac-8b078ed7196c", opts.Audience);
    }

    [TestMethod]
    public void Audience_AlreadyApiScheme_IsUnchanged()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "api://my-app" };
        Assert.AreEqual("api://my-app", opts.Audience);
    }

    [TestMethod]
    public void Audience_HttpsScheme_IsUnchanged()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "https://my-tenant.onmicrosoft.com/my-app" };
        Assert.AreEqual("https://my-tenant.onmicrosoft.com/my-app", opts.Audience);
    }

    [TestMethod]
    public void Audience_Null_StaysNull()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = null };
        Assert.IsNull(opts.Audience);
    }

    [TestMethod]
    public void Audience_EmptyOrWhitespace_IsUnchanged()
    {
        var opts = new EntraAuthOptions { TenantId = "t", Audience = "   " };
        Assert.AreEqual("   ", opts.Audience);
    }

    [TestMethod]
    public void FromEnvironment_BareGuidAudience_IsPrefixedWithApiScheme()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"tenant-guid\",\"audience\":\"05794411-b275-4801-97ac-8b078ed7196c\"}");

        var opts = EntraAuthOptions.FromEnvironment();

        Assert.AreEqual("api://05794411-b275-4801-97ac-8b078ed7196c", opts.Audience);
    }
}
