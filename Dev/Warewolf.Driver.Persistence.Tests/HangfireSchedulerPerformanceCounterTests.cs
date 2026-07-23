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
using System.Reflection;
using System.Text;
using System.Transactions;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Drivers.HangfireScheduler.Test_Utils;
using Warewolf.Driver.Persistence;
using Warewolf.Driver.Persistence.Drivers;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Tests
{
    /// <summary>
    /// Pins the perf-counter laziness contract introduced for the Azure Execution Engine:
    /// Windows performance counters (admin-only category creation, blocked by the Azure
    /// App Service sandbox) must never be constructed by the ctor, and never at all when
    /// a host has pre-registered its own <see cref="IWarewolfPerformanceCounterLocater"/>.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class HangfireSchedulerPerformanceCounterTests
    {
        static readonly FieldInfo PerformanceCounterField =
            typeof(Persistence.Drivers.HangfireScheduler).GetField("_performanceCounter",
                BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(nameof(Persistence.Drivers.HangfireScheduler), "_performanceCounter");

        [TestInitialize]
        public void Setup() => CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();

        [TestCleanup]
        public void Cleanup() => CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_Ctor_DoesNotConstructPerformanceCounters()
        {
            var jobStorage = new MemoryStorage();
            var client = new BackgroundJobClient(jobStorage);
            var mockPersistedValues = new Mock<IPersistedValues>();

            var sut = new Persistence.Drivers.HangfireScheduler(client, jobStorage, mockPersistedValues.Object);

            Assert.IsNull(PerformanceCounterField.GetValue(sut),
                "The ctor must not build Windows performance counters — they require admin " +
                "rights to create categories and are blocked by the Azure App Service sandbox.");
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void HangfireScheduler_ResumeWorkflow_WithPreRegisteredLocater_DoesNotBuildRealCounters()
        {
            var preRegistered = new Mock<IWarewolfPerformanceCounterLocater>().Object;
            CustomContainer.Register(preRegistered);

            var jobStorage = new MemoryStorage();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();
            var mockPersistedValues = new Mock<IPersistedValues>();
            var mockWarewolfTransactionScopeFactory = new Mock<IWarewolfTransactionScopeFactory>();
            var mockTransactionScopeWrapper = new Mock<ITransactionScopeWrapper>();

            mockWarewolfTransactionScopeFactory.Setup(o => o.New(TransactionScopeAsyncFlowOption.Suppress))
                .Returns(mockTransactionScopeWrapper.Object);

            var sut = new Persistence.Drivers.HangfireScheduler(mockBackgroundJobClient.Object, jobStorage, mockPersistedValues.Object)
            {
                TransactionScopeFactory = mockWarewolfTransactionScopeFactory.Object,
                WorkflowResume = new SuccessWorkflowResume()
            };

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder("ab04663e-1e09-4338-8f61-a06a7ae5ebab")},
                {"environment", new StringBuilder("")},
                {"startActivityId", new StringBuilder(string.Empty)},
                {"versionNumber", new StringBuilder("1")},
                {"currentuserprincipal", new StringBuilder(new MockPrincipal().Name)}
            };

            var contextMock = new PerformContextMock("11", values);

            var result = sut.ResumeWorkflow(values, contextMock.Object);

            Assert.AreEqual(GlobalConstants.Success, result);
            Assert.AreSame(preRegistered, CustomContainer.Get<IWarewolfPerformanceCounterLocater>(),
                "LoadAndRegisterTypes must keep the pre-registered locater (the Azure engine's no-op) " +
                "instead of replacing it with the real counter manager.");
            Assert.IsNull(PerformanceCounterField.GetValue(sut),
                "With a pre-registered locater, the real Windows counters must never be built.");
        }

        class SuccessWorkflowResume : WorkflowResume
        {
            protected override ExecuteMessage ExecuteImpl(Dev2JsonSerializer serializer, Guid resourceId, Dictionary<string, StringBuilder> values)
            {
                return new ExecuteMessage { HasError = false, Message = new StringBuilder("ok") };
            }
        }
    }
}
