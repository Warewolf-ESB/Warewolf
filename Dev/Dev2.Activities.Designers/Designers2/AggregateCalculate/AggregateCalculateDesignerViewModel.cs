using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Core;
using Dev2.Interfaces;

namespace Dev2.Activities.Designers2.AggregateCalculate
{
    public class AggregateCalculateDesignerViewModel : ActivityDesignerViewModel
    {
        public AggregateCalculateDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
        }

        public override void Validate()
        {
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
