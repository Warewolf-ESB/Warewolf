using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System.Collections.Generic;

namespace Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses
{
    public partial class OutputUIMap
    {

        public bool DoesBug8747Pass()
        {
            WpfTree theTree = OutputTree();

            // Item 0 is the XML at the top
            UITestControl theWorkflow = theTree.Nodes[1];
            UITestControl workflowSearcher = new UITestControl(theWorkflow);
            workflowSearcher.SearchProperties.Add("AutomationId", "DsfActivity", PropertyExpressionOperator.Contains);
            workflowSearcher.SearchProperties.Add("ControlType", "TreeItem");
            UITestControlCollection subWorkflows = workflowSearcher.FindMatchingControls();
            UITestControl firstWorkflow = subWorkflows[0];

            // This lists the step of the specific bugs error
            WpfTreeItem theStep = (WpfTreeItem)GetWorkflowSteps(firstWorkflow, "Assign")[0];
            if (!IsStepInError(theStep))
            {
                return false;
            }

            // Whew - First one passes - How about the second one?
            WpfTreeItem otherWorkflow = (WpfTreeItem)theTree.Nodes[3]; // It's also a step - Very confusing :p
            if (IsStepInError(otherWorkflow))
            {
                return false;
            }

            // Everything passes :D
            return true;
        }

        public UITestControlCollection GetOutputWindow()
        {
            WpfTree debugOutputControlTree = OutputTree();
            return debugOutputControlTree.Nodes;

        }

        public UITestControlCollection GetStepInOutputWindow(UITestControl outputWindow, string stepToFind)
        {
            UITestControl workflowSearcher = new UITestControl(outputWindow);
            UITestControlCollection coll = outputWindow.GetChildren();
            UITestControlCollection results = new UITestControlCollection();
            foreach (var child in coll)
            {
                if (child.Name.Equals(stepToFind))
                {
                    results.Add(child);
                }
            }

            return results;
        }

        public UITestControl GetStepDetails(UITestControl outputWindow, string stepInformationToFind)
        {
            UITestControl workflowSearcher = new UITestControl(outputWindow);
            UITestControlCollection coll = outputWindow.GetChildren();
            UITestControlCollection results = new UITestControlCollection();
            for (int i = 0; i <= coll.Count; i++)
            {
                if (coll[i].Name.Equals(stepInformationToFind + " : "))
                {
                    return coll[i + 1];
                }
            }
            return null;
        }

        public UITestControlCollection GetInputDetailsDetails(UITestControl outputWindow)
        {
            UITestControl workflowSearcher = new UITestControl(outputWindow);
            UITestControlCollection coll = outputWindow.GetChildren();
            UITestControlCollection results = new UITestControlCollection();
            for (int i = 0; i <= coll.Count; i++)
            {
                if (coll[i].Name.Equals("Inputs : "))
                {
                    int j = 1;
                    while (!coll[i + j].Name.Equals("Outputs : "))
                    {
                        if (coll[i + j].ControlType != ControlType.Button)
                            results.Add(coll[i + j]);
                        j++;
                    }
                    return results;
                }
            }
            return results;
        }

        public void ClickClose()
        {
            // Base control
            UITestControl theControl = UIBusinessDesignStudioWindow.UIDebugOutputCustom;
            theControl.Find();

            // Sub button
            UITestControl closeBtn = new UITestControl(theControl);
            closeBtn.SearchProperties.Add("AutomationId", "closeBtn");
            closeBtn.Find();

            // The clicking is a bit slow for some reason - Need to investigate
            Mouse.Click(closeBtn);
        }

        public UITestControlCollection GetWorkflowSteps(UITestControl theWorkflow, string controlId)
        {
            UITestControl stepSearcher = new UITestControl(theWorkflow);
            stepSearcher.SearchProperties.Add("AutomationId", controlId, PropertyExpressionOperator.Contains);
            UITestControlCollection steps = stepSearcher.FindMatchingControls();
            return steps;
        }

        private bool IsStepInError(WpfTreeItem theStep)
        {
            UITestControl errorSearcher = new UITestControl(theStep);
            errorSearcher.SearchProperties.Add("ControlType", "Text");
            errorSearcher.SearchProperties.Add("Name", "Error : ");
            UITestControlCollection errorResults = errorSearcher.FindMatchingControls();
            // Steps that aren't in errors still have the error text
            // Due to this, we can use the visibility (AKA: If height is -1, it's hidden (And not just 1 pixel above 0...))
            return errorResults[0].Height != -1;
        }
    }
}
