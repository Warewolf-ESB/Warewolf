/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Regression coverage for the nested sub-workflow authorization gap: commit
 *  6283783e9e patched DsfActivity.AuthorizationService onto every node reachable
 *  via the flat top-level `next`-chain walked by WorkflowExecutor.ExecuteActivityChain,
 *  but a sub-workflow invoke ("DsfActivity") nested inside a container activity
 *  (Sequence, ForEach, Gate, etc.) is only reached later, internally, when that
 *  container's own Execute() iterates its children — never through the outer loop.
 *  ProfilerMain.json reproduces this: GetTrackingPeriod / GetCollectionMethod are
 *  invoked from inside a DsfSequenceActivity and were denied Execute permission
 *  because they kept the legacy ServerAuthorizationService.Instance default.
 *
 *  These tests exercise WorkflowExecutor.ExecuteActivityChain directly (internal,
 *  visible here via InternalsVisibleTo) with hand-built IDev2Activity fakes standing
 *  in for container nodes, and real DsfActivity instances standing in for the
 *  nested sub-workflow invokes, so we can assert on the internal
 *  DsfActivity.AuthorizationService property (also visible here via IVT) without
 *  needing a real compiled workflow XAML fixture.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.State;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Execution
{
    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowExecutorNestedAuthorizationTests
    {
        static IDSFDataObject NewDataObject() => new DsfDataObject(string.Empty, Guid.NewGuid(), string.Empty);

        static DsfActivity NewDsfActivity(string name) => new DsfActivity
        {
            UniqueID = Guid.NewGuid().ToString(),
            ServiceName = name
        };

        [TestMethod]
        public void ExecuteActivityChain_DsfActivityNestedInsideContainer_PatchesAuthorizationService()
        {
            // Repro of the ProfilerMain / GetTrackingPeriod bug: the DsfActivity is not
            // `startActivity` itself, it is only reachable via the container's
            // GetChildrenNodes(), exactly like a DsfSequenceActivity's Activities collection.
            var nested = NewDsfActivity("GetTrackingPeriod");
            var container = new FakeContainerActivity(children: new IDev2Activity[] { nested });

            WorkflowExecutor.ExecuteActivityChain(NewDataObject(), container);

            Assert.IsInstanceOfType(nested.AuthorizationService, typeof(LightweightAuthorizationService),
                "A DsfActivity nested one level inside a container's GetChildrenNodes() must be patched to the permissive Lightweight authorization service.");
        }

        [TestMethod]
        public void ExecuteActivityChain_DsfActivityNestedTwoLevelsDeep_PatchesAuthorizationService()
        {
            // Proves genuine recursion (e.g. a Sequence nested inside a Gate, or a
            // ForEach nested inside a Sequence), not just one extra hop.
            var nested = NewDsfActivity("GetCollectionMethod");
            var innerContainer = new FakeContainerActivity(children: new IDev2Activity[] { nested });
            var outerContainer = new FakeContainerActivity(children: new IDev2Activity[] { innerContainer });

            WorkflowExecutor.ExecuteActivityChain(NewDataObject(), outerContainer);

            Assert.IsInstanceOfType(nested.AuthorizationService, typeof(LightweightAuthorizationService),
                "A DsfActivity nested two levels deep must still be patched.");
        }

        [TestMethod]
        public void ExecuteActivityChain_TopLevelDsfActivity_StillPatchesAuthorizationService()
        {
            // Regression guard for the original 6283783e9e behaviour: a DsfActivity
            // reached directly via the flat top-level `next`-chain (not nested in any
            // container) must continue to be patched.
            var topLevel = NewDsfActivity("HelloWorld");

            WorkflowExecutor.ExecuteActivityChain(NewDataObject(), topLevel);

            Assert.IsInstanceOfType(topLevel.AuthorizationService, typeof(LightweightAuthorizationService),
                "Top-level chain nodes must still be patched (no regression of the existing 6283783e9e fix).");
        }

        [TestMethod]
        public void ExecuteActivityChain_CyclicChildrenNodes_DoesNotStackOverflowOrHang()
        {
            // Defensive test: some composite activity misbehaving (or a future
            // implementation change) could return itself, or a node already seen, from
            // GetChildrenNodes(). The visited-set guard must prevent infinite recursion.
            var cyclic = new FakeContainerActivity(children: Array.Empty<IDev2Activity>());
            cyclic.Children = new IDev2Activity[] { cyclic };

            var task = Task.Run(() => WorkflowExecutor.ExecuteActivityChain(NewDataObject(), cyclic));

            Assert.IsTrue(task.Wait(TimeSpan.FromSeconds(10)),
                "A self-referencing GetChildrenNodes() graph must not hang or stack-overflow the recursive authorization patch.");
        }

        [TestMethod]
        public void ExecuteActivityChain_NullChildrenNodes_DoesNotThrow()
        {
            // Defensive test: GetChildrenNodes() returning null (rather than an empty
            // collection) must not throw a NullReferenceException from the patch walk.
            var container = new FakeContainerActivity(children: null);

            WorkflowExecutor.ExecuteActivityChain(NewDataObject(), container);
            // No assertion beyond "did not throw" — reaching this line is the pass condition.
        }

        /// <summary>
        /// Minimal <see cref="IDev2Activity"/> test double standing in for a composite/container
        /// activity (e.g. DsfSequenceActivity). Only <see cref="UniqueID"/>, <see cref="Execute"/>
        /// and <see cref="GetChildrenNodes"/> are exercised by WorkflowExecutor.ExecuteActivityChain;
        /// every other member is unused by the code path under test.
        /// </summary>
        sealed class FakeContainerActivity : IDev2Activity
        {
            public FakeContainerActivity(IEnumerable<IDev2Activity> children)
            {
                UniqueID = Guid.NewGuid().ToString();
                Children = children;
            }

            public IEnumerable<IDev2Activity> Children { get; set; }

            public string UniqueID { get; set; }
            public IEnumerable<IDev2Activity> NextNodes { get; set; }
            public Guid ActivityId { get; set; }

            // Ends the chain: WorkflowExecutor.ExecuteActivityChain only needs to walk this
            // container's GetChildrenNodes() to find nested DsfActivity invokes; the container's
            // own real Execute() behaviour is out of scope for these authorization-patch tests.
            public IDev2Activity Execute(IDSFDataObject data, int update) => null;

            public IEnumerable<IDev2Activity> GetChildrenNodes() => Children;

            public enFindMissingType GetFindMissingType() => throw new NotImplementedException();
            public List<string> GetOutputs() => throw new NotImplementedException();
            public System.Activities.Statements.FlowNode GetFlowNode() => throw new NotImplementedException();
            public string GetDisplayName() => throw new NotImplementedException();
            public IEnumerable<StateVariable> GetState() => throw new NotImplementedException();
            public IEnumerable<IDev2Activity> GetNextNodes() => throw new NotImplementedException();
            public List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors() => throw new NotImplementedException();
            public T As<T>() where T : class, IDev2Activity => throw new NotImplementedException();
        }
    }
}
