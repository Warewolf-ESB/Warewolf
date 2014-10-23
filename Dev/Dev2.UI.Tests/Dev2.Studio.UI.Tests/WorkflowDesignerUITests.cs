
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.Studio.UI.Tests.Enums;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    // ReSharper disable InconsistentNaming
    public class WorkflowDesignerUITests : UIMapBase
    {
        #region Init/Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            
        }
        #endregion

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("WorkflowDesigner_CodedUI")]
        public void WorkflowDesigner_CodedUI_CopyAndPastingAndDeleteingActivity_CopyPasteAndDeleteWork()
        {
            //------------Setup for test--------------------------
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            //Find the start point
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, workflowPoint1);

            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);

            SendKeys.SendWait("Hello");

            //Get Large View button
            WorkflowDesignerUIMap.OpenCloseLargeView(ToolType.Assign, theTab);


            //------------Execute Test---------------------------

            SendKeys.SendWait("^c");
            SendKeys.SendWait("^v");

            //------------Assert Results-------------------------
            UITestControlCollection allControlsOnDesignSurface = WorkflowDesignerUIMap.GetAllControlsOnDesignSurface(theTab);

            IEnumerable<UITestControl> uiTestControls = allControlsOnDesignSurface.Where(c => c.Name == "DsfMultiAssignActivity");

            Assert.IsTrue(uiTestControls.Count() == 2);

            SendKeys.SendWait("{DELETE}");

            allControlsOnDesignSurface = WorkflowDesignerUIMap.GetAllControlsOnDesignSurface(theTab);

            uiTestControls = allControlsOnDesignSurface.Where(c => c.Name == "DsfMultiAssignActivity");

            Assert.IsTrue(uiTestControls.Count() == 1);
        }


        //2013.05.29: Ashley Lewis for bug 9455 - Dont allow copy paste workflow xaml to another workflow
        [TestMethod]
        public void CopyWorkFlowWithContextMenuCopyAndPasteToAnotherWorkflowExpectedNothingCopied()
        {
            Clipboard.SetText(" ");

            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            WorkflowDesignerUIMap.CopyWorkflowXamlWithContextMenu(theTab);
            Assert.IsTrue(string.IsNullOrWhiteSpace(Clipboard.GetText()),
                          "Able to copy workflow Xaml using context menu");
            RibbonUIMap.CreateNewWorkflow();
            theTab = TabManagerUIMap.GetActiveTab();
            var startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            Mouse.Click(new Point(startButton.BoundingRectangle.X - 5, startButton.BoundingRectangle.Y - 5));
            SendKeys.SendWait("^V");
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab,
                                                                                    "Unsaved 1(FlowchartDesigner)"));
        }

        //2013.06.06: Ashley Lewis for 9448 - Dsf Activity Title - shows up as "DSFActivity" After a service has been dragged onto a workflow.
        [TestMethod]
        public void AddSecondServiceToWorkFlowExpectedDisplayTitleNotDsfActivity()
        {
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);


            ExplorerUIMap.EnterExplorerSearchText("email service");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "Communication", "Email Service",
                                                        new Point(startButton.BoundingRectangle.X + 50,
                                                                  startButton.BoundingRectangle.Y + 150));

            WorkflowDesignerUIMap.TryCloseMappings("Email Service");

            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "Communication", "Email Service",
                                                        new Point(startButton.BoundingRectangle.X + 50,
                                                                  startButton.BoundingRectangle.Y + 300));

            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "DsfActivity(ServiceDesigner)"), "Dropped services display title was 'DsfActivity' rather than the name of the service");
        }

        [TestMethod]
        [TestCategory("Toolbox_Icons")]
        [Description("Toolbox icons display")]
        [Owner("Ashley Lewis")]
        public void Toolbox_UITest_OpenToolbox_IconsAreDisplayed()
        {
            RibbonUIMap.CreateNewWorkflow();
            ToolboxUIMap.ClearSearch();
            foreach(var tool in ToolboxUIMap.GetAllTools())
            {

                var kids = tool.GetChildren();

                if(kids.Count == 3)
                {
                    var icon = kids[1];

                    var wValue = icon.BoundingRectangle.Width;
                    var hValue = icon.BoundingRectangle.Height;

                    Assert.AreEqual(18, wValue);
                    Assert.AreEqual(18, hValue);

                }
                else
                {
                    Assert.Fail(tool.FriendlyName + " is missing its icon in the toolbox");
                }
            }
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Clicking a debug output step should highlight that activity on the design surface")]
        [Owner("Ashley Lewis")]
        public void DebugOutput_ClickStep_ActivityIsHighlighted()
        {
            //Create testing workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            //Drag on multiassign
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            var thePoint = new Point(theStartButton.BoundingRectangle.X + 30, theStartButton.BoundingRectangle.Y + 90);
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, thePoint);

            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);

            //Set up multi assign
            SendKeys.SendWait("[[AssignThis]]{TAB}Some Data");

            //run and wait until debug output comes through
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            //Click step
            var step = OutputUIMap.GetOutputWindow();
            Playback.Wait(1500);
            Mouse.Click(step[2]);
            Playback.Wait(100);
            Mouse.Click(step[1]);
            Playback.Wait(100);
            Mouse.Click(step[2]);
            Playback.Wait(100);

            //Assert the design surface activity is highlighted
            var assign = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Assign");
            Assert.IsTrue(WorkflowDesignerUIMap.IsControlSelected(assign),
                          "Selecting a step in the debug output does not select the activity on the design surface");

        }

        [TestMethod]
        [TestCategory("UITest")]
        [Owner("Tshepo Ntlhokoa")]
        public void DragAStartNodeOntoATool_HoverOverAToolForAWhile_NoDrillDownShouldHappen()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            var workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an assign onto the Workflow
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, workflowPoint1);

            //Drag Start Node
            Mouse.StartDragging(theStartButton, MouseButtons.Left);
            UITestControl assign = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Assign");
            var point = new Point(assign.BoundingRectangle.X + 150, assign.BoundingRectangle.Y + 50);
            //Hover over the multi assign for 5 seconds
            Mouse.Move(point);
            Playback.Wait(2000);
            Mouse.Click();

            // ensure the start btn is visible, hence no drill down
            theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Assert.IsTrue(theStartButton.Exists, "Start Node Hover Caused Drilldown");
        }
    }
}
