using Dev2.Studio.Core.Interfaces;

namespace Dev2.Messages
{
    public class SaveUnsavedWorkflowMessage
    {
        public IContextualResourceModel ResourceModel { get; set; }
        public string ResourceName { get; set; }
        public string ResourceCategory { get; set; }
        public bool KeepTabOpen { get; set; }

        public SaveUnsavedWorkflowMessage(IContextualResourceModel resourceModel, string resourceName, string resourceCategory, bool keepTabOpen)
        {
            ResourceModel = resourceModel;
            ResourceName = resourceName;
            ResourceCategory = resourceCategory;
            KeepTabOpen = keepTabOpen;
        }
    }
}