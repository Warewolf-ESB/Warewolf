/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Warewolf.Execution.Lightweight.Auth.Middleware;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// The engine's function middleware, in order. One list for both hosts: the Functions worker
/// (<see cref="HostBuilderExtensions.ConfigureWarewolf"/>) and the self-hosted engine
/// (<see cref="SelfHost.SelfHostedEngine"/>), so the two can't run different pipelines.
/// </summary>
internal static class WarewolfMiddleware
{
    internal static readonly Type[] Order =
    {
        // ── Usage/uptime publish — MUST be the very first middleware so its stopwatch spans the
        //    FULL request/response lifecycle (including every middleware below and the function
        //    body itself), instead of only the workflow-execution portion. Its post-`next()` code
        //    still runs last-out, after everything downstream has completed — including error
        //    paths — so publishing here does not lose access to the workflow-execution facts
        //    recorded further down the pipeline (see UsagePublishContext).
        typeof(UsagePublishMiddleware),

        // ── Instance correlation — MUST run right after usage-publish so all subsequent
        //    middleware and function code inherits the scope.
        typeof(InstanceCorrelationMiddleware),

        // ── Auth middleware pipeline — ORDER IS CRITICAL ──────────────
        // 1. Intercept unauthenticated requests to /secure/* before Easy Auth redirect fires.
        //    Pass /public/* straight through.
        typeof(EasyAuthRedirectMiddleware),

        // 2. Decode X-MS-CLIENT-PRINCIPAL Easy Auth header into a strongly-typed
        //    WorkflowClaimsPrincipal and store it in FunctionContext.Items.
        typeof(ClaimsPrincipalBuilderMiddleware),

        // 3. Enforce secure.config group + permission policies on /secure/* routes. Returns 403
        //    if the caller lacks access.
        typeof(WorkflowAuthorizationMiddleware),
    };

    /// <summary>
    /// Adds <see cref="Order"/> to the Functions worker. The same thing the worker's generic
    /// <c>UseMiddleware&lt;T&gt;()</c> does for each: a singleton, resolved per invocation.
    /// </summary>
    internal static IFunctionsWorkerApplicationBuilder UseWarewolfMiddleware(this IFunctionsWorkerApplicationBuilder worker)
    {
        foreach (var type in Order)
        {
            worker.Services.AddSingleton(type);
            worker.Use(next => context => ((IFunctionsWorkerMiddleware)context.InstanceServices.GetRequiredService(type)).Invoke(context, next));
        }
        return worker;
    }
}
