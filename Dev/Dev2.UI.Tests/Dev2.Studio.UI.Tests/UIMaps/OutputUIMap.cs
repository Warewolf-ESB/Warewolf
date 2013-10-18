using System;
using System.Linq;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses
{
    public partial class OutputUIMap
    {
        UITestControl _debugOutput;
        public OutputUIMap()
        {
            _debugOutput = VisualTreeWalker.GetControl("UI_DocManager_AutoID", "OutputPane", "DebugOutput");
        }

        /// <summary>
        /// Finds a control on the Output Pane
        /// </summary>
        /// <param name="controlAutomationId">The automation ID of the control you are looking for</param>
        /// <returns>Returns the control as a UITestControl object</returns>
        public UITestControl FindControlByAutomationId(string controlAutomationId)
        {
            // Unless the UI drastically changes (In which case most Automation tests will fail),
            // the order will remain constant

            // Cake names are used until they are replaced by the real names
            UITestControlCollection theCollection = new UITestControlCollection();
            try
            {
                theCollection = GetOutputWindow();
            }
            catch
            {
                Assert.Fail("Error - Could not find '" + controlAutomationId + "' on the workflow designer!");
            }
            UITestControl splurtControl = theCollection[4];
            UITestControlCollection splurtChildChildren = splurtControl.GetChildren()[0].GetChildren();
            UITestControl cake2 = splurtChildChildren[0];
            UITestControlCollection cake2Children = cake2.GetChildren();
            UITestControl cake38 = cake2Children[3];
            UITestControlCollection cake38Children = cake38.GetChildren();
            // Cake38 -> ActivityTypeDesigner -> Cake53 -> FlowchartDesigner -> *Control Here*
            UITestControl cake53 = cake38Children[0].GetChildren()[0];
            UITestControlCollection cake53Children = cake53.GetChildren();
            UITestControl flowchartDesigner = cake53Children[0];
            UITestControlCollection flowchartDesignerChildren = flowchartDesigner.GetChildren();
            foreach (UITestControl theControl in flowchartDesignerChildren)
            {
                string automationId = theControl.GetProperty("AutomationId").ToString();
                if (automationId.Contains(controlAutomationId))
                {
                    return theControl;
                }
            }
            return null;
        }

        private WpfTree GetOutputTree()
        {
            return _debugOutput.GetChildren().FirstOrDefault(child => child.ClassName == "Uia.TreeView") as WpfTree;
        }

        public UITestControlCollection GetOutputWindow()
        {
            WpfTree debugOutputControlTree = GetOutputTree();
            return debugOutputControlTree.Nodes;
        }

        private UITestControl GetStatusBar()
        {
            return VisualTreeWalker.GetChildByAutomationIDPath(_debugOutput, "Dev2StatusBarAutomationID", "StatusBar");
        }

        public string GetStatusBarStatus()
        {
            var statusBarChildren = GetStatusBar().GetChildren();
            var statusBar = statusBarChildren.FirstOrDefault(child => child.ClassName == "Uia.Text") as WpfText;
            return statusBar.DisplayText;
        }

        public bool IsSpinnerSpinning()
        {
            var spinner =
                GetStatusBar().GetChildren().FirstOrDefault(child => child.ClassName == "Uia.CircularProgressBar");
            return spinner.Height != -1;
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

        public UITestControlCollection GetStepsByFriendly(UITestControl outputWindow, string friendlyName)
        {
            var children = outputWindow.GetChildren().Where(c => c.FriendlyName.Equals(friendlyName, StringComparison.CurrentCultureIgnoreCase));
            var uiCollection = new UITestControlCollection();

            foreach(var child in children)
            {
                uiCollection.Add(child);
            }

            return uiCollection;
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

        public static UITestControlCollection GetInputDetailsDetails(UITestControl outputWindow)
        {           
            var coll = outputWindow.GetChildren();
            var results = new UITestControlCollection();
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
            var closeBtn = new UITestControl(theControl);
            closeBtn.SearchProperties.Add("AutomationId", "closeBtn");
            closeBtn.Find();

            // The clicking is a bit slow for some reason - Need to investigate
            Mouse.Click(closeBtn);
        }

        public void ClickSearch()
        {
            // Base control
            UITestControl theControl = UIBusinessDesignStudioWindow.UIDebugOutputCustom;
            theControl.Find();

            // Sub button
            var srchBtn = new UITestControl(theControl);
            srchBtn.SearchProperties.Add("AutomationId", "UI_DataListSearchtxt_AutoID");
            srchBtn.Find();

            // The clicking is a bit slow for some reason - Need to investigate
            Mouse.Click(srchBtn);
        }

        public void ClearSearch()
        {
            // Base control
            UITestControl theControl = UIBusinessDesignStudioWindow.UIDebugOutputCustom;
            theControl.Find();

            // Sub button
            var srchBtn = new UITestControl(theControl);
            srchBtn.SearchProperties.Add("AutomationId", "UI_DataListSearchtxt_AutoID");
            srchBtn.Find();

            // The clicking is a bit slow for some reason - Need to investigate
            Mouse.Click(srchBtn);
            Keyboard.SendKeys("^a");
            Keyboard.SendKeys("{DELETE}");
        }

        public UITestControlCollection GetWorkflowSteps(UITestControl theWorkflow, string controlId)
        {
            var stepSearcher = new UITestControl(theWorkflow);
            stepSearcher.SearchProperties.Add("AutomationId", controlId, PropertyExpressionOperator.Contains);
            UITestControlCollection steps = stepSearcher.FindMatchingControls();
            return steps;
        }

        public static bool IsStepInError(UITestControl theStep)
        {
            var errorResults = theStep.GetChildren()
                                      .ToList()
                                      .Where(c => c.FriendlyName.Equals("Error : "))
                                      .ToList();
            
            if(errorResults.Count == 0)
            {
                return false;
            }

            // Steps that aren't in errors still have the error text
            // Due to this, we can use the visibility (AKA: If height is -1, it's hidden (And not just 1 pixel above 0...))
            return errorResults[0].Height != -1;
        }

        public bool IsAnyStepsInError()
        {
            var debugOutput = GetOutputWindow().ToList();
            return debugOutput.Any(IsStepInError);
        }

        public void WaitForStepCount(int expectedStepCount, int timeOut)
        {
            const int interval = 100;
            int count = 0;
            while(GetOutputWindow().Count < expectedStepCount && count <= timeOut)
            {
                Playback.Wait(interval);
                count = count + interval;
            }
            if(count == timeOut && GetOutputWindow().Count < expectedStepCount)
            {
                throw new Exception("Debug output never reached the expected step count in the given time out");
            }
        }

        public void WaitForExecution()
        {
            while (IsSpinnerSpinning())
            {
                Playback.Wait(500);
            }
        }
    }
}
