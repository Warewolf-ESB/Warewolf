using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.Replace
{
    public class ReplaceDesignerViewModel : ActivityDesignerViewModel
    {
        public ReplaceDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }
       
        public override void Validate()
        {
        }
    }
}