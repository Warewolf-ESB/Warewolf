
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddMissingAndFindUnusedDataListItemsMessage : IMessage
    {
        public IResourceModel CurrentResourceModel { get; set; }

        public AddMissingAndFindUnusedDataListItemsMessage(IResourceModel currentResourceModel)
        {
            CurrentResourceModel = currentResourceModel;
        }
    }
}
