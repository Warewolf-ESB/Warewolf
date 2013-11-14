using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Unzip
{
    public class UnzipDesignerViewModel : FileActivityDesignerViewModel
    {
        public UnzipDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "Zip Name", "Destination")
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateDestinationUserNameAndPassword();
            ValidateInputAndOutputPaths();
        }
    }
}