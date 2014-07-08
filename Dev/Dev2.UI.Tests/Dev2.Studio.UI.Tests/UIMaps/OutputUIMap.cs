using System;
using System.Linq;
using Dev2.Studio.UI.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    public partial class OutputUIMap
    {
        UITestControl _outputPane;
        private UITestControl _outputStatus;

        public OutputUIMap()
        {
            var vstw = new VisualTreeWalker();

            _outputPane = vstw.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z96bb9badc4b148518ea4eff80920f8d9", "OutputPane", "DebugOutput");
            vstw.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z96bb9badc4b148518ea4eff80920f8d9", "OutputPane", "DebugOutput", "Edit");
            _outputStatus = vstw.GetControlFromRoot(0, false, 0, "Uia.SplitPane", "Z96bb9badc4b148518ea4eff80920f8d9", "OutputPane", "DebugOutput", "Dev2StatusBarAutomationID", "StatusBar");

        }

        private WpfTree GetOutputTree()
        {
            var vstw = new VisualTreeWalker();

            var debugOutputTree = vstw.GetChildByAutomationIDPath(_outputPane, "Uia.TreeView");
            return debugOutputTree as WpfTree;
        }

        public UITestControlCollection GetOutputWindow()
        {
            WpfTree debugOutputControlTree = GetOutputTree();
            return debugOutputControlTree.Nodes;
        }

        /// <summary>
        /// Gets the status bar status.
        /// </summary>
        /// <returns></returns>
        public string GetStatusBarStatus()
        {
            var statusBarChildren = _outputStatus.GetChildren();
            if(statusBarChildren != null)
            {
                var statusBar = statusBarChildren.FirstOrDefault(child => child.ClassName == "Uia.Text") as WpfText;
                if(statusBar != null)
                {
                    return statusBar.DisplayText;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Determines whether [is spinner spinning].
        /// </summary>
        /// <returns></returns>
        public bool IsSpinnerSpinning()
        {
            if(_outputStatus != null)
            {
                var kids = _outputStatus.GetChildren();

                if(kids != null)
                {
                    var spinner = kids.FirstOrDefault(child => child.ClassName == "Uia.CircularProgressBar");
                    return spinner != null && spinner.Height != -1;
                }
            }

            return false;
        }

        public UITestControlCollection GetStepInOutputWindow(UITestControl outputWindow, string stepToFind)
        {
            UITestControlCollection coll = outputWindow.GetChildren();
            UITestControlCollection results = new UITestControlCollection();
            foreach(var child in coll)
            {
                if(child.Name.Equals(stepToFind))
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
            UITestControlCollection coll = outputWindow.GetChildren();
            for(int i = 0; i <= coll.Count; i++)
            {
                if(coll[i].Name.Equals(stepInformationToFind + " : "))
                {
                    return coll[i + 1];
                }
            }
            return null;
        }

        public string GetStepName(UITestControl workflowStep)
        {
            //return workflowStep.Name;
            string workflowNamePrefix = "Workflow : ";
            UITestControlCollection coll = workflowStep.GetChildren();
            for(int i = 0; i <= coll.Count; i++)
            {
                if(coll[i].Name.Contains(workflowNamePrefix))
                {
                    return coll[i + 1].FriendlyName;
                }
            }
            return null;
        }


        public bool AssertDebugOutputContains(UITestControl workflowStep, string[] outputs)
        {
            UITestControlCollection coll = workflowStep.GetChildren();
            return outputs.All(a => coll.Any(b => b.FriendlyName.Contains(a)));
        }


        public static UITestControlCollection GetInputDetailsDetails(UITestControl outputWindow)
        {
            var coll = outputWindow.GetChildren();
            var results = new UITestControlCollection();
            for(int i = 0; i <= coll.Count; i++)
            {
                if(coll[i].Name.Equals("Inputs : "))
                {
                    int j = 1;
                    while(!coll[i + j].Name.Equals("Outputs : "))
                    {
                        if(coll[i + j].ControlType != ControlType.Button)
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


        public static bool IsRemoteExecute(UITestControl theStep)
        {
            var errorResults = theStep.GetChildren()
                                      .ToList()
                                      .Where(c => c.FriendlyName.Contains("Remote Connection"))
                                      .ToList();

            if(errorResults.Count == 0)
            {
                return false;
            }

            // Steps that aren't in errors still have the error text
            // Due to this, we can use the visibility (AKA: If height is -1, it's hidden (And not just 1 pixel above 0...))
            return errorResults[0].Height != -1;
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

        /// <summary>
        /// Determines whether [is execution remote].
        /// </summary>
        /// <returns></returns>
        public bool IsExecutionRemote()
        {
            var debugOutput = GetOutputWindow().ToList();

            return debugOutput.Any(IsRemoteExecute);
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

        public void WaitForExecution(int waitAmt = 200)
        {
            while(IsSpinnerSpinning())
            {
                Playback.Wait(waitAmt);
            }
        }

        public UITestControl GetLastStep()
        {
            UITestControlCollection uiTestControlCollection = GetOutputWindow();
            Assert.IsTrue(uiTestControlCollection.Count > 0);
            return uiTestControlCollection[uiTestControlCollection.Count - 1];
        }

        public UITestControl GetStep(int stepIndex)
        {
            UITestControlCollection uiTestControlCollection = GetOutputWindow();
            Assert.IsTrue(uiTestControlCollection.Count > 0);
            return uiTestControlCollection[stepIndex];
        }
        public UITestControl GetStepType(int stepIndex)
        {
            UITestControlCollection uiTestControlCollection = GetOutputWindow();
            Assert.IsTrue(uiTestControlCollection.Count > 0);
            return uiTestControlCollection[stepIndex];
        }
    }
}
