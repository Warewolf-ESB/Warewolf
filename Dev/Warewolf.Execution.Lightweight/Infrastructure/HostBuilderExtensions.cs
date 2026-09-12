/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// Extension methods for <see cref="IHostBuilder"/> that wire up all Warewolf
/// services in a single call, keeping <c>Program.cs</c> free from DI details.
/// </summary>
internal static class HostBuilderExtensions
{
    /// <summary>
    /// Configures the Azure Functions isolated worker defaults and registers all
    /// Warewolf services according to the supplied <paramref name="config"/>.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <param name="config">Immutable environment configuration snapshot.</param>
    /// <param name="serviceBusTrigger">
    /// Loaded once in <c>Program.cs</c> (before the host is built) via
    /// <see cref="ServiceBusTriggerDefinition.Load"/> — <c>null</c> when the trigger is
    /// disabled. Its <see cref="ServiceBusTriggerDefinition.QueueName"/> is injected into
    /// configuration here, BEFORE <c>Build()</c>, so the
    /// <c>[ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")]</c> attribute
    /// indirection on <c>Functions.ServiceBusWorkflowTriggerFunction</c> resolves it exactly
    /// as it previously resolved the app setting of the same name.
    /// </param>
    internal static IHostBuilder ConfigureWarewolf(
        this IHostBuilder     builder,
        HostEnvironmentConfig config,
        ServiceBusTriggerDefinition? serviceBusTrigger = null)
        => builder
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                // ── Usage/uptime publish — MUST be the very first middleware so its
                //    stopwatch spans the FULL request/response lifecycle (including
                //    every middleware below and the function body itself), instead of
                //    only the workflow-execution portion. Its post-`next()` code still
                //    runs last-out, after everything downstream has completed —
                //    including error paths — so publishing here does not lose access
                //    to the workflow-execution facts recorded further down the
                //    pipeline (see UsagePublishContext). ─────────────────────────────
                worker.UseMiddleware<UsagePublishMiddleware>();

                // ── Instance correlation — MUST run right after usage-publish so all
                //    subsequent middleware and function code inherits the scope ─────
                worker.UseMiddleware<InstanceCorrelationMiddleware>();

                // ── Auth middleware pipeline — ORDER IS CRITICAL ──────────────
                // 1. Intercept unauthenticated requests to /secure/* before
                //    Easy Auth redirect fires. Pass /public/* straight through.
                worker.UseMiddleware<EasyAuthRedirectMiddleware>();

                // 2. Decode X-MS-CLIENT-PRINCIPAL Easy Auth header into a
                //    strongly-typed WorkflowClaimsPrincipal and store it in
                //    FunctionContext.Items.
                worker.UseMiddleware<ClaimsPrincipalBuilderMiddleware>();

                // 3. Enforce secure.config group + permission policies on
                //    /secure/* routes. Returns 403 if the caller lacks access.
                worker.UseMiddleware<WorkflowAuthorizationMiddleware>();
            })
            // Registered AFTER ConfigureFunctionsWorkerDefaults (which adds its own
            // local.settings.json / environment-variable configuration sources) so this
            // in-memory override, when present, wins over any leftover
            // WAREWOLF_SERVICEBUS_TRIGGER_QUEUE app setting — later-registered sources take
            // precedence for the same key in the .NET generic host.
            .ConfigureAppConfiguration((_, appConfig) =>
            {
                if (serviceBusTrigger is not null)
                {
                    appConfig.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>(
                            "WAREWOLF_SERVICEBUS_TRIGGER_QUEUE", serviceBusTrigger.QueueName),
                    });
                }
            })
            .ConfigureServices(services =>
            {
                services.AddCoreServices(config, serviceBusTrigger);

                if (config.EncryptionEnabled)
                    services.AddKeyVaultEncryption(config);
            });
}
