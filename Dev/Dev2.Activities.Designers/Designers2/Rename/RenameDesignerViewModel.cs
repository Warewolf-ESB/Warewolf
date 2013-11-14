using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Rename
{
    public class RenameDesignerViewModel : FileActivityDesignerViewModel
    {
        public RenameDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", "New Name")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateDestinationUsernameAndPassword();
            ValidateInputAndOutputPaths();
        }
    }
}