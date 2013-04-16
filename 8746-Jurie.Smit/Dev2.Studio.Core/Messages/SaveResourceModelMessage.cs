using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class SaveResourceModelMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SaveResourceModelMessage(IContextualResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }

        public IContextualResourceModel ResourceModel { get; set; }
    }
}