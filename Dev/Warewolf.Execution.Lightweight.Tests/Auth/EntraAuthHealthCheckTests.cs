/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests for EntraAuthHealthCheck — a simple IHostedService that logs one line
 *  at startup and exits.  Covers:
 *  - StartAsync when bearer validation is ENABLED → LogInformation path
 *  - StartAsync when bearer validation is DISABLED → LogWarning path
 *  - StopAsync → completes without error
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests.Auth;

[TestClass]
public class EntraAuthHealthCheckTests
{
    private static EntraAuthHealthCheck Build(EntraAuthOptions options) =>
        new(options, NullLogger<EntraAuthHealthCheck>.Instance);

    // ── StartAsync ───────────────────────────────────────────────────────────

    [TestMethod]
    public async Task StartAsync_WhenEnabled_CompletesWithoutException()
    {
        var options    = new EntraAuthOptions
        {
            TenantId = "my-tenant",
            Audience = "api://my-app",
        };
        var healthCheck = Build(options);

        // Act — should complete and log at Information level; no exception expected
        await healthCheck.StartAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task StartAsync_WhenDisabled_CompletesWithoutException()
    {
        // TenantId absent → IsEnabled == false → LogWarning path
        var options     = new EntraAuthOptions();
        var healthCheck = Build(options);

        await healthCheck.StartAsync(CancellationToken.None);
    }

    [TestMethod]
    public async Task StartAsync_WhenEnabled_ReturnsCompletedTask()
    {
        var options     = new EntraAuthOptions { TenantId = "t", ClientId = "c" };
        var healthCheck = Build(options);

        var task = healthCheck.StartAsync(CancellationToken.None);

        Assert.IsTrue(task.IsCompleted);
        await task; // ensure no exception surfaces
    }

    [TestMethod]
    public async Task StartAsync_WhenDisabled_ReturnsCompletedTask()
    {
        var healthCheck = Build(new EntraAuthOptions());

        var task = healthCheck.StartAsync(CancellationToken.None);

        Assert.IsTrue(task.IsCompleted);
        await task;
    }

    // ── StopAsync ────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task StopAsync_ReturnsCompletedTask()
    {
        var healthCheck = Build(new EntraAuthOptions());

        var task = healthCheck.StopAsync(CancellationToken.None);

        Assert.IsTrue(task.IsCompleted);
        await task;
    }

    [TestMethod]
    public async Task StopAsync_WithCancelledToken_DoesNotThrow()
    {
        var healthCheck = Build(new EntraAuthOptions());
        using var cts   = new CancellationTokenSource();
        cts.Cancel();

        // StopAsync is fire-and-complete so cancellation should still work
        await healthCheck.StopAsync(cts.Token);
    }
}
