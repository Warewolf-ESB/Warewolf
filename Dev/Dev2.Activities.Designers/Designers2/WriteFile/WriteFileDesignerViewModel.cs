using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.WriteFile
{
    public class WriteFileDesignerViewModel : ActivityDesignerViewModel
    {
        public WriteFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}