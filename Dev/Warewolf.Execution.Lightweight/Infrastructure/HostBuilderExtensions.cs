/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Hosting;

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
    internal static IHostBuilder ConfigureWarewolf(
        this IHostBuilder     builder,
        HostEnvironmentConfig config)
        => builder
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddCoreServices(config.WorkflowsDirectory);

                if (config.EncryptionEnabled)
                    services.AddKeyVaultEncryption(config);
            });
}
