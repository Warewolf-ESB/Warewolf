using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.ReadFile
{
    public class ReadFileDesignerViewModel : FileActivityDesignerViewModel
    {
        public ReadFileDesignerViewModel(ModelItem modelItem)
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