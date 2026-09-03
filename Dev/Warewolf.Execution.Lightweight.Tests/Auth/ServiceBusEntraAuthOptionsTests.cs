/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusEntraAuthOptions.FromEnvironment() — pins the two
 *  behaviours the spec's confused-deputy prevention (§4.2 step 2) depends on:
 *   - it reads the DEDICATED WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE, not the general
 *     HTTP WAREWOLF_ENTRA_AUDIENCE;
 *   - it shares the tenant with the HTTP path (WAREWOLF_ENTRA_TENANT_ID).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
[DoNotParallelize] // reads/writes process-global environment variables
public class ServiceBusEntraAuthOptionsTests
{
    // WOLF-8516: WAREWOLF_ENTRA_TENANT_ID / _AUDIENCE / _SERVICEBUS_AUDIENCE were merged into
    // one JSON app setting — see EntraIdentityOptions.
    private const string ConfigEnv = EntraIdentityOptions.EnvVar;

    private string? _previousConfig;

    [TestInitialize]
    public void SaveEnvironment()
    {
        _previousConfig = Environment.GetEnvironmentVariable(ConfigEnv);
    }

    [TestCleanup]
    public void RestoreEnvironment()
    {
        Environment.SetEnvironmentVariable(ConfigEnv, _previousConfig);
    }

    [TestMethod]
    public void FromEnvironment_ReadsDedicatedServiceBusAudience_AndSharedTenant()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"11111111-1111-1111-1111-111111111111\",\"audience\":\"api://http-app\",\"serviceBusAudience\":\"api://servicebus-trigger-app\"}");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.AreEqual("11111111-1111-1111-1111-111111111111", options.TenantId);
        Assert.AreEqual("api://servicebus-trigger-app", options.Audience);
        Assert.IsTrue(options.IsEnabled);
    }

    [TestMethod]
    public void FromEnvironment_NeverFallsBackToHttpAudience()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"11111111-1111-1111-1111-111111111111\",\"audience\":\"api://http-app\"}");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.AreNotEqual("api://http-app", options.Audience);
        Assert.IsNull(options.Audience);
    }

    [TestMethod]
    public void FromEnvironment_MissingServiceBusAudience_NotEnabled_FailsClosed()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"tenantId\":\"11111111-1111-1111-1111-111111111111\"}");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.IsFalse(options.IsEnabled,
            "Missing the dedicated Service Bus audience must fail closed — the trigger dead-letters every message.");
    }

    [TestMethod]
    public void FromEnvironment_MissingTenant_NotEnabled_FailsClosed()
    {
        Environment.SetEnvironmentVariable(ConfigEnv,
            /*lang=json,strict*/ "{\"serviceBusAudience\":\"api://servicebus-trigger-app\"}");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.IsFalse(options.IsEnabled);
    }

    [TestMethod]
    public void IsServiceBusEntraAuthOptions_ASubclassOfEntraAuthOptions()
    {
        Assert.IsInstanceOfType(ServiceBusEntraAuthOptions.FromEnvironment(), typeof(EntraAuthOptions));
    }
}
