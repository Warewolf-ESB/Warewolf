using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.WriteFile
{
    public class WriteFileDesignerViewModel : FileActivityDesignerViewModel
    {
        public WriteFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem, "File Name", string.Empty)
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