/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// (OBS-06) Lightweight startup health check.  Emits a single log line at
/// <see cref="LogLevel.Warning"/> when Entra bearer validation is not
/// configured, so operators see immediately when the token path is inactive.
///
/// No timer, no per-request cost — this runs once per cold start and exits.
/// </summary>
internal sealed class EntraAuthHealthCheck : IHostedService
{
    private readonly EntraAuthOptions _options;
    private readonly ILogger<EntraAuthHealthCheck> _logger;

    public EntraAuthHealthCheck(
        EntraAuthOptions options,
        ILogger<EntraAuthHealthCheck> logger)
    {
        _options = options;
        _logger  = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.IsEnabled)
        {
            // Tenant ID and audience values identify the deployment's Entra objects —
            // Debug-only, never in production Info/Error/Warning entries.
            _logger.LogInformation("EntraAuthHealthCheck: bearer-token validation ENABLED.");
            _logger.LogDebug(
                "EntraAuthHealthCheck: bearer-token validation ENABLED — TenantId={TenantId} Audiences=[{Audiences}]",
                _options.TenantId,
                string.Join(", ", _options.ValidAudiences));
        }
        else
        {
            _logger.LogWarning(
                "EntraAuthHealthCheck: bearer-token validation DISABLED — set " +
                "WAREWOLF_ENTRA_TENANT_ID and WAREWOLF_ENTRA_AUDIENCE / WAREWOLF_ENTRA_CLIENT_ID " +
                "to enable. Easy Auth principal flow remains active.");
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
