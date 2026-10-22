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
            // The middleware and its order: WarewolfMiddleware.Order, shared with the self-hosted engine.
            .ConfigureFunctionsWorkerDefaults(worker => worker.UseWarewolfMiddleware())
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
