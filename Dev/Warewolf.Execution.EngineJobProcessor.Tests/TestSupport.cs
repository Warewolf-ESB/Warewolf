/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Shared test helpers for the JobProcessor suite.
 */

using Dev2.Common;
using Dev2.Common.Wrappers;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;

namespace Warewolf.Execution.EngineJobProcessor.Tests;

/// <summary>
/// A public, Hangfire-serialisable seed method. <c>[AutomaticRetry(Attempts = 0)]</c>
/// mirrors the real suspend job so a transition to <c>Failed</c> is NOT rescheduled back
/// to <c>Scheduled</c> by Hangfire's global retry filter (which would defeat fail-only).
/// The poller/reaper never invoke this body — they only read job state.
/// </summary>
public static class ProcessorSeed
{
    [AutomaticRetry(Attempts = 0)]
    public static void Continuation() { }
}

internal static class TestSupport
{
    /// <summary>Creates a job in the <c>Scheduled</c> state with the given (UTC) enqueue time.</summary>
    public static string SeedScheduled(MemoryStorage storage, DateTime enqueueAtUtc)
    {
        var client = new BackgroundJobClient(storage);
        return client.Create(() => ProcessorSeed.Continuation(), new ScheduledState(enqueueAtUtc));
    }

    /// <summary>
    /// Swaps <see cref="Config.Persistence"/> for a deterministic instance backed by a
    /// temp file (machine state on dev boxes is unpredictable — a real Warewolf install
    /// may own <c>C:\ProgramData\Warewolf\Server Settings\persistencesettings.json</c>).
    /// Dispose restores the previous instance. Callers must be <c>[DoNotParallelize]</c>.
    /// </summary>
    public static IDisposable SwapPersistence(bool enable)
    {
        var previous = Config.Persistence;

        var dir = Directory.CreateTempSubdirectory("wwjobproc-").FullName;
        var path = Path.Combine(dir, "persistencesettings.json");
        File.WriteAllText(path, $$"""{"Enable": {{(enable ? "true" : "false")}}, "PersistenceScheduler": "Hangfire"}""");
        Config.Persistence = new PersistenceSettings(path, new FileWrapper(), new DirectoryWrapper());

        return new Restorer(() =>
        {
            Config.Persistence = previous;
            try { Directory.Delete(dir, recursive: true); } catch (IOException) { }
        });
    }

    sealed class Restorer : IDisposable
    {
        readonly Action _restore;
        public Restorer(Action restore) => _restore = restore;
        public void Dispose() => _restore();
    }
}
