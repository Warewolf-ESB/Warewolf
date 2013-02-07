using System.Activities.Presentation.Model;

namespace Dev2.Studio.Core.Messages
{
    public class ShowQuickVariableInputMessage
    {

        public ShowQuickVariableInputMessage(ModelItem modelItem)
        {
            ModelItem = modelItem;
        }

        public ModelItem ModelItem { get; set; }
    }
}
