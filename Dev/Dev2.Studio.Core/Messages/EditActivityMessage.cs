using System.Activities.Presentation.Model;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class EditActivityMessage:IMessage
    {
        public ModelItem ModelItem { get; set; }

        public EditActivityMessage(ModelItem modelItem)
        {
            ModelItem = modelItem;
        }
    }
}