using System;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using Dev2.Activities.Adorners;
using Dev2.Activities.Designers;

namespace Dev2.Core.Tests.Activities
{

    public class TestActivityDesigner : ActivityDesignerBase<TestActivityViewModel>
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
