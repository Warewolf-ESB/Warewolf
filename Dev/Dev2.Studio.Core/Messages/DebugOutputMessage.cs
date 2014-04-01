
// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using Dev2.Diagnostics;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
// ReSharper restore CheckNamespace
{
    public class DebugOutputMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugOutputMessage(IList<DebugState> debugStates)
        {
            DebugStates = debugStates;
        }

        public IList<DebugState> DebugStates { get; set; }
    }
}   