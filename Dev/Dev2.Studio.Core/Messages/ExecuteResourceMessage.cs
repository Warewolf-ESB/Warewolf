using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class ExecuteResourceMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ExecuteResourceMessage(IContextualResourceModel resource)
        {
            Resource = resource;
        }

        public IContextualResourceModel Resource { get; set; }
    }
}