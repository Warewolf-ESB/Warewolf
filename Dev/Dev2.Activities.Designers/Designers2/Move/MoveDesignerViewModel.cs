using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Move
{
    public class MoveDesignerViewModel : FileActivityDesignerViewModel
    {
        public MoveDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File or Folder", "Destination")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidInputAndOutputPaths();
        }
    }
}