/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    /// <summary>
    /// Exclusive ownership of a compiled workflow, which is what makes concurrent execution safe.
    ///
    /// <para><b>The defect these cover.</b> <c>WorkflowExecutor</c> used to cache ONE
    /// <c>DynamicActivity</c> per file path and hand it to every execution at once, on the stated
    /// reasoning that "all runtime state flows through DsfDataObject". That is false.
    /// <c>ActivityParser.Parse</c> clones nothing - it walks the cached <c>Flowchart</c> and
    /// returns references to the SAME <c>Dsf*Activity</c> objects - and those objects hold
    /// per-execution state in instance fields. Every database activity does
    /// <c>ServiceExecution = new DatabaseServiceExecution(dataObject)</c> in
    /// <c>BeforeExecutionStart</c> and reads it back in <c>ExecutionImpl</c>, so two concurrent
    /// executions overwrite each other and the loser runs against the winner's data object -
    /// leaving its own output variable unwritten.</para>
    ///
    /// <para>Measured live against the deployed engine on 2026-08-11, on a workflow whose first
    /// activity is a SQL stored-procedure call writing <c>[[JobLogId]]</c>:</para>
    /// <list type="bullet">
    ///   <item>sequential x10 - 10/10 HTTP 200;</item>
    ///   <item>concurrency 3 - 3/3 HTTP 200;</item>
    ///   <item>concurrency 4 / 6 / 10 - 3/4, 4/6, 8/10, the rest HTTP 500 with
    ///   "Object reference not set to an instance of an object." and
    ///   "Error with variables in input. [[JobLogId]]";</item>
    ///   <item>a SQL-free workflow - 20/20 at concurrency 20, because an Assign carries no such
    ///   instance state.</item>
    /// </list>
    ///
    /// <para>These tests assert on OBJECT IDENTITY rather than on a database result, so they need
    /// no SQL Server, no engine and no network: if two renters can never hold the same instance,
    /// the activity state they mutate cannot collide. That is the whole invariant.</para>
    /// </summary>
    [TestClass]
    public class WorkflowPoolConcurrencyTests
    {
        static string WorkflowPath =>
            Path.Combine(AppContext.BaseDirectory, "TestResources", "hangfiredemo", "Hello World.bite");

        static System.Text.StringBuilder Xaml()
        {
            var contents = WorkflowExecutor.ReadWorkflowFile(WorkflowPath);
            var (xaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(contents);
            Assert.IsNotNull(xaml, "test resource must contain a XamlDefinition");
            return xaml!;
        }

        [TestInitialize]
        public void Setup() => WorkflowExecutor.ClearWorkflowPool();

        [TestCleanup]
        public void Cleanup() => WorkflowExecutor.ClearWorkflowPool();

        // ── The exclusivity invariant ────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_TwiceWithoutReturning_YieldsTwoDistinctInstances()
        {
            // THE REGRESSION GUARD. The old GetOrLoadDynamicActivity returned the SAME cached
            // instance here, which is precisely how two concurrent executions came to share one
            // activity object.
            var first = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            var second = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.IsNotNull(first);
            Assert.IsNotNull(second);
            Assert.AreNotSame(first!.Activity, second!.Activity,
                "two simultaneous renters must never hold the same DynamicActivity");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_TwiceWithoutReturning_YieldsDistinctActivityChains()
        {
            // The chain is what actually gets executed, and its nodes are the objects carrying the
            // per-execution ServiceExecution field. Distinct compiled workflows are only useful if
            // their parsed chains are distinct too.
            var first = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            var second = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.IsNotNull(first!.StartActivity, "a valid workflow must parse to a start node");
            Assert.IsNotNull(second!.StartActivity);
            Assert.AreNotSame(first.StartActivity, second.StartActivity,
                "concurrent executions must not run the same IDev2Activity instances");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Rent_UnderConcurrency_NeverHandsTheSameInstanceToTwoCallers()
        {
            // Mirrors the shape that broke in production: many callers wanting one workflow at the
            // same moment. Concurrency 8 is above the 4 at which live failures began.
            const int callers = 8;
            var rented = new ConcurrentBag<WorkflowExecutor.PreparedWorkflow>();
            var barrier = new System.Threading.Barrier(callers);

            await Task.WhenAll(Enumerable.Range(0, callers).Select(_ => Task.Run(() =>
            {
                barrier.SignalAndWait();                       // maximise real overlap
                var p = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
                if (p is not null) rented.Add(p);
            })));

            Assert.AreEqual(callers, rented.Count, "every caller must receive a workflow");

            var distinctActivities = rented.Select(r => (object)r.Activity).Distinct(ReferenceComparer.Instance).Count();
            Assert.AreEqual(callers, distinctActivities,
                "each concurrent renter must hold its own DynamicActivity");

            var chains = rented.Where(r => r.StartActivity is not null)
                               .Select(r => (object)r.StartActivity!)
                               .ToList();
            Assert.AreEqual(chains.Count, chains.Distinct(ReferenceComparer.Instance).Count(),
                "each concurrent renter must hold its own activity chain");
        }

        // ── The reuse property that keeps this affordable ────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_AfterReturn_ReusesTheInstanceRatherThanRecompiling()
        {
            // Without this the fix would trade a correctness bug for a performance one:
            // ActivityXamlServices.Load compiles the whole XAML every time.
            var first = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, first);

            var second = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.AreSame(first!.Activity, second!.Activity,
                "a returned workflow must be reused by the next renter");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Pool_GrowsOnlyToPeakConcurrency()
        {
            // Two overlapping renters produce two instances; returning both leaves exactly two
            // idle - not one per execution ever performed.
            var a = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            var b = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, a);
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, b);

            Assert.AreEqual(2, WorkflowExecutor.PooledWorkflowCount(WorkflowPath));

            // A third sequential rent/return cycle must not grow the pool any further.
            var c = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, c);
            Assert.AreEqual(2, WorkflowExecutor.PooledWorkflowCount(WorkflowPath));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Pool_KeepsWorkflowsSeparatedByPath()
        {
            var other = Path.Combine(AppContext.BaseDirectory, "TestResources", "hangfiredemo",
                                     "Suspend Execution Example.bite");
            var otherContents = WorkflowExecutor.ReadWorkflowFile(other);
            var (otherXaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(otherContents);

            var a = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            var b = WorkflowExecutor.RentPreparedWorkflow(other, otherXaml!);
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, a);
            WorkflowExecutor.ReturnPreparedWorkflow(other, b);

            Assert.AreEqual(1, WorkflowExecutor.PooledWorkflowCount(WorkflowPath));
            Assert.AreEqual(1, WorkflowExecutor.PooledWorkflowCount(other));
        }

        // ── Robustness of the rent/return contract ───────────────────────────

        // ── The memory bound ─────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Pool_RetainsNoMoreThanTheCap()
        {
            // Without a cap the pool grows to peak concurrency and NEVER shrinks, so one burst
            // permanently raises the memory floor. A compiled workflow tree measured ~32 MB (engine
            // working set 396 MB -> 712 MB across 10 concurrent executions, 2026-08-12), and on a
            // ~1.5 GB Consumption instance that reached exhaustion: the XAML compile itself failed
            // with "Insufficient memory to continue the execution of the program", surfacing as 500.
            var cap = WorkflowExecutor.MaxPooledPerWorkflow;
            var rented = new System.Collections.Generic.List<WorkflowExecutor.PreparedWorkflow>();
            for (var i = 0; i < cap + 4; i++)
            {
                rented.Add(WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml())!);
            }
            foreach (var p in rented)
            {
                WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, p);
            }

            Assert.AreEqual(cap, WorkflowExecutor.PooledWorkflowCount(WorkflowPath),
                $"returning {cap + 4} instances must retain exactly {cap}; the excess is released");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_BeyondTheCap_StillSucceedsAndStaysExclusive()
        {
            // The cap bounds what is KEPT, never what can be rented. Throttling rents would turn a
            // memory guard into a concurrency limit and could deadlock the caller.
            var cap = WorkflowExecutor.MaxPooledPerWorkflow;
            var rented = new System.Collections.Generic.List<WorkflowExecutor.PreparedWorkflow>();
            for (var i = 0; i < cap + 3; i++)
            {
                var p = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
                Assert.IsNotNull(p, $"rent #{i + 1} must succeed even beyond the cap of {cap}");
                rented.Add(p!);
            }

            var distinct = rented.Select(r => (object)r.Activity).Distinct(ReferenceComparer.Instance).Count();
            Assert.AreEqual(rented.Count, distinct,
                "every concurrent renter must still hold its own instance, cap or no cap");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void PoolCap_IsAtLeastOne_SoReuseIsNeverDisabledEntirely()
        {
            Assert.IsTrue(WorkflowExecutor.MaxPooledPerWorkflow >= 1,
                "a cap below 1 would recompile the XAML on every single execution");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Return_IsNullTolerant_SoCallersCanReturnFromAFinallyUnconditionally()
        {
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, null);
            WorkflowExecutor.ReturnPreparedWorkflow(null!, null);
            Assert.AreEqual(0, WorkflowExecutor.PooledWorkflowCount(WorkflowPath),
                "returning nothing must not add a phantom entry");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_NormalisesThePathSoCasingAndSeparatorsShareOnePool()
        {
            var a = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath.ToUpperInvariant(), a);

            Assert.AreEqual(1, WorkflowExecutor.PooledWorkflowCount(WorkflowPath),
                "a differently-cased path must not create a second pool");
        }

        sealed class ReferenceComparer : System.Collections.Generic.IEqualityComparer<object>
        {
            internal static readonly ReferenceComparer Instance = new();
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
        // ── Staleness: a changed .bite must never be served from the pool ────
        //
        // The defect: the pool key was the file path ALONE, and RentPreparedWorkflow returns a
        // pooled instance BEFORE it looks at the freshly-read xamlDefinition. So once a workflow
        // had executed, edit_workflow could rewrite its .bite and every later execution still
        // replayed the pre-edit compilation until the process restarted - the tool reported
        // success, the file genuinely changed, and behaviour did not. Reproduced against
        // warewolfserver-mcp on 2026-08-21: get_workflow_definition showed the edited description
        // while execute_workflow kept returning the old output.

        static string CopyToTempBite()
        {
            var temp = Path.Combine(Path.GetTempPath(), "wwpool-" + Guid.NewGuid().ToString("N") + ".bite");
            File.Copy(WorkflowPath, temp);
            return temp;
        }

        static System.Text.StringBuilder XamlOf(string path)
        {
            var contents = WorkflowExecutor.ReadWorkflowFile(path);
            var (xaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(contents);
            Assert.IsNotNull(xaml, "test resource must contain a XamlDefinition");
            return xaml!;
        }

        /// <summary>
        /// Appends trailing whitespace after the root element - legal XML that leaves the workflow
        /// semantically identical (so it still parses) while changing the file's length and
        /// last-write time, which is what the pool key discriminates on.
        /// </summary>
        static void MutateBite(string path) => File.AppendAllText(path, "\n");

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_AfterTheFileChanges_DoesNotServeTheStalePooledInstance()
        {
            var temp = CopyToTempBite();
            try
            {
                var first = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                Assert.IsNotNull(first);
                WorkflowExecutor.ReturnPreparedWorkflow(temp, first);

                MutateBite(temp);

                var second = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));

                Assert.IsNotNull(second);
                Assert.AreNotSame(first, second,
                    "a pooled instance compiled from the PRE-edit file was served after the file changed");
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_WhenFileIsUnchanged_StillReusesThePooledInstance()
        {
            // The other half of the contract: keying on file identity must not defeat pooling for
            // the overwhelmingly common case where nothing changed between executions.
            var temp = CopyToTempBite();
            try
            {
                var first = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                WorkflowExecutor.ReturnPreparedWorkflow(temp, first);

                var second = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));

                Assert.AreSame(first, second, "an unchanged workflow should still be pooled and reused");
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Return_AfterTheFileChangedMidExecution_DoesNotFileTheStaleInstanceUnderTheNewKey()
        {
            // Return must use the key STAMPED at rent time, not recompute it from the file. If it
            // recomputed, an instance compiled from the old definition would be filed under the new
            // definition's key and handed to the very next execution - reintroducing the bug by a
            // different route.
            var temp = CopyToTempBite();
            try
            {
                var rented = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                Assert.IsNotNull(rented);

                MutateBite(temp);                                   // changes while "executing"
                WorkflowExecutor.ReturnPreparedWorkflow(temp, rented);

                var next = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));

                Assert.AreNotSame(rented, next,
                    "the instance compiled from the pre-change file was filed under the changed file's key");
            }
            finally
            {
                File.Delete(temp);
            }
        }

        // ── Explicit eviction (what the MCP write tools call) ────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void EvictWorkflow_DropsPooledInstancesForThatPath()
        {
            var temp = CopyToTempBite();
            try
            {
                var first = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                WorkflowExecutor.ReturnPreparedWorkflow(temp, first);

                WorkflowExecutor.EvictWorkflow(temp);

                var second = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));

                Assert.AreNotSame(first, second, "eviction should force the next rent to recompile");
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void EvictWorkflow_LeavesOtherWorkflowsPooled()
        {
            var a = CopyToTempBite();
            var b = CopyToTempBite();
            try
            {
                var pooledB = WorkflowExecutor.RentPreparedWorkflow(b, XamlOf(b));
                WorkflowExecutor.ReturnPreparedWorkflow(b, pooledB);

                WorkflowExecutor.EvictWorkflow(a);

                var againB = WorkflowExecutor.RentPreparedWorkflow(b, XamlOf(b));
                Assert.AreSame(pooledB, againB, "evicting one workflow must not clear another");
            }
            finally
            {
                File.Delete(a);
                File.Delete(b);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void EvictWorkflow_IsSafeForUnknownNullAndEmptyPaths()
        {
            WorkflowExecutor.EvictWorkflow(Path.Combine(Path.GetTempPath(), "never-pooled.bite"));
            WorkflowExecutor.EvictWorkflow(null!);
            WorkflowExecutor.EvictWorkflow(string.Empty);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Rent_AfterFileChange_StillYieldsExclusiveInstances()
        {
            // The staleness fix must not weaken the invariant this whole class exists to protect:
            // two renters never hold the same instance, regardless of which key bucket they hit.
            var temp = CopyToTempBite();
            try
            {
                var first = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                MutateBite(temp);
                var second = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));
                var third = WorkflowExecutor.RentPreparedWorkflow(temp, XamlOf(temp));

                Assert.AreNotSame(first, second);
                Assert.AreNotSame(second, third);
                Assert.AreNotSame(first, third);
            }
            finally
            {
                File.Delete(temp);
            }
        }
    }
}
