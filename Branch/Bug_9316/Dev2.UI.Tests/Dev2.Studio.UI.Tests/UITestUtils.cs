using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dev2.Studio.UI.Tests
{
    public static class UITestUtils
    {
        public static string GetStudioWindowName()
        {
            return "Business Design Studio (DEV2\\" + Environment.UserName + ")";
        }

        //public static void CreateWorkflow(string WorkflowName)
        //{
        //    RibbonUIMap.ClickRibbonMenuItem("Home", "Workflow");
        //    while (!WorkflowWizardUIMap.IsWindowOpen())
        //        Thread.Sleep(500);
        //    Thread.Sleep(1000);
        //    WorkflowWizardUIMap.EnterWorkflowName(WorkflowName);
        //    WorkflowWizardUIMap.EnterWorkflowCategory("CodedUITestCategory");
        //    WorkflowWizardUIMap.DoneButtonClick();
        //}

    }
}
