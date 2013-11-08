using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.ReadFolder
{
    public class ReadFolderDesignerViewModel : FileActivityDesignerViewModel
    {
        public ReadFolderDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "Directory", string.Empty)
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