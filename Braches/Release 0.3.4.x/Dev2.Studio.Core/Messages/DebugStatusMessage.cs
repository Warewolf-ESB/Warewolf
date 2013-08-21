
namespace Dev2.Studio.Core.Messages
{
    public class DebugStatusMessage : IMessage
    {
        public DebugStatusMessage(bool debugStatus)
        {
            DebugStatus = debugStatus;
        }

        public bool DebugStatus { get; set; }
    }
}
