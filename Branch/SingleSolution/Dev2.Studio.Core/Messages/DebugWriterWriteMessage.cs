using Dev2.Diagnostics;

namespace Dev2.Studio.Core.Messages
{
    public class DebugWriterWriteMessage : IMessage
    {
        public DebugWriterWriteMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugWriterWriteMessage(IDebugState debugState)
        {
            DebugState = debugState;
        }

        public IDebugState DebugState { get; set; }
    }
}