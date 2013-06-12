using System.Activities.Presentation.Model;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class ShowActivitySettingsWizardMessage:IMessage
    {
        public ModelItem ModelItem { get; set; }

        public ShowActivitySettingsWizardMessage(ModelItem modelItem)
        {
            ModelItem = modelItem;
        }
    }
}