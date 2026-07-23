/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Shared helpers for the Phase-5 resume tests (ResumptionExecutor + the
 *  WorkflowResume route). Seeds real Hangfire jobs into an in-memory store and
 *  swaps the process-global Config.Persistence deterministically.
 */

using Dev2.Common;
using Dev2.Common.Wrappers;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    /// <summary>
    /// A public, Hangfire-serialisable job method used only to seed jobs. Carries
    /// <c>[AutomaticRetry(Attempts = 0)]</c> to mirror the real suspend job
    /// (<c>HangfireScheduler.ResumeWorkflow</c>): without it, Hangfire's global
    /// AutomaticRetry filter would reschedule a Failed job back to Scheduled, masking the
    /// engine's fail-only recording.
    /// </summary>
    public static class ResumeSeed
    {
        [AutomaticRetry(Attempts = 0)]
        public static void Continuation(Dictionary<string, StringBuilder> values) { }
    }

    internal static class ResumeTestSupport
    {
        /// <summary>The five legacy suspend keys — parity with the Server-created job shape.</summary>
        public static Dictionary<string, StringBuilder> LegacyValues(
            string resourceId = "ab04663e-1e09-4338-8f61-a06a7ae5ebab",
            string startActivityId = "4032a11e-4fb3-4208-af48-b92a0602ab4b",
            string environment = "{}",
            string versionNumber = "1",
            string principal = "alice") => new()
        {
            { "resourceID", new StringBuilder(resourceId) },
            { "environment", new StringBuilder(environment) },
            { "startActivityId", new StringBuilder(startActivityId) },
            { "versionNumber", new StringBuilder(versionNumber) },
            { "currentuserprincipal", new StringBuilder(principal) },
        };

        /// <summary>Creates a job in the <c>Scheduled</c> state (the only state the resume route claims).</summary>
        public static string SeedScheduledJob(MemoryStorage storage, Dictionary<string, StringBuilder> values)
        {
            var client = new BackgroundJobClient(storage);
            return client.Create(() => ResumeSeed.Continuation(values), new ScheduledState(DateTime.UtcNow.AddDays(1)));
        }

        /// <summary>
        /// Creates a Scheduled job then transitions it into <paramref name="state"/> — used to
        /// exercise the "not claimable unless Scheduled" guard for terminal/in-flight states
        /// (Failed / Succeeded / ManuallyResumed / Processing).
        /// </summary>
        public static string SeedJobInState(MemoryStorage storage, Dictionary<string, StringBuilder> values, IState state)
        {
            var client = new BackgroundJobClient(storage);
            var jobId = client.Create(() => ResumeSeed.Continuation(values), new ScheduledState(DateTime.UtcNow.AddDays(1)));
            client.ChangeState(jobId, state);
            return jobId;
        }

        /// <summary>Last recorded state name for a job — read exactly as the driver's state guards do.</summary>
        public static string? LastStateName(MemoryStorage storage, string jobId)
        {
            var details = storage.GetMonitoringApi().JobDetails(jobId);
            return details?.History?.OrderBy(s => s.CreatedAt).LastOrDefault()?.StateName;
        }

        /// <summary>
        /// Swaps <see cref="Config.Persistence"/> for a temp-file-backed instance with the
        /// requested <c>Enable</c> flag (dev boxes may own a real machine settings file).
        /// Dispose restores the previous instance. Callers must be <c>[DoNotParallelize]</c>.
        /// </summary>
        public static IDisposable SwapPersistence(bool enable)
        {
            var previous = Config.Persistence;

            var dir = Directory.CreateTempSubdirectory("wwresume-").FullName;
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

    /// <summary>No-op <see cref="IExecutionLogger"/> — the resume tests assert on state, not logs.</summary>
    internal sealed class NoOpExecutionLogger : IExecutionLogger
    {
        public void LogTrace(string message, Guid executionId) { }
        public void LogTrace(string message, Exception exception, Guid executionId) { }
        public void LogDebug(string message, Guid executionId) { }
        public void LogDebug(string message, Exception exception, Guid executionId) { }
        public void LogError(Exception ex, string log) { }
        public void LogInfo(string message, Guid executionId) { }
        public void LogInfo(string message, Exception exception, Guid executionId) { }
        public void LogInfo(string message) { }
        public void LogWarning(string message, Guid executionId) { }
        public void LogWarning(string message, Exception exception, Guid executionId) { }
        public void LogError(string message, Guid executionId) { }
        public void LogError(string activityName, Exception ex, Guid executionId) { }
        public void LogFatal(string message, Guid executionId) { }
        public void LogFatal(string message, Exception exception, Guid executionId) { }
    }
}
