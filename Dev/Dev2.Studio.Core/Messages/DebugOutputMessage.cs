using System.Collections.Generic;
using Dev2.Diagnostics.Debug;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Messages
// ReSharper restore CheckNamespace
{
    public class DebugOutputMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugOutputMessage(IList<IDebugState> debugStates)
        {
            DebugStates = debugStates;
        }

        public IList<IDebugState> DebugStates { get; set; }
    }
}