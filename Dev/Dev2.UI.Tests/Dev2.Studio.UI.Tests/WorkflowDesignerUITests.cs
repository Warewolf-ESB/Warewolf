using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest, System.Runtime.InteropServices.GuidAttribute("7E6836ED-8C14-4BFD-ADD0-3C5C6F0CB815")]
// ReSharper disable InconsistentNaming
    public class WorkflowDesignerUITests : UIMapBase
// ReSharper restore InconsistentNaming
    {
        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        #region Cleanup

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        // Bug 6501
        [TestMethod][Ignore]//ashley: testing 17.10.2013
        public void DeleteFirstDatagridRow_Expected_RowIsNotDeleted()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("BaseConvert");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Enter some data
            UITestControl baseConversion = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "BaseConvert");
            Point p = new Point(baseConversion.BoundingRectangle.X + 40, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(p);
            SendKeys.SendWait("someText");

            // Click the index
            p = new Point(baseConversion.BoundingRectangle.X + 20, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            Thread.Sleep(500);
            SendKeys.SendWait("{UP}");
            Thread.Sleep(500);
            //SendKeys.SendWait("{UP}");
            //Thread.Sleep(500);
            //SendKeys.SendWait("{RIGHT}");
            //Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(100);

            // Try type some data
            p = new Point(baseConversion.BoundingRectangle.X + 40, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(p);
            SendKeys.SendWait("newText");
            SendKeys.SendWait("{END}"); // Shift Home - Highlights the item
            SendKeys.SendWait("+{HOME}"); // Shift Home - Highlights the item
            // Just to make sure it wasn't already copied before the test
            Clipboard.SetText("someRandomText");
            SendKeys.SendWait("^c"); // Copy command
            string clipboardText = Clipboard.GetText();
            if(clipboardText == "someText")
            {
                Assert.Fail("Error - The Item was not deleted! [ " + clipboardText + " ]");
            }
        }

        //2013.05.29: Ashley Lewis for bug 9455 - Dont allow copy paste workflow xaml to another workflow
        [TestMethod][Ignore]//ashley: testing 17.10.2013
        public void CopyWorkFlowWithContextMenuCopyAndPasteToAnotherWorkflowExpectedNothingCopied()
        {
            Clipboard.SetText(" ");

            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            WorkflowDesignerUIMap.CopyWorkflowXamlWithContextMenu(theTab);
            Assert.IsTrue(string.IsNullOrWhiteSpace(Clipboard.GetText()), "Able to copy workflow Xaml using context menu");
            RibbonUIMap.CreateNewWorkflow();
            theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            var startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            Mouse.Click(new Point(startButton.BoundingRectangle.X - 5, startButton.BoundingRectangle.Y - 5));
            SendKeys.SendWait("^V");
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Unsaved 1(FlowchartDesigner)"));
            TabManagerUIMap.CloseAllTabs();
        }

        //2013.06.06: Ashley Lewis for 9448 - Dsf Activity Title - shows up as "DSFActivity" After a service has been dragged onto a workflow.
        [TestMethod][Ignore]//ashley: testing 17.10.2013
        public void AddSecondServiceToWorkFlowExpectedDisplayTitleNotDsfActivity()
        {
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);

            DockManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 150));

            DockManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 300));

            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();

            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "DsfActivity(DsfActivityDesigner)"), "Dropped services display title was 'DsfActivity' rather than the name of the service");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("UITest")]
        [Description("Test for 'All Tools' workflow: The workflow is openned. The icons must display. The tab must be able to close again")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void StudioTooling_StudioToolingUITest_CanToolsDisplay_NoExceptionsThrown()
        // ReSharper restore InconsistentNaming
        {
            // Open the Explorer
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AllTools");

            // Open the Workflow
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MOCAKE", "AllTools");
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());

            // Assert all the icons are visible
            var designer = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            var allTools = designer.GetChildren();
            var allFoundTools = new UITestControlCollection();
            foreach(var child in allTools)
            {
                if(child.ControlType == "Custom" &&
                    child.ClassName != "Uia.ConnectorWithoutStartDot" &&
                    child.ClassName != "Uia.StartSymbol" &&
                    child.ClassName != "Uia.UserControl" &&
                    child.ClassName != "Uia.DsfWebPageActivityDesigner")
                {
                    //Some of the tools on the design surface are out of view, look for them...
                    if(child.BoundingRectangle.Y > 800)
                    {
                        //Look low
                        Mouse.StartDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollBar(theTab));
                        Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollDown(theTab));
                    }
                    else
                    {
                        //Look high
                        Mouse.StartDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollBar(theTab));
                        Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollUp(theTab));
                    }
                    Assert.IsTrue(WorkflowDesignerUIMap.IsActivityIconVisible(child), child.FriendlyName + " is missing its icon on the design surface");
                    allFoundTools.Add(child);
                }
            }
            Assert.AreEqual(allTools.ToList().Count(child => child.ControlType == "Custom" &&
                    child.ClassName != "Uia.ConnectorWithoutStartDot" &&
                    child.ClassName != "Uia.StartSymbol" &&
                    child.ClassName != "Uia.UserControl" &&
                    child.ClassName != "Uia.DsfWebPageActivityDesigner"), allFoundTools.Count, "Not all tools on the alls tools text workflow can be checked for icons");

            Assert.IsTrue(true, "Studio was terminated or hung while opening and closing the all tools workflow");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("Toolbox_Icons")]
        [Description("Toolbox icons display")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void Toolbox_UITest_OpenToolbox_IconsAreDisplayed()
        // ReSharper restore InconsistentNaming
        {
            RibbonUIMap.CreateNewWorkflow();
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            foreach(var tool in ToolboxUIMap.GetAllTools())
            {
                Assert.IsTrue(ToolboxUIMap.IsIconVisible(tool), tool.FriendlyName + " is missing its icon in the toolbox");
            }
        }

        /// <summary>
        /// Debugs the output_ click step_ activity is highlighted.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/08/13</date>
        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("UITest")]
        [Description("Clicking a debug output step should highlight that activity on the design surface")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DebugOutput_ClickStep_ActivityIsHighlighted()
        // ReSharper restore InconsistentNaming
        {
            //Create testing workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();

            //Drag on multiassign
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            var thePoint = new Point(theStartButton.BoundingRectangle.X + 30, theStartButton.BoundingRectangle.Y + 100);
            ToolboxUIMap.DragControlToWorkflowDesigner("MultiAssign", thePoint);

            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "MultiAssign", 0);

            //Set up multi assign
            SendKeys.SendWait("[[AssignThis]]{TAB}Some Data");

            //run and wait until debug output comes through
            WorkflowDesignerUIMap.RunWorkflowAndWaitUntilOutputStepCountAtLeast(2);

            //Click step
            DockManagerUIMap.ClickOpenTabPage("Output");
            var step = OutputUIMap.GetOutputWindow();
            Mouse.Click(step[2]);
            Playback.Wait(100);
            Mouse.Click(step[1]);
            Playback.Wait(100);
            Mouse.Click(step[2]);
            Playback.Wait(100);
            Mouse.Click(step[1]);
            Playback.Wait(100);

            //Assert the design surface activity is highlighted
            var workflow = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            Assert.IsTrue(WorkflowDesignerUIMap.IsControlSelected(workflow), "Selecting a step in the debug output does not select the activity on the design surface");

            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("UnsavedWorkflows_UITest")]
        [Description("For bug 10086 - Switching tabs does not flicker unsaved status")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void Tabs_UnsavedStar_SwitchingTabs_DoesNotChangeUnsavedStatus()
        // ReSharper restore InconsistentNaming
        {
            var firstName = "Test" + Guid.NewGuid().ToString().Substring(24);
            var secondName = "Test" + Guid.NewGuid().ToString().Substring(24);
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClickServerInServerDDL("localhost");
            // Create first workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();
            var theStartNode = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "StartSymbol");
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("PBI_9957_UITEST");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "BUGS", "PBI_9957_UITEST",
                new Point(theStartNode.BoundingRectangle.X + 20,
                            theStartNode.BoundingRectangle.Y + 100));
            RibbonUIMap.ClickRibbonMenuItem("Save");
            WizardsUIMap.WaitForWizard();
            SaveDialogUIMap.ClickAndTypeInNameTextbox(firstName);
            Playback.Wait(3000);
          
            // Create second workflow
            RibbonUIMap.CreateNewWorkflow();
            Playback.Wait(1000);
            theTab = TabManagerUIMap.GetActiveTab();
            theStartNode = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "StartSymbol");
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindToolboxItemByAutomationId("MultiAssign"),
                new Point(theStartNode.BoundingRectangle.X + 20,
                            theStartNode.BoundingRectangle.Y + 100));
            RibbonUIMap.ClickRibbonMenuItem("Save");
            WizardsUIMap.WaitForWizard(10000);
            SaveDialogUIMap.ClickAndTypeInNameTextbox(secondName);
            Playback.Wait(3000);

            // Switch between tabs ensuring the star is never added to their name
            UITestControl tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(secondName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Move(new Point(StudioWindow.Left + 200, StudioWindow.Top + 200));
            ExplorerUIMap.ClosePane(tryGetTab);
            Mouse.Click(TabManagerUIMap.FindTabByName(secondName));
            tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(firstName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Move(new Point(StudioWindow.Left + 200, StudioWindow.Top + 200));
            ExplorerUIMap.ClosePane(tryGetTab);
            Mouse.Click(TabManagerUIMap.FindTabByName(firstName));
            tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(secondName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Move(new Point(StudioWindow.Left + 200, StudioWindow.Top + 200));
            ExplorerUIMap.ClosePane(tryGetTab);
            Mouse.Click(TabManagerUIMap.FindTabByName(secondName));
        }

        // Bug 6617
        [TestMethod][Ignore]//ashley: testing 17.10.2013
        public void OpeningDependancyWindowTwiceKeepsItOpen()
        {
            // The workflow so we have a second tab
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Base64ToString");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            DockManagerUIMap.ClickOpenTabPage("Explorer");

            // Open the Dependancy Window twice
            for(int openCount = 0; openCount < 2; openCount++)
            {
                DockManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            }

            string activeTab = TabManagerUIMap.GetActiveTabName();
            if(activeTab == "Base64ToString")
            {
                Assert.Fail("Opening the Dependency View twice should keep the UI on the same tab");
            }
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("UITest")]
        [Description("for bug 9802 - Foreach drill down test (2013.06.28)")]
        [Owner("Ashley")]
        public void DragAMultiAssignIntoAndOutOfAForEach_NoDrillDown()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 50);

            // Drag a ForEach onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach"); // ForEach
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Get a multiassign, and drag it onto the "Drop Activity Here" part of the ForEach box
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // pause for drill down...
            Playback.Wait(5000);

            // after pause check if start node is visible
            theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Assert.IsTrue(theStartButton.Exists, "Dropping a multiassign onto a foreach drilled down");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [TestCategory("UITest")]
        [Owner("Tshepo Ntlhokoa")]
        public void DragAStartNodeOntoATool_HoverOverAToolForAWhile_NoDrillDownShouldHappen()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            var workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an assign onto the Workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            //Drag Start Node
            Mouse.StartDragging(theStartButton, MouseButtons.Left);
            UITestControl assign = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Assign");
            var point = new Point(assign.BoundingRectangle.X + 150, assign.BoundingRectangle.Y + 50);
            //Hover over the multi assign for 5 seconds
            Mouse.Move(point);
            Playback.Wait(5000);

            //Get the active tab and compare against the original tab
            UITestControl activeTab = TabManagerUIMap.GetActiveTab();
            Assert.AreEqual(theTab, activeTab);
        }
    }
}
