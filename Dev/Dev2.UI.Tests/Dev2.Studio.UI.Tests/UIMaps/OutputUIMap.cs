namespace Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
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
            WpfTreeItem theStep = (WpfTreeItem)GetWorkflowSteps(firstWorkflow)[0];
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

        private UITestControlCollection GetWorkflowSteps(UITestControl theWorkflow)
        {
            UITestControl stepSearcher = new UITestControl(theWorkflow);
            stepSearcher.SearchProperties.Add("AutomationId", "Assign", PropertyExpressionOperator.Contains);
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
            if (errorResults[0].Height != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
