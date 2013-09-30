using System;
using System.Activities.Presentation.View;
using Dev2.Activities.Designers2.Core;

namespace Dev2.Core.Tests.Activities
{

    public class testActivityDesigner : ActivityDesigner<TestActivityViewModel>
    {

        public void SetWorkflowSelection(Selection selection)
        {
            //WorkflowDesignerSelection = selection;
        }

        public void CallSelectionChange(Selection item)
        {
            //SelectionChanged(item);
        }

        public void TestRestoreAllRequested()
        {
            OnDesignerManagementServiceRestoreAllRequested(null, new EventArgs());
        }

        public void TestExpandAllRequested()
        {
            OnDesignerManagementServiceExpandAllRequested(null, new EventArgs());
        }

        public void TestCollapseAllRequested()
        {
            OnDesignerManagementServiceCollapseAllRequested(null, new EventArgs());
        }

        public void TestInit()
        {
            OnLoaded();
        }
    }
}
