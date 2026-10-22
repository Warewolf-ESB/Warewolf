/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using SameSite = Microsoft.Azure.Functions.Worker.Http.SameSite;

namespace Warewolf.Execution.Lightweight.SelfHost;

/// <summary>
/// Runs the engine without the Azure Functions host: its HTTP functions on Kestrel, through the same
/// middleware, as the "Warewolf Server" Windows service or a systemd unit. This is how a packaged engine
/// runs on a VPS beside WolfClaw (docs in the WolfClaw plans: warewolf-devops-mcp
/// docs/plans/wolfclaw-package.md). Started with <c>--self-host</c> or <c>WAREWOLF_SELF_HOST=1</c>.
/// <para>
/// What differs from Azure: it listens on <c>WAREWOLF_SELFHOST_URLS</c> (loopback port 3142 by default),
/// keeps workflows in <c>WAREWOLF_DATA_DIR</c> (%ProgramData%\Warewolf or /var/lib/warewolf), skips the
/// licence check (a locally hosted engine is free), drops Easy Auth principal headers (nothing here
/// validates them) and runs no Service Bus triggers or Application Insights.
/// </para>
/// </summary>
internal static class SelfHostedEngine
{
    internal const string ServiceName = "Warewolf Server";
    internal const string DefaultUrls = "http://127.0.0.1:3142";

    internal static bool IsRequested(string[] args, Func<string, string?> getEnv) =>
        args.Contains("--self-host", StringComparer.OrdinalIgnoreCase)
        || getEnv("WAREWOLF_SELF_HOST") is { } value && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));

    /// <summary>The self-hosted defaults, set before the engine reads its configuration. Anything already set wins.</summary>
    internal static void ApplyDefaults(Func<string, string?> getEnv, Action<string, string> setEnv, bool isWindows)
    {
        void Default(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(getEnv(name))) setEnv(name, value);
        }

        Default("WAREWOLF_DATA_DIR", DefaultDataDirectory(isWindows, getEnv));
        Default("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
        Default("WAREWOLF_SELFHOST_URLS", DefaultUrls);
    }

    internal static string DefaultDataDirectory(bool isWindows, Func<string, string?> getEnv) =>
        isWindows
            ? Path.Combine(getEnv("ProgramData") ?? @"C:\ProgramData", "Warewolf")
            : "/var/lib/warewolf";

    internal static IHost Build(string[] args, HostEnvironmentConfig config, LoggingConfiguration loggingConfig)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args, ContentRootPath = AppContext.BaseDirectory });
        builder.Host.UseWindowsService(options => options.ServiceName = ServiceName).UseSystemd();
        builder.WebHost.UseUrls(Environment.GetEnvironmentVariable("WAREWOLF_SELFHOST_URLS") ?? DefaultUrls);

        builder.Logging.ClearProviders();
        if (loggingConfig.EnableConsoleLogging) builder.Logging.AddConsole();
        builder.Logging.AddFilter("Warewolf.Execution.Lightweight", loggingConfig.MelMinimumLevel);

        builder.Services.AddExecutionLogging(loggingConfig);
        builder.Services.AddCoreServices(config, serviceBusTrigger: null);
        if (config.EncryptionEnabled) builder.Services.AddKeyVaultEncryption(config);
        foreach (var middleware in WarewolfMiddleware.Order) builder.Services.AddSingleton(middleware);

        var app = builder.Build();
        foreach (var function in HttpFunction.Discover(typeof(SelfHostedEngine).Assembly))
        {
            RequestDelegate handle = http => HandleAsync(http, function);
            if (function.Methods.Length == 0) app.Map(function.Route, handle);
            else app.MapMethods(function.Route, function.Methods, handle);
        }
        return app;
    }

    /// <summary>One request: the function's context, the middleware (<see cref="WarewolfMiddleware.Order"/>), the function, the response.</summary>
    internal static async Task HandleAsync(HttpContext http, HttpFunction function, IReadOnlyList<Type>? middlewareOrder = null)
    {
        var order = middlewareOrder ?? WarewolfMiddleware.Order;
        var context = new SelfHostFunctionContext(function, http.RequestServices, BindingData(http.Request), http.RequestAborted);
        context.Request = await SelfHostHttpRequestData.CreateAsync(context, http.Request);

        FunctionExecutionDelegate pipeline = async _ =>
        {
            var returned = await function.InvokeAsync(context);
            if (returned is not null) context.Response = returned;
        };
        for (var i = order.Count - 1; i >= 0; i--)
        {
            var middleware = (IFunctionsWorkerMiddleware)http.RequestServices.GetRequiredService(order[i]);
            var next = pipeline;
            pipeline = ctx => middleware.Invoke(ctx, next);
        }

        await pipeline(context);
        await WriteAsync(http.Response, context.Response);
    }

    /// <summary>What the Functions host binds parameters from: the query string, then the route values over it.</summary>
    internal static IReadOnlyDictionary<string, object?> BindingData(HttpRequest request)
    {
        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in request.Query) data[key] = value.ToString();
        foreach (var (key, value) in request.RouteValues) data[key] = value;
        return data;
    }

    internal static async Task WriteAsync(HttpResponse response, HttpResponseData? result)
    {
        if (result is null)
        {
            response.StatusCode = StatusCodes.Status204NoContent;
            return;
        }

        response.StatusCode = (int)result.StatusCode;
        foreach (var header in result.Headers) response.Headers.Append(header.Key, header.Value.ToArray());
        if (result is SelfHostHttpResponseData selfHosted)
        {
            foreach (var cookie in selfHosted.AppendedCookies)
            {
                response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions
                {
                    Domain = cookie.Domain,
                    Path = cookie.Path,
                    Expires = cookie.Expires,
                    HttpOnly = cookie.HttpOnly ?? false,
                    Secure = cookie.Secure ?? false,
                    MaxAge = cookie.MaxAge is { } seconds ? TimeSpan.FromSeconds(seconds) : null,
                    SameSite = cookie.SameSite switch
                    {
                        SameSite.Strict => Microsoft.AspNetCore.Http.SameSiteMode.Strict,
                        SameSite.Lax => Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                        SameSite.ExplicitNone => Microsoft.AspNetCore.Http.SameSiteMode.None,
                        _ => Microsoft.AspNetCore.Http.SameSiteMode.Unspecified,
                    },
                });
            }
        }

        if (result.Body.CanSeek) result.Body.Position = 0;
        await result.Body.CopyToAsync(response.Body, response.HttpContext.RequestAborted);
    }

    internal static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
}
