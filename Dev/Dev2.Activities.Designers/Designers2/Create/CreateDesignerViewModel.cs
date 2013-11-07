using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Create
{
    public class CreateDesignerViewModel : FileActivityDesignerViewModel
    {
        public CreateDesignerViewModel(ModelItem modelItem)
            : base(modelItem, string.Empty, "File or Folder")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateOutputPath();
        }
    }
}