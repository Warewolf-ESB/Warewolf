/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;

namespace Warewolf.Execution.Lightweight.Infrastructure;

/// <summary>
/// No-op <see cref="IWarewolfPerformanceCounterLocater"/> for the Azure Execution Engine.
///
/// Windows performance counters (<c>PerformanceCounterCategory.Create</c>) require admin
/// rights and are blocked by the Azure App Service sandbox, and Azure already provides
/// its own telemetry (Application Insights). Registering this locater into
/// <c>CustomContainer</c> at cold start short-circuits the
/// <c>CustomContainer.Get&lt;IWarewolfPerformanceCounterLocater&gt;() == null</c> check in
/// <c>HangfireScheduler.LoadAndRegisterTypes</c>, so the real counter machinery is never
/// constructed in this host. The on-prem Server is unaffected — it registers the real
/// <c>WarewolfPerformanceCounterManager</c> as before.
///
/// Every lookup returns the same shared <see cref="EmptyCounter"/> instance:
/// <see cref="EmptyCounter"/> is stateless (all mutating members are no-ops), so a single
/// instance is safe under concurrent use.
/// </summary>
internal sealed class NoOpPerformanceCounterLocater : IWarewolfPerformanceCounterLocater
{
    private static readonly EmptyCounter Counter = new();

    public IPerformanceCounter GetCounter(string name) => Counter;

    public IPerformanceCounter GetCounter(WarewolfPerfCounterType type) => Counter;

    public IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type) => Counter;
}
