/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Warewolf.Driver.Persistence;

namespace Warewolf.Execution.Lightweight;

/// <summary>
/// Ambient per-execution context consumed when a workflow suspends inside the engine.
///
/// <see cref="WorkflowExecutor"/> opens a scope around the activity-chain walk; if a
/// <c>SuspendExecutionActivity</c> in that chain schedules a persistence job,
/// <see cref="LightweightJobValuesEnricher"/> (registered into <c>CustomContainer</c> at
/// startup) reads the active scope and stamps engine-specific keys into the persisted
/// job values. This keeps <c>SuspendExecutionActivity</c> and the five legacy job keys
/// (<c>resourceID</c>, <c>environment</c>, <c>startActivityId</c>, <c>versionNumber</c>,
/// <c>currentuserprincipal</c>) completely untouched — Server-created jobs and
/// engine-created jobs stay schema-compatible.
///
/// <see cref="AsyncLocal{T}"/> flows through the synchronous activity walk and any
/// awaits inside it, and is naturally isolated between concurrent requests.
/// </summary>
internal static class SuspendSnapshotContext
{
    static readonly AsyncLocal<Snapshot?> _current = new();

    internal static Snapshot? Current => _current.Value;

    /// <summary>Opens a snapshot scope; dispose to clear it (restores the previous value).</summary>
    internal static IDisposable BeginScope(string workflowName, string workflowFilePath, Guid executionId)
    {
        var previous = _current.Value;
        _current.Value = new Snapshot(workflowName, workflowFilePath, executionId);
        return new Popper(previous);
    }

    internal sealed record Snapshot(string WorkflowName, string WorkflowFilePath, Guid ExecutionId);

    sealed class Popper : IDisposable
    {
        readonly Snapshot? _previous;
        public Popper(Snapshot? previous) => _previous = previous;
        public void Dispose() => _current.Value = _previous;
    }
}

/// <summary>
/// <see cref="IJobValuesEnricher"/> implementation for the engine: adds
/// engine-resume metadata to the persisted job values created by
/// <c>HangfireScheduler.ScheduleJob</c>. All keys are additive — resume paths that
/// don't know them (the on-prem Server) ignore them.
/// </summary>
internal sealed class LightweightJobValuesEnricher : IJobValuesEnricher
{
    internal const string WorkflowNameKey     = "engineWorkflowName";
    internal const string WorkflowFilePathKey = "engineWorkflowFilePath";
    internal const string ExecutionIdKey      = "engineExecutionId";
    internal const string SuspendedAtUtcKey   = "engineSuspendedAtUtc";

    public void Enrich(Dictionary<string, StringBuilder> values)
    {
        var snapshot = SuspendSnapshotContext.Current;
        if (snapshot is null || values is null)
        {
            return;
        }

        values[WorkflowNameKey]     = new StringBuilder(snapshot.WorkflowName ?? string.Empty);
        values[WorkflowFilePathKey] = new StringBuilder(snapshot.WorkflowFilePath ?? string.Empty);
        values[ExecutionIdKey]      = new StringBuilder(snapshot.ExecutionId.ToString());
        values[SuspendedAtUtcKey]   = new StringBuilder(DateTime.UtcNow.ToString("O"));
    }
}
