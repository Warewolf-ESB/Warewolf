using System.Activities.Presentation.View;
using System.Windows.Controls.Primitives;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;

namespace Dev2.Core.Tests.Activities
{

    public class testActivityDesigner : ActivityDesignerBase<TestActivityViewModel>
    {
        public void SetOptionsAdorner(AbstractOptionsAdorner adorner)
        {
            OptionsAdorner = adorner;
        }

        public void SetOverlaydorner(AbstractOverlayAdorner adorner)
        {
            OverlayAdorner = adorner;
        }

        public void SetWorkflowSelection(Selection selection)
        {
            WorkflowDesignerSelection = selection;
        }

        public void CallSelectionChange(Selection item)
        {
          SelectionChanged(item);  
        }

        public void CallShowContent(ButtonBase selectedOption)
        {
            ShowContent(selectedOption);
        }

    }
}
