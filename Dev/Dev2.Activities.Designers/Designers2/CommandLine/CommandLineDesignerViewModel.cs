using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Activities.Designers2.CommandLine
{
    public class CommandLineDesignerViewModel : ActivityDesignerViewModel
    {
        public CommandLineDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarHelpToggle();
        }

        public override void Validate()
        {
        }
    }
}