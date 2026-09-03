/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  WOLF-8516: unit tests for EntraIdentityOptions.FromEnvironment() — the shared parser
 *  for the merged WAREWOLF_ENTRA_CONFIG JSON app setting.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads/writes process-global environment variables
public class EntraIdentityOptionsTests
{
    private const string ConfigEnv = EntraIdentityOptions.EnvVar;

    private string? _previousConfig;

    [TestInitialize]
    public void SaveEnvironment() => _previousConfig = Environment.GetEnvironmentVariable(ConfigEnv);

    [TestCleanup]
    public void RestoreEnvironment() => Environment.SetEnvironmentVariable(ConfigEnv, _previousConfig);

    [TestMethod]
    public void FromEnvironment_FullJson_ParsesAllFourFields()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"t\",\"audience\":\"a\",\"clientId\":\"c\",\"serviceBusAudience\":\"sb\"}");

        var opts = EntraIdentityOptions.FromEnvironment();

        Assert.AreEqual("t", opts.TenantId);
        Assert.AreEqual("a", opts.Audience);
        Assert.AreEqual("c", opts.ClientId);
        Assert.AreEqual("sb", opts.ServiceBusAudience);
    }

    [TestMethod]
    public void FromEnvironment_MissingVar_AllFieldsNull()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, null);

        var opts = EntraIdentityOptions.FromEnvironment();

        Assert.IsNull(opts.TenantId);
        Assert.IsNull(opts.Audience);
        Assert.IsNull(opts.ClientId);
        Assert.IsNull(opts.ServiceBusAudience);
    }

    [TestMethod]
    public void FromEnvironment_PartialJson_AbsentFieldsAreNull()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, /*lang=json,strict*/ "{\"tenantId\":\"t\"}");

        var opts = EntraIdentityOptions.FromEnvironment();

        Assert.AreEqual("t", opts.TenantId);
        Assert.IsNull(opts.Audience);
        Assert.IsNull(opts.ClientId);
        Assert.IsNull(opts.ServiceBusAudience);
    }

    [TestMethod]
    public void FromEnvironment_MalformedJson_TreatedAsUnset_DoesNotThrow()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, "{ not valid json");

        var opts = EntraIdentityOptions.FromEnvironment();

        Assert.IsNull(opts.TenantId);
        Assert.IsNull(opts.Audience);
        Assert.IsNull(opts.ClientId);
        Assert.IsNull(opts.ServiceBusAudience);
    }

    [TestMethod]
    public void FromEnvironment_CaseInsensitivePropertyNames()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, /*lang=json,strict*/ "{\"TENANTID\":\"t\",\"Audience\":\"a\"}");

        var opts = EntraIdentityOptions.FromEnvironment();

        Assert.AreEqual("t", opts.TenantId);
        Assert.AreEqual("a", opts.Audience);
    }
}
