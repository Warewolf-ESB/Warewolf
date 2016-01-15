using System.Activities.Presentation.Model;

namespace Dev2
{
    public class SelectModelItemMessage
    {
        public ModelItem ModelItem { get; set; }

        public SelectModelItemMessage(ModelItem modelItem)
        {
            ModelItem = modelItem;
        }
    }
}