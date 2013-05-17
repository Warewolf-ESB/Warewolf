using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Messages
{
    public class ShowActivityWizardMessage:IMessage
    {
        public ModelItem ModelItem { get; set; }

        public ShowActivityWizardMessage(ModelItem modelItem)
        {
            ModelItem = modelItem;
        }
    }
}