/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
using System;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// A <see cref="PerRequestDebugCapturer"/>-shaped <see cref="IDebugDispatcher"/> for
    /// <c>execute_test</c> runs, additionally forwarding every captured state into
    /// <see cref="TestDebugMessageRepo"/>.
    ///
    /// <para>
    /// <b>Why this forwarding is required.</b> Container activities (<c>Sequence</c>/<c>ForEach</c>/
    /// <c>SelectAndApply</c>/<c>SuspendExecution</c>) assert their <b>children</b>'s steps via
    /// <c>Dev2.Activities.ServiceTestHelper.UpdateDebugStateWithAssertions</c> — shared code, called
    /// automatically from inside those activities' own <c>Execute()</c>, not something
    /// <see cref="WorkflowExecutor"/> can skip or intercept. That method reads the child's captured
    /// debug state back out of <see cref="TestDebugMessageRepo.Instance"/>
    /// (<c>ServiceTestHelper.cs:163</c>), keyed by <c>(dataObject.ResourceID, dataObject.TestName)</c>
    /// — exactly what the default <see cref="Dev2.Diagnostics.Debug.DebugDispatcherImplementation"/>
    /// populates when <c>WriteArgs.isTestExecution</c> is true
    /// (<c>DebugDispatcher.cs:97-99</c>). Lightweight installs a per-request
    /// <c>AsyncLocal</c> dispatcher override for every execution (debug or test) that pre-empts the
    /// default dispatcher entirely — so without this forwarding, <see cref="TestDebugMessageRepo"/>
    /// would stay permanently empty under Lightweight and every container-child <c>Assert</c> step
    /// would come back <c>TestInvalid</c> rather than evaluated.
    /// </para>
    ///
    /// <para>
    /// <b>Known, inherited concurrency scope.</b> <see cref="TestDebugMessageRepo"/> is a
    /// process-wide static dictionary keyed only by <c>(resourceId, testName)</c> — the exact same
    /// structure the full Warewolf server itself uses for this feature, so two concurrent
    /// <c>execute_test</c> runs of the SAME workflow's SAME test name could observe each other's
    /// captured states for this one narrow path (container-child asserts only — every other
    /// assertion path reads straight from the per-execution <c>DsfDataObject.Environment</c> and is
    /// unaffected). This is a pre-existing limitation of shared <c>Dev2.Activities</c> code, not a
    /// regression Lightweight introduces; <see cref="WorkflowExecutor.ExecuteTest"/> narrows the
    /// exposure window by removing this run's entry immediately after use
    /// (<see cref="TestDebugMessageRepo.FetchDebugItems"/>) rather than leaving it to leak
    /// indefinitely.
    /// </para>
    /// </summary>
    internal sealed class TestDebugCapturer : IDebugDispatcher
    {
        readonly List<IDebugState> _states = new();

        /// <summary>All debug states captured during this test run.</summary>
        public IReadOnlyList<IDebugState> States => _states;

        public void Write(WriteArgs writeArgs)
        {
            if (writeArgs.debugState is not { } state)
            {
                return;
            }

            _states.Add(state);

            if (writeArgs.isTestExecution)
            {
                TestDebugMessageRepo.Instance.AddDebugItem(state.SourceResourceID, writeArgs.testName, state);
            }
        }

        // ---- no-op IDebugDispatcher members not needed for capture ----
        public int Count => 0;
        public void Add(Guid workspaceId, IDebugWriter writer) { }
        public IDebugWriter Get(Guid workspaceId) => null;
        public void Remove(Guid workspaceId) { }
        public void Shutdown() { }
    }
}
