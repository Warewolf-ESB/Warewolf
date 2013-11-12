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

            if (!Overwrite && !AppendTop && !AppendBottom)
            {
                Overwrite = true;
            }
        }

        public override void Validate()
        {
            Errors = null;
            ValidateUserNameAndPassword();
            ValidateInputPath();
        }

        bool Overwrite { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        bool AppendTop { set { SetProperty(value); } get { return GetProperty<bool>(); } }
        bool AppendBottom { set { SetProperty(value); } get { return GetProperty<bool>(); } }
    }
}