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
    private const string TenantVar = "WAREWOLF_ENTRA_TENANT_ID";
    private const string ServiceBusAudienceVar = "WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE";
    private const string HttpAudienceVar = "WAREWOLF_ENTRA_AUDIENCE";

    private string? _previousTenant;
    private string? _previousServiceBusAudience;
    private string? _previousHttpAudience;

    [TestInitialize]
    public void SaveEnvironment()
    {
        _previousTenant = Environment.GetEnvironmentVariable(TenantVar);
        _previousServiceBusAudience = Environment.GetEnvironmentVariable(ServiceBusAudienceVar);
        _previousHttpAudience = Environment.GetEnvironmentVariable(HttpAudienceVar);
    }

    [TestCleanup]
    public void RestoreEnvironment()
    {
        Environment.SetEnvironmentVariable(TenantVar, _previousTenant);
        Environment.SetEnvironmentVariable(ServiceBusAudienceVar, _previousServiceBusAudience);
        Environment.SetEnvironmentVariable(HttpAudienceVar, _previousHttpAudience);
    }

    [TestMethod]
    public void FromEnvironment_ReadsDedicatedServiceBusAudience_AndSharedTenant()
    {
        Environment.SetEnvironmentVariable(TenantVar, "11111111-1111-1111-1111-111111111111");
        Environment.SetEnvironmentVariable(ServiceBusAudienceVar, "api://servicebus-trigger-app");
        Environment.SetEnvironmentVariable(HttpAudienceVar, "api://http-app");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.AreEqual("11111111-1111-1111-1111-111111111111", options.TenantId);
        Assert.AreEqual("api://servicebus-trigger-app", options.Audience);
        Assert.IsTrue(options.IsEnabled);
    }

    [TestMethod]
    public void FromEnvironment_NeverFallsBackToHttpAudience()
    {
        Environment.SetEnvironmentVariable(TenantVar, "11111111-1111-1111-1111-111111111111");
        Environment.SetEnvironmentVariable(ServiceBusAudienceVar, null);
        Environment.SetEnvironmentVariable(HttpAudienceVar, "api://http-app");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.AreNotEqual("api://http-app", options.Audience);
        Assert.IsNull(options.Audience);
    }

    [TestMethod]
    public void FromEnvironment_MissingServiceBusAudience_NotEnabled_FailsClosed()
    {
        Environment.SetEnvironmentVariable(TenantVar, "11111111-1111-1111-1111-111111111111");
        Environment.SetEnvironmentVariable(ServiceBusAudienceVar, null);

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.IsFalse(options.IsEnabled,
            "Missing the dedicated Service Bus audience must fail closed — the trigger dead-letters every message.");
    }

    [TestMethod]
    public void FromEnvironment_MissingTenant_NotEnabled_FailsClosed()
    {
        Environment.SetEnvironmentVariable(TenantVar, null);
        Environment.SetEnvironmentVariable(ServiceBusAudienceVar, "api://servicebus-trigger-app");

        var options = ServiceBusEntraAuthOptions.FromEnvironment();

        Assert.IsFalse(options.IsEnabled);
    }

    [TestMethod]
    public void IsServiceBusEntraAuthOptions_ASubclassOfEntraAuthOptions()
    {
        Assert.IsInstanceOfType(ServiceBusEntraAuthOptions.FromEnvironment(), typeof(EntraAuthOptions));
    }
}
