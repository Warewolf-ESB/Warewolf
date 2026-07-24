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
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Data.Interfaces.Enums;
using Dev2.Interfaces;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Persistence;
using Warewolf.Driver.Persistence.Drivers;
using Warewolf.Storage.Interfaces;
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Tests
{
    /// <summary>
    /// Pins the additive <see cref="IResumptionExecutor"/> host seam on both manual-resumption
    /// paths of <c>HangfireScheduler</c>. When the Azure Execution Engine registers a
    /// <see cref="IResumptionExecutor"/> into <c>CustomContainer</c>:
    /// <list type="bullet">
    ///   <item><c>ResumeJob</c> runs the continuation through the seam (not the in-process
    ///         <c>WorkflowResume</c>) and — on success — transitions the job to
    ///         <c>ManuallyResumed</c>; a seam-reported error fails the job and throws.</item>
    ///   <item><c>ManualResumeWithOverrideJob</c> invokes the override seam AFTER the job is
    ///         moved to <c>ManuallyResumed</c>.</item>
    /// </list>
    /// The Server registers nothing, so those paths keep their existing behaviour (covered by
    /// the sibling <c>HangfireSchedulerTests</c>).
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class HangfireSchedulerResumeSeamTests
    {
        [TestInitialize]
        public void Setup()
        {
            CustomContainer.DeRegister<IResumptionExecutor>();
            // Skip the real Windows performance-counter construction inside LoadAndRegisterTypes
            // (admin-only / sandbox-blocked) — behaviour under test is the resume seam, not counters.
            CustomContainer.Register<IWarewolfPerformanceCounterLocater>(new Mock<IWarewolfPerformanceCounterLocater>().Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            CustomContainer.DeRegister<IResumptionExecutor>();
            CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ResumeJob_WithRegisteredExecutor_UsesSeam_AndTransitionsToManuallyResumed()
        {
            var fake = new FakeResumptionExecutor(SerializeExecuteMessage(hasError: false, message: "Execution Completed."));
            CustomContainer.Register<IResumptionExecutor>(fake);

            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, storage, new Mock<IPersistedValues>().Object);
            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", Values());

            var result = scheduler.ResumeJob(NewDataObject(), jobId, overrideVariables: false, environment: "NewEnvironment");

            Assert.AreEqual(GlobalConstants.Success, result);
            Assert.IsTrue(fake.ExecuteCalled, "The engine's IResumptionExecutor.Execute must run the continuation, not the in-process WorkflowResume.");
            Assert.IsTrue(fake.ExecuteValues.ContainsKey("resourceID"), "The seam must receive the persisted suspension values.");
            Assert.AreEqual("ManuallyResumed", LastStateName(storage, jobId));
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ResumeJob_SeamReportsError_MarksJobFailed_AndThrows()
        {
            var fake = new FakeResumptionExecutor(SerializeExecuteMessage(hasError: true, message: "boom"));
            CustomContainer.Register<IResumptionExecutor>(fake);

            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, storage, new Mock<IPersistedValues>().Object);
            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", Values());

            var ex = Assert.ThrowsException<Exception>(
                () => scheduler.ResumeJob(NewDataObject(), jobId, overrideVariables: false, environment: "NewEnvironment"));

            StringAssert.Contains(ex.Message, "boom");
            Assert.IsTrue(fake.ExecuteCalled);
            Assert.AreEqual(FailedState.StateName, LastStateName(storage, jobId),
                "A seam-reported error must fail the job (fail-only) — no ManuallyResumed transition.");
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ManualResumeWithOverrideJob_WithRegisteredExecutor_InvokesOverrideSeam_AfterManuallyResumed()
        {
            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, storage, new Mock<IPersistedValues>().Object);
            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", Values());

            // Capture the job's state at the moment the seam fires — proves it runs AFTER the
            // ManuallyResumed transition the driver applies just before invoking the seam.
            string stateWhenSeamRan = null;
            var fake = new FakeResumptionExecutor(onOverride: () => stateWhenSeamRan = LastStateName(storage, jobId));
            CustomContainer.Register<IResumptionExecutor>(fake);

            var environment = new Mock<IExecutionEnvironment>();
            environment.Setup(o => o.HasErrors()).Returns(false);
            var dataObject = new Mock<IDSFDataObject>();
            dataObject.Setup(o => o.Environment).Returns(environment.Object);

            var result = scheduler.ManualResumeWithOverrideJob(dataObject.Object, jobId);

            Assert.AreEqual(GlobalConstants.Success, result);
            Assert.IsTrue(fake.OverrideCalled, "The override continuation must execute on the engine via the seam.");
            Assert.AreSame(dataObject.Object, fake.OverrideDataObject);
            Assert.IsTrue(fake.OverrideValues.ContainsKey("resourceID"));
            Assert.AreEqual("ManuallyResumed", stateWhenSeamRan,
                "The seam must fire AFTER the job is moved to ManuallyResumed.");
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ResumeJob_IsSynchronous_FinalisesStateOnlyAfterContinuationCompletes()
        {
            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var scheduler = new Persistence.Drivers.HangfireScheduler(client, storage, new Mock<IPersistedValues>().Object);
            var jobId = scheduler.ScheduleJob(enSuspendOption.SuspendForDays, "1", Values());

            // Capture the job's state AT THE MOMENT the continuation runs: it must not yet be
            // ManuallyResumed. Proves ResumeJob runs the continuation synchronously and only
            // then finalises the state — there is no early ack that returns before completion.
            string stateDuringExecute = null;
            var fake = new FakeResumptionExecutor(
                executeResult: SerializeExecuteMessage(hasError: false, message: "Execution Completed."),
                onExecute: () => stateDuringExecute = LastStateName(storage, jobId));
            CustomContainer.Register<IResumptionExecutor>(fake);

            var result = scheduler.ResumeJob(NewDataObject(), jobId, overrideVariables: false, environment: "NewEnvironment");

            Assert.AreEqual(GlobalConstants.Success, result);
            Assert.IsTrue(fake.ExecuteCalled);
            Assert.AreNotEqual("ManuallyResumed", stateDuringExecute,
                "The job must NOT be ManuallyResumed while the continuation is still running (no early ack).");
            Assert.AreEqual("ManuallyResumed", LastStateName(storage, jobId),
                "State is transitioned to ManuallyResumed only after the synchronous continuation completes, before ResumeJob returns.");
        }

        // ── helpers ─────────────────────────────────────────────────────────────────

        static Dictionary<string, StringBuilder> Values() => new()
        {
            { "resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab") },
            { "environment", new StringBuilder("{}") },
            { "startActivityId", new StringBuilder("4032a11e-4fb3-4208-af48-b92a0602ab4b") },
            { "versionNumber", new StringBuilder("1") },
            { "currentuserprincipal", new StringBuilder("alice") },
        };

        static IDSFDataObject NewDataObject() => new Mock<IDSFDataObject>().Object;

        static StringBuilder SerializeExecuteMessage(bool hasError, string message) =>
            new Dev2JsonSerializer().SerializeToBuilder(new ExecuteMessage
            {
                HasError = hasError,
                Message = new StringBuilder(message),
            });

        static string LastStateName(MemoryStorage storage, string jobId) =>
            storage.GetMonitoringApi().JobDetails(jobId)?.History?.OrderBy(s => s.CreatedAt).LastOrDefault()?.StateName;

        sealed class FakeResumptionExecutor : IResumptionExecutor
        {
            readonly StringBuilder _executeResult;
            readonly Action _onOverride;
            readonly Action _onExecute;

            public FakeResumptionExecutor(StringBuilder executeResult = null, Action onOverride = null, Action onExecute = null)
            {
                _executeResult = executeResult;
                _onOverride = onOverride;
                _onExecute = onExecute;
            }

            public bool ExecuteCalled { get; private set; }
            public Dictionary<string, StringBuilder> ExecuteValues { get; private set; }
            public bool OverrideCalled { get; private set; }
            public IDSFDataObject OverrideDataObject { get; private set; }
            public Dictionary<string, StringBuilder> OverrideValues { get; private set; }

            public StringBuilder Execute(Dictionary<string, StringBuilder> values)
            {
                ExecuteCalled = true;
                ExecuteValues = values;
                _onExecute?.Invoke();
                return _executeResult;
            }

            public void ExecuteOverrideContinuation(IDSFDataObject dsfDataObject, Dictionary<string, StringBuilder> values)
            {
                OverrideCalled = true;
                OverrideDataObject = dsfDataObject;
                OverrideValues = values;
                _onOverride?.Invoke();
            }
        }
    }
}
