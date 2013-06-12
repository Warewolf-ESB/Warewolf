using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class DebugResourceMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public DebugResourceMessage(IContextualResourceModel resource)
        {
            Resource = resource;
        }

        public IContextualResourceModel Resource { get; set; }
    }
}