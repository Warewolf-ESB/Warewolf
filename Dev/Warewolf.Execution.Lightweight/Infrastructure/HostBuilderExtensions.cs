/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Extensions.Hosting;
using Warewolf.Execution.Lightweight.Auth.Middleware;

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
            .ConfigureFunctionsWorkerDefaults(worker =>
            {
                // ── Instance correlation — MUST be first so all subsequent
                //    middleware and function code inherits the scope ───────────
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
            .ConfigureServices(services =>
            {
                services.AddCoreServices(config);

                if (config.EncryptionEnabled)
                    services.AddKeyVaultEncryption(config);
            });
}
