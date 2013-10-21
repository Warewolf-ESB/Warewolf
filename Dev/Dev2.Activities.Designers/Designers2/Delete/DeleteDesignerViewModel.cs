using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Delete
{
    public class DeleteDesignerViewModel : ActivityDesignerViewModel
    {
        public DeleteDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}