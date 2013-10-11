using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest, System.Runtime.InteropServices.GuidAttribute("7E6836ED-8C14-4BFD-ADD0-3C5C6F0CB815")]
    public class StudioBugTests
    {
        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        public void DoCleanup(string workflowName, bool clickNo = false)
        {
            try
            {
                // Test complete - Delete itself  
                if(clickNo)
                {
                    TabManagerUIMap.CloseTab_Click_No(workflowName);
                }
                else
                {
                    TabManagerUIMap.CloseTab(workflowName);
                }
            }
            catch(Exception e)
            {
                // Log it so the UI Test still passes...
                Trace.WriteLine(e.Message);
            }

        }

        #region Test
        // Bug 6501
        [TestMethod][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void DeleteFirstDatagridRow_Expected_RowIsNotDeleted()
        {

            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
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

            // Cleanup! \o/
            // All good - Cleanup time!
            new TestBase().DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }


        //2013.05.29: Ashley Lewis for bug 9455 - Dont allow copy paste workflow xaml to another workflow
        [TestMethod][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
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
        [TestMethod][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void AddSecondServiceToWorkFlowExpectedDisplayTitleNotDsfActivity()
        {
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);

            DocManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 150));

            DocManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", new Point(startButton.BoundingRectangle.X + 50, startButton.BoundingRectangle.Y + 300));

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();

            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "DsfActivity(DsfActivityDesigner)"), "Dropped services display title was 'DsfActivity' rather than the name of the service");
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("for bug 9717 - copy paste multiple decisions (2013.06.22)")]
        [Owner("Ashley")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void CopyDecisionsWithContextMenuAndPasteExpectedNoWizardsDisplayed()
        {
            //Initialize
            Clipboard.SetText(" ");
            RibbonUIMap.CreateNewWorkflow();
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl startButton = WorkflowDesignerUIMap.FindStartNode(theTab);
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            //Drag on two decisions
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            if (WizardsUIMap.WaitForWizard(5000))
            {
                Playback.Wait(2000);
                _decisionWizardUiMap.HitDoneWithKeyboard();
            }
            else
            {
                Assert.Fail("Decision wizard not shown within the given timeout");
            }
            var newPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            newPoint.Y = newPoint.Y + 200;
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, newPoint);
            if(WizardsUIMap.WaitForWizard(5000))
            {
                Playback.Wait(2000);
                _decisionWizardUiMap.HitDoneWithKeyboard();
            }
            else
            {
                Assert.Fail("Decision wizard not shown within the given timeout");
            }
            //Rubberband select them
            var startDragPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            startDragPoint.X = startDragPoint.X - 100;
            startDragPoint.Y = startDragPoint.Y - 100;
            Mouse.Move(startDragPoint);
            newPoint.X = newPoint.X + 100;
            newPoint.Y = newPoint.Y + 100;
            Mouse.StartDragging();
            Mouse.StopDragging(newPoint);
            startDragPoint.X = startDragPoint.X + 110;
            startDragPoint.Y = startDragPoint.Y + 110;
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, startDragPoint);
            var designSurface = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            SendKeys.SendWait("{DOWN}{DOWN}{ENTER}");
            Mouse.Click(designSurface);
            SendKeys.SendWait("^v");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            Assert.AreEqual("System Menu Bar", uIItemImage.FriendlyName);
            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test for 'All Tools' workflow: The workflow is openned. The icons must display. The tab must be able to close again")]
        [Owner("Ashley")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        // ReSharper disable InconsistentNaming
        public void StudioTooling_StudioToolingUITest_CanToolsDisplay_NoExceptionsThrown()
        // ReSharper restore InconsistentNaming
        {
            // Open the Explorer
            DocManagerUIMap.ClickOpenTabPage("Explorer");
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

            DoCleanup("AllTools", true);

            Assert.IsTrue(true, "Studio was terminated or hung while opening and closing the all tools workflow");
        }

        [TestMethod]
        [TestCategory("Toolbox_Icons")]
        [Description("Toolbox icons display")]
        [Owner("Ashley Lewis")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        // ReSharper disable InconsistentNaming
        public void Toolbox_UITest_OpenToolbox_IconsAreDisplayed()
        // ReSharper restore InconsistentNaming
        {
            RibbonUIMap.CreateNewWorkflow();
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
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
        [TestMethod]
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
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            var thePoint = new Point(theStartButton.BoundingRectangle.X + 30, theStartButton.BoundingRectangle.Y + 100);
            ToolboxUIMap.DragControlToWorkflowDesigner("MultiAssign", thePoint);
            WorkflowDesignerUIMap.Adorner_ClickDoneButton(theTab, "MultiAssign");

            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "MultiAssign", 0);

            //Set up multi assign
            SendKeys.SendWait("[[AssignThis]]{TAB}Some Data");

            //issue with debug not showing up - run until debug output comes through
            WorkflowDesignerUIMap.RunWorkflowUntilOutputStepCountAtLeast(2, 5);

            //Click step
            DocManagerUIMap.ClickOpenTabPage("Output");
            Playback.Wait(100);
            var step = OutputUIMap.GetOutputWindow();
            Mouse.Click(step[2]);
            Mouse.Click(step[1]);

            //Assert the design surface activity is highlighted
            var workflow = WorkflowDesignerUIMap.GetFlowchartDesigner(theTab);
            Assert.IsTrue(WorkflowDesignerUIMap.IsControlSelected(workflow), "Selecting a step in the debug output does not select the activity on the design surface");

            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        [TestMethod]
        [TestCategory("UnsavedWorkflows_UITest")]
        [Description("For bug 10086 - Switching tabs does not flicker unsaved status")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void Tabs_UnsavedStar_SwitchingTabs_DoesNotChangeUnsavedStatus()
        // ReSharper restore InconsistentNaming
        {
            var firstName = "Test" + Guid.NewGuid().ToString().Substring(24);
            var secondName = "Test" + Guid.NewGuid().ToString().Substring(24);
            // Create first workflow
            RibbonUIMap.CreateNewWorkflow();
            var theTab = TabManagerUIMap.GetActiveTab();
            var theStartNode = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "StartSymbol");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("PBI_9957_UITEST");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "BUGS", "PBI_9957_UITEST",
                new Point(theStartNode.BoundingRectangle.X + 20,
                            theStartNode.BoundingRectangle.Y + 100));
            RibbonUIMap.ClickRibbonMenuItem("Save");
            if(WizardsUIMap.WaitForWizard(5000))
            {
                SaveDialogUIMap.ClickAndTypeInNameTextbox(firstName);
                Playback.Wait(1000);
            }
            else
            {
                Assert.Fail("Save dialog did not display with the given timeout period");
            }

            // Create second workflow
            RibbonUIMap.CreateNewWorkflow();
            theTab = TabManagerUIMap.GetActiveTab();
            theStartNode = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "StartSymbol");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindToolboxItemByAutomationId("MultiAssign"),
                new Point(theStartNode.BoundingRectangle.X + 20,
                            theStartNode.BoundingRectangle.Y + 100));
            RibbonUIMap.ClickRibbonMenuItem("Save");
            if(WizardsUIMap.WaitForWizard(5000))
            {
                SaveDialogUIMap.ClickAndTypeInNameTextbox(secondName);
                Playback.Wait(1000);
            }
            else
            {
                Assert.Fail("Save dialog did not display with the given timeout period");
            }

            // Switch between tabs ensuring the star is never added to their name
            UITestControl tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(secondName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Click(TabManagerUIMap.FindTabByName(secondName));
            tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(firstName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Click(TabManagerUIMap.FindTabByName(firstName));
            tryGetTab = null;
            tryGetTab = TabManagerUIMap.FindTabByName(secondName);
            Assert.IsNotNull(tryGetTab, "Tab has a star after it's name even though it was not altered");
            Mouse.Click(TabManagerUIMap.FindTabByName(secondName));

            // Test Cleanup
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("RenameResource_WithDashes")]
        public void RenameResource_WithDashes_ResourceRenamed()
        {
            TabManagerUIMap.CloseAllTabs();
            const string newTestResourceWithDashes = "New-Test-Resource-With-Dashes";
            const string oldResourceName = "OldResourceName";
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes))
            {
                ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            }
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            if(ExplorerUIMap.ServiceExists("Localhost", "WORKFLOWS", "Unassigned", oldResourceName))
            {
                ExplorerUIMap.RightClickDeleteProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            }
            RibbonUIMap.CreateNewWorkflow();
            SendKeys.SendWait("^s");
            if(WizardsUIMap.WaitForWizard(5000))
            {
                SaveDialogUIMap.ClickAndTypeInNameTextbox(oldResourceName);
            }
            else
            {
                Assert.Fail("Save wizard did not display in the given time period");
            }
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(oldResourceName);
            ExplorerUIMap.RightClickRenameProject("Localhost", "WORKFLOWS", "Unassigned", oldResourceName);
            SendKeys.SendWait("New-Test-Resource-With-Dashes{ENTER}");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(newTestResourceWithDashes);
            ExplorerUIMap.DoubleClickOpenProject("Localhost", "WORKFLOWS", "Unassigned", newTestResourceWithDashes);
            SendKeys.SendWait("^s");

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            if (DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F5}");
                Playback.Wait(1000);
            }
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion Test


        #region Depecated Test

        // Bug 6180
        [TestMethod]
        // Deploy Rework
        public void MakeSureDeployedItemsAreNotFiltered()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Base64ToString");
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            // Wait for it to open!
            Thread.Sleep(5000);
            UITestControl theTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            TabManagerUIMap.Click(theTab);
            DeployViewUIMap.EnterTextInSourceServerFilterBox(theTab, "ldnslgnsdg"); // Random text
            if(!DeployViewUIMap.DoesSourceServerHaveDeployItems(theTab))
            {
                Assert.Fail("The deployed item has been removed with the filter - It should not be");
            }
        }

        // Bug 6617
        [TestMethod][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void OpeningDependancyWindowTwiceKeepsItOpen()
        {
            // The workflow so we have a second tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Base64ToString");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Open the Dependancy Window twice
            for(int openCount = 0; openCount < 2; openCount++)
            {
                DocManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            }

            string activeTab = TabManagerUIMap.GetActiveTabName();
            if(activeTab == "Base64ToString")
            {
                Assert.Fail("Opening the Dependency View twice should keep the UI on the same tab");
            }
        }

        // Bug 8816
        [TestMethod]
        public void IsDeployButtonEnabledWithNothingToDeploy_Expected_DeployButtonIsDisabled()
        {
            // Click the Deploy button in the Ribbon
            RibbonUIMap.ClickRibbonMenuItem("Deploy");
            Playback.Wait(2000);

            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy");

            // Make sure the Deploy button is disabled
            Assert.IsTrue(!DeployViewUIMap.IsDeployButtonEnabled(deployTab));

            // Connect to a Destination Server
            DeployViewUIMap.ChooseDestinationServer(deployTab, "localhost");

            // Make sure its still disabled, as nothing has been chosen to deploy
            Assert.IsTrue(!DeployViewUIMap.IsDeployButtonEnabled(deployTab), "As we have not chosen anything to deploy, the Deploy button should still be disabled!");
        }

        // Bug 8819
        [TestMethod]
        public void EnterFilterOnDestinationServer_Expected_DeployedItemsStillVisible()
        {
            // Choose to deploy one of our own items
            //ExplorerUIMap.DoRefresh();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Set ourself as the destination server
            UITestControl deployTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            DeployViewUIMap.ChooseDestinationServer(deployTab, "localhost");

            // Make sure the Destination server has items
            Assert.IsTrue(DeployViewUIMap.DoesDestinationServerHaveItems(deployTab));

            // Enter a filter in the destination server
            DeployViewUIMap.EnterTextInDestinationServerFilterBox(deployTab, "zzzzzzzzz");

            // And make sure it still has items
            Assert.IsTrue(DeployViewUIMap.DoesDestinationServerHaveItems(deployTab), "After a filter was applied, the destination Server lost all its items!");
        }

        [TestMethod]
        [TestCategory("UITest")]
        [Description("for bug 9802 - Foreach drill down test (2013.06.28)")]
        [Owner("Ashley")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void DragAMultiAssignIntoAndOutOfAForEach_NoDrillDown()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            requiredPoint.Offset(20, 50);

            // Drag a ForEach onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach"); // ForEach
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Get a multiassign, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // pause for drill down...
            Playback.Wait(5000);

            // after pause check if start node is visible
            theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Assert.IsTrue(theStartButton.Exists, "Dropping a multiassign onto a foreach drilled down");

            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        #endregion


        private int GetInstanceUnderParent(UITestControl control)
        {
            UITestControl parent = control.GetParent();
            UITestControlCollection col = parent.GetChildren();
            int index = 1;

            foreach(UITestControl child in col)
            {
                if(child.Equals(control))
                {
                    break;
                }

                if(child.ControlType == control.ControlType)
                {
                    index++;
                }
            }
            return index;
        }

        #region Additional test attributes

        // You can use the following additional attributes as you write your tests:

        ////Use TestInitialize to run code before running each test 
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        ////Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{        
        //    // To generate code for this test, select "Generate Code for Coded UI Test" from the shortcut menu and select one of the menu items.
        //    // For more information on generated code, see http://go.microsoft.com/fwlink/?LinkId=179463
        //}

        #endregion

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }
        private TestContext _testContextInstance;

        #region UI Maps

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if(_ribbonMap == null)
                {
                    _ribbonMap = new RibbonUIMap();
                }

                return _ribbonMap;
            }
        }

        private RibbonUIMap _ribbonMap;

        #endregion

        #region DocManager UI Map

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if((_docManagerMap == null))
                {
                    _docManagerMap = new DocManagerUIMap();
                }

                return _docManagerMap;
            }
        }

        private DocManagerUIMap _docManagerMap;

        #endregion

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if((_toolboxUIMap == null))
                {
                    _toolboxUIMap = new ToolboxUIMap();
                }

                return _toolboxUIMap;
            }
        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion

        #region Explorer UI Map

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if((_explorerUIMap == null))
                {
                    _explorerUIMap = new ExplorerUIMap();
                }

                return _explorerUIMap;
            }
        }

        private ExplorerUIMap _explorerUIMap;

        #endregion

        #region DependencyGraph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if((DependencyGraphUIMap == null))
                {
                    DependencyGraphUIMap = new DependencyGraph();
                }

                return DependencyGraphUIMap;
            }
            set { throw new NotImplementedException(); }
        }

        private DependencyGraph _dependencyGraphUIMap;

        #endregion

        #region DeployView UI Map

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if((_deployViewUIMap == null))
                {
                    _deployViewUIMap = new DeployViewUIMap();
                }

                return _deployViewUIMap;
            }
        }

        private DeployViewUIMap _deployViewUIMap;

        #endregion

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if(_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }

        private TabManagerUIMap _tabManagerUIMap;

        #endregion TabManager UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if(_workflowDesignerUIMap == null)
                {
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map

        #region WorkflowWizard UI Map

        public WorkflowWizardUIMap WorkflowWizardUIMap
        {
            get
            {
                if(_workflowWizardUIMap == null)
                {
                    _workflowWizardUIMap = new WorkflowWizardUIMap();
                }

                return _workflowWizardUIMap;
            }
        }

        private WorkflowWizardUIMap _workflowWizardUIMap;

        #endregion WorkflowWizard UI Map

        #region Database Wizard UI Map

        public DatabaseServiceWizardUIMap DatabaseServiceWizardUIMap
        {
            get
            {
                if(_databaseServiceWizardUIMap == null)
                {
                    _databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return _databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap _databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Plugin Wizard UI Map

        public PluginServiceWizardUIMap PluginServiceWizardUIMap
        {
            get
            {
                if(_pluginServiceWizardUIMap == null)
                {
                    _pluginServiceWizardUIMap = new PluginServiceWizardUIMap();
                }

                return _pluginServiceWizardUIMap;
            }
        }

        private PluginServiceWizardUIMap _pluginServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Webpage Wizard UI Map

        public WebpageServiceWizardUIMap WebpageServiceWizardUIMap
        {
            get
            {
                if(_webpageServiceWizardUIMap == null)
                {
                    _webpageServiceWizardUIMap = new WebpageServiceWizardUIMap();
                }

                return _webpageServiceWizardUIMap;
            }
        }

        private WebpageServiceWizardUIMap _webpageServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region External UI Map

        public ExternalUIMap ExternalUIMap
        {
            get
            {
                if(_externalUIMap == null)
                {
                    _externalUIMap = new ExternalUIMap();
                }
                return _externalUIMap;
            }
        }

        private ExternalUIMap _externalUIMap;

        #endregion External Window UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
        {
            get
            {
                if(_variablesUIMap == null)
                {
                    _variablesUIMap = new VariablesUIMap();
                }
                return _variablesUIMap;
            }
        }

        private VariablesUIMap _variablesUIMap;

        #endregion Connect Window UI Map

        #region WorkflowDesigner UI Map

        public ResourceChangedPopUpUIMap ResourceChangedPopUpUIMap
        {
            get
            {
                if(_resourceChangedPopUpUIMap == null)
                {
                    _resourceChangedPopUpUIMap = new ResourceChangedPopUpUIMap();
                }

                return _resourceChangedPopUpUIMap;
            }
        }

        private ResourceChangedPopUpUIMap _resourceChangedPopUpUIMap;

        #endregion WorkflowDesigner UI Map

        #region DocManager UI Map

        private OutputUIMap _outputMap;

        #endregion

        #region map UI Map

        public UIMap UIMap
        {
            get
            {
                if((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;
        
        #endregion

        #region Save Dialog UI Map

        public SaveDialogUIMap SaveDialogUIMap
        {
            get
            {
                if((_saveDialogUIMap == null))
                {
                    _saveDialogUIMap = new SaveDialogUIMap();
                }

                return _saveDialogUIMap;
            }
        }

        private SaveDialogUIMap _saveDialogUIMap;

        #endregion

        #region Save Dialog UI Map

        public DebugUIMap DebugUIMap
        {
            get
            {
                if((_debugUIMap == null))
                {
                    _debugUIMap = new DebugUIMap();
                }

                return _debugUIMap;
            }
        }

        private DebugUIMap _debugUIMap;

        #endregion

        #endregion UI Maps
    }
}
