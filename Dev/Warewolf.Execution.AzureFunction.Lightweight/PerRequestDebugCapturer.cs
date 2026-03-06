using Dev2.Common.Interfaces.Diagnostics.Debug;
using System;
using System.Collections.Generic;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Per-request <see cref="IDebugDispatcher"/> that captures <see cref="IDebugState"/> objects
    /// directly into a local list — no global singleton, no dictionary, no locking.
    /// <para>
    /// Usage:
    /// <code>
    ///   var capturer = new PerRequestDebugCapturer();
    ///   using (DebugDispatcher.UseContextDispatcher(capturer))
    ///       ExecuteActivityChain(...);
    ///   // capturer.States contains the collected debug states
    /// </code>
    /// </para>
    /// </summary>
    internal sealed class PerRequestDebugCapturer : IDebugDispatcher
    {
        readonly List<IDebugState> _states = new();

        /// <summary>All debug states captured during execution.</summary>
        public IReadOnlyList<IDebugState> States => _states;

        public void Write(WriteArgs writeArgs)
        {
            if (writeArgs.debugState is { } state)
                _states.Add(state);
        }

        // ---- no-op IDebugDispatcher members not needed for capture ----
        public int Count => 0;
        public void Add(Guid workspaceId, IDebugWriter writer) { }
        public IDebugWriter Get(Guid workspaceId) => null;
        public void Remove(Guid workspaceId) { }
        public void Shutdown() { }
    }
}
