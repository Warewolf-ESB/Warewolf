using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.ReadFile
{
    public class ReadFileDesignerViewModel : ActivityDesignerViewModel
    {
        public ReadFileDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}