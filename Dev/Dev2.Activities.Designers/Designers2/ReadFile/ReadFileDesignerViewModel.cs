using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.ReadFile
{
    public class ReadFileDesignerViewModel : FileActivityDesignerViewModel
    {
        public ReadFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();
        }

        // DO NOT bind to these properties - these are here for convenience only!!!
        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputs();
        }
        
    }
}