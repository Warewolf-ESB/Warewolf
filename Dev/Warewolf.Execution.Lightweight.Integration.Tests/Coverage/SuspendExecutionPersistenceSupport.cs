/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Swaps the process-global Config.Persistence for the duration of the "Suspend Execution"
 *  fidelity row so SuspendExecutionActivity's real Hangfire scheduling path (its parameterless
 *  ctor -> PersistenceExecution() -> HangfireScheduler() -> new SqlServerStorage(ConnectionString),
 *  see SuspendExecutionActivity.cs:52-66 / HangfireScheduler.cs:71-79,520-534) has somewhere real
 *  to connect. Without this, both the original and round-tripped executions fail identically on
 *  "Hangfire SqlServerStorage could not connect" before ever exercising the activity's own
 *  round-trip logic, which is why that row can only ever report PassBothFailedIdentically instead
 *  of a genuine Pass (see FidelityAllowList.cs's remarks: only Status == "Pass" counts).
 *
 *  Points at the same Dev2TestingDB instance FidelityFixtureGenerator.WriteMssqlSource already
 *  uses for the "SQL Server Database" row (TestRun.ps1's Start-HostMSSQLServer provisions
 *  testUser as a sysadmin login there), so Hangfire.SqlServer can create its own [HangFire]
 *  schema on first connect (SqlServerStorageOptions.PrepareSchemaIfNecessary defaults to true).
 *  No live Hangfire worker is required: HangfireScheduler.ScheduleJob only needs to persist a row
 *  via IBackgroundJobClient.Create, it never needs the job to actually run.
 */

using System;
using System.IO;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Serializers;
using Dev2.Common.Wrappers;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Data;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    internal static class SuspendExecutionPersistenceSupport
    {
        /// <summary>
        /// Swaps <see cref="Config.Persistence"/> for a temp-file-backed instance pointing
        /// Hangfire at <see cref="FidelityFixtureGenerator.MssqlConnectionString"/>, with
        /// <c>TrustServerCertificate</c> set (the test SQL Server presents a self-signed
        /// certificate). Dispose restores the previous instance.
        /// </summary>
        internal static IDisposable SwapToRealHangfireSqlServer()
        {
            var previous = Config.Persistence;

            var dbSource = new DbSource { ServerType = enSourceType.SqlDatabase };
            dbSource.ConnectionString = FidelityFixtureGenerator.MssqlConnectionString;
            dbSource.TrustServerCertificate = true;

            var dir = Directory.CreateTempSubdirectory("wf-fidelity-suspend-").FullName;
            var path = Path.Combine(dir, "persistencesettings.json");
            Config.Persistence = new PersistenceSettings(path, new FileWrapper(), new DirectoryWrapper())
            {
                Enable = true,
                PersistenceScheduler = nameof(Hangfire),
                EncryptDataSource = false,
                PersistenceDataSource = new NamedGuidWithEncryptedPayload
                {
                    Name = "Fidelity Hangfire Persistence",
                    Value = Guid.NewGuid(),
                    Payload = new Dev2JsonSerializer().Serialize(dbSource),
                },
            };

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
}
