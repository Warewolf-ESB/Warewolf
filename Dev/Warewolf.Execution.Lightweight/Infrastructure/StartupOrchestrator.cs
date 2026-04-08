/*
 *  Warewolf - Once bitten, there's no goingback
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Orchestrates the post-<see cref="IHost.Build"/> startup sequence:
/// <list type="number">
///   <item>Key Vault initialisation (when encryption is enabled).</item>
///   <item>Workflow index warm-up (pre-loads the O(1) lookup table).</item>
/// </list>
///
/// Both phases are individually guarded with structured error handling so that
/// a failure in either phase produces a meaningful, structured log entry before
/// the process terminates.
/// </summary>
internal static class StartupOrchestrator
{
    /// <summary>
    /// Executes all startup phases in order.  Throws on the first unrecoverable
    /// failure so the Azure Functions host refuses to accept traffic.
    /// </summary>
    /// <param name="host">The fully built <see cref="IHost"/>.</param>
    /// <param name="config">Immutable environment configuration snapshot.</param>
    internal static async Task RunStartupAsync(IHost host, HostEnvironmentConfig config)
    {
        var logger = host.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(StartupOrchestrator));

        await InitializeEncryptionAsync(host, config, logger);
        WarmUpWorkflowIndex(config, logger);
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    static async Task InitializeEncryptionAsync(
        IHost                 host,
        HostEnvironmentConfig config,
        ILogger               logger)
    {
        if (!config.EncryptionEnabled)
            return;

        try
        {
            await host.InitializeKeyVaultAsync(config).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // InitializeKeyVaultAsync already writes the AuditLogger entry.
            // Log a host-level fatal entry here so the Azure Functions runtime
            // captures it even when the AuditLogger itself fails.
            logger.LogCritical(ex,
                "Startup | Phase=KeyVaultInit | Status=Failed | " +
                "The host cannot start without AES key material.");
            throw; // Propagate to abort cold start.
        }
    }

    static void WarmUpWorkflowIndex(HostEnvironmentConfig config, ILogger logger)
    {
        try
        {
            WorkflowIndex.Instance.WarmUp(config.WorkflowsDirectory);
            logger.LogInformation(
                "Startup | Phase=WorkflowIndexWarmUp | Status=Completed | " +
                "Directory={WorkflowsDirectory}", config.WorkflowsDirectory);
        }
        catch (Exception ex)
        {
            // Warm-up failure is non-fatal: WorkflowIndex falls back to
            // disk-based resolution on the first HTTP request.
            logger.LogWarning(ex,
                "Startup | Phase=WorkflowIndexWarmUp | Status=Degraded | " +
                "Falling back to on-demand disk resolution. " +
                "Directory={WorkflowsDirectory}", config.WorkflowsDirectory);
        }
    }
}
