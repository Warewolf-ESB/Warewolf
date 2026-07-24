/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;
using Dev2.Data.Interfaces.Enums;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Persistence;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Tests
{
    /// <summary>
    /// Pins the additive <see cref="IJobValuesEnricher"/> host seam on
    /// <c>HangfireScheduler.ScheduleJob</c>: a registered enricher's keys land in the
    /// persisted job args; without one the job shape is byte-identical to before the
    /// seam existed (Server behaviour). Jobs are read back through the same
    /// <c>MonitoringApi.JobDetails</c> path the resume side uses.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class HangfireSchedulerEnricherTests
    {
        static readonly string[] LegacyKeys =
            { "resourceID", "environment", "startActivityId", "versionNumber", "currentuserprincipal" };

        [TestInitialize]
        public void Setup() => CustomContainer.DeRegister<IJobValuesEnricher>();

        [TestCleanup]
        public void Cleanup() => CustomContainer.DeRegister<IJobValuesEnricher>();

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ScheduleJob_WithRegisteredEnricher_PersistsAdditiveKeys()
        {
            CustomContainer.Register<IJobValuesEnricher>(new StampingEnricher());

            var jobStorage = new MemoryStorage();
            var scheduler = NewScheduler(jobStorage);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", NewLegacyValues());

            var persisted = ReadPersistedValues(jobStorage, jobId);
            Assert.AreEqual("stamped", persisted[StampingEnricher.Key].ToString());
            foreach (var legacyKey in LegacyKeys)
            {
                Assert.IsTrue(persisted.ContainsKey(legacyKey), $"Legacy key '{legacyKey}' must survive enrichment.");
            }
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ScheduleJob_WithoutEnricher_JobShapeUnchanged()
        {
            var jobStorage = new MemoryStorage();
            var scheduler = NewScheduler(jobStorage);

            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", NewLegacyValues());

            var persisted = ReadPersistedValues(jobStorage, jobId);
            CollectionAssert.AreEquivalent(LegacyKeys, persisted.Keys.ToArray(),
                "Without a registered enricher (the Server case) the persisted job must carry exactly the five legacy keys.");
        }

        static Persistence.Drivers.HangfireScheduler NewScheduler(MemoryStorage jobStorage) =>
            new(new BackgroundJobClient(jobStorage), jobStorage, new Mock<IPersistedValues>().Object);

        static Dictionary<string, StringBuilder> NewLegacyValues() => new()
        {
            {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
            {"environment", new StringBuilder("{}")},
            {"startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b")},
            {"versionNumber", new StringBuilder("1")},
            {"currentuserprincipal", new StringBuilder("alice")},
        };

        static Dictionary<string, StringBuilder> ReadPersistedValues(MemoryStorage jobStorage, string jobId)
        {
            Assert.IsNotNull(jobId);
            var jobDetails = jobStorage.GetMonitoringApi().JobDetails(jobId);
            var values = jobDetails.Job.Args[0] as Dictionary<string, StringBuilder>;
            Assert.IsNotNull(values, "Job args must round-trip as the values dictionary.");
            return values;
        }

        sealed class StampingEnricher : IJobValuesEnricher
        {
            public const string Key = "engineTestStamp";
            public void Enrich(Dictionary<string, StringBuilder> values) => values[Key] = new StringBuilder("stamped");
        }
    }
}
