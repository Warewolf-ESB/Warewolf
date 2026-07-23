/*
 * Tests for NoOpPerformanceCounterLocater and its idempotent startup registration.
 *
 * Windows performance counters require admin rights to create categories and are
 * blocked by the Azure App Service sandbox — the engine therefore registers this
 * no-op locater at cold start so HangfireScheduler.LoadAndRegisterTypes never
 * constructs the real counter machinery.
 */

using Dev2;
using Dev2.Common.Interfaces.Monitoring;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Warewolf.Execution.Lightweight.Infrastructure;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    [DoNotParallelize] // mutates process-global CustomContainer registrations
    public class NoOpPerformanceCounterLocaterTests
    {
        [TestInitialize]
        public void Setup() => CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();

        [TestCleanup]
        public void Cleanup() => CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetCounter_AllOverloads_ReturnUsableNoOpCounters()
        {
            var sut = new NoOpPerformanceCounterLocater();

            var byName = sut.GetCounter("anything");
            var byType = sut.GetCounter(WarewolfPerfCounterType.ExecutionErrors);
            var byResource = sut.GetCounter(Guid.NewGuid(), WarewolfPerfCounterType.AverageExecutionTime);

            foreach (var counter in new[] { byName, byType, byResource })
            {
                Assert.IsNotNull(counter);
                // Every mutating member must be a harmless no-op — no sandbox-blocked
                // Windows perf-counter API may be touched.
                counter.Increment();
                counter.IncrementBy(42);
                counter.Decrement();
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RegisterNoOpPerformanceCounters_RegistersOnce_AndKeepsExistingLocater()
        {
            var register = typeof(StartupOrchestrator).GetMethod(
                                "RegisterNoOpPerformanceCounters",
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?? throw new MissingMethodException(nameof(StartupOrchestrator), "RegisterNoOpPerformanceCounters");

            // First call on an empty container registers the no-op locater.
            register.Invoke(null, null);
            var registered = CustomContainer.Get<IWarewolfPerformanceCounterLocater>();
            Assert.IsInstanceOfType(registered, typeof(NoOpPerformanceCounterLocater));

            // Second call is idempotent — the registered instance is kept.
            register.Invoke(null, null);
            Assert.AreSame(registered, CustomContainer.Get<IWarewolfPerformanceCounterLocater>());

            // A pre-registered (host-supplied) locater is never replaced.
            CustomContainer.DeRegister<IWarewolfPerformanceCounterLocater>();
            IWarewolfPerformanceCounterLocater preRegistered = new StubLocater();
            CustomContainer.Register(preRegistered);
            register.Invoke(null, null);
            Assert.AreSame(preRegistered, CustomContainer.Get<IWarewolfPerformanceCounterLocater>());
        }

        sealed class StubLocater : IWarewolfPerformanceCounterLocater
        {
            public IPerformanceCounter GetCounter(string name) => null!;
            public IPerformanceCounter GetCounter(WarewolfPerfCounterType type) => null!;
            public IPerformanceCounter GetCounter(Guid resourceId, WarewolfPerfCounterType type) => null!;
        }
    }
}
