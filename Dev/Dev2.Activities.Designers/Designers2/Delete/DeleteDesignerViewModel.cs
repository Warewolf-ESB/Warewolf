using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Delete
{
    public class DeleteDesignerViewModel : FileActivityDesignerViewModel
    {
        public DeleteDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", string.Empty)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputPath();
        }
    }
}