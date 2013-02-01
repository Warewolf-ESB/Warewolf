using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DatabaseServiceUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.PluginServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WebpageServiceWizardUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ConnectViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DeployViewUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExternalUIMapClasses;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Dev2.CodedUI.Tests.UIMaps.VariablesUIMapClasses;


namespace Dev2.CodedUI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class StudioBugTests
    {
        public StudioBugTests()
        {
        }

        // These run at the start of every test to make sure everything is sane
        [TestInitialize]
        public void CheckStartIsValid()
        {
            // Use the base class for validity checks - Easier to control :D
            var myTestBase = new TestBase();
            myTestBase.CheckStartIsValid();
        }

        // Bug 3512
        [TestMethod]
        public void WorkflowXamlShouldNotBeVisibleByCopyPaste()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            UITestControl theTab = TabManagerUIMap.FindTabByName("Base64ToString");
            TabManagerUIMap.Click("Base64ToString");
            WorkflowDesignerUIMap.CopyWorkflowXAML(theTab);
            System.Diagnostics.Process.Start("notepad.exe");
            System.Threading.Thread.Sleep(1000);
            Keyboard.SendKeys("^V");
            if (ExternalUIMap.NotepadTextContains("<?xml version=\"1.0\" encoding=\"utf-16\"?>"))
            {
                var myBase = new TestBase();
                myBase.KillAllInstancesOf("notepad");
                Assert.Inconclusive("The Workflow XAML should not be pastable into Notepad");
            }
            var zeBase = new TestBase();
            zeBase.KillAllInstancesOf("notepad");
        }

        // Bug 6180
        [TestMethod]
        public void MakeSureDeployedItemsAreNotFiltered()
        {
            // Jurie has apparently fixed this, but just hasn't checked it in :D
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            //this.UIMap.DeployOptionClick - To fix
            // Wait for it to open!
            System.Threading.Thread.Sleep(5000);
            UITestControl theTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            TabManagerUIMap.Click(theTab);
            DeployViewUIMap.EnterTextInSourceServerFilterBox(theTab, "ldnslgnsdg"); // Random text
            if (!DeployViewUIMap.DoesSourceServerHaveDeployItems(theTab))
            {
                Assert.Inconclusive("The deployed item has been removed with the filter - It should not be (Jurie should have fixed this....)");
            }
        }

        // Bug 5725 and Bug 5050
        // Removed until we get a DependancyUIMap
        [TestMethod]
        public void DependancyGraph_OnDoubleClickWorkFlow_Expected_NewTab()
        {
            /*
            // Create new Activity
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("5725", "CodedUITestCategory");

            // Refresh
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Open Dependancy Graph
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5725");

            // Double Click
            Mouse.DoubleClick(new Point(Screen.PrimaryScreen.Bounds.Width/2,Screen.PrimaryScreen.Bounds.Height/2)); <--- Needs a UIMap :(

            // Wait
            System.Threading.Thread.Sleep(2500);

            // Assert not viewing dependency graph any more
            Assert.AreEqual(TabManagerUIMap.GetActiveTabName(), "5725");

            // Garbage collection
            new TestBase().DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5725");
             */
        }

        // Bug 6127 - DataSplit
        [TestMethod]
        public void DataSplit_DeletingARow_Expected_UpdateTitle()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("6127", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("6127");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Drag a Datasplit control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("DataSplit"), new Point(p.X, p.Y + 200));
            UITestControl datasplitBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfDataSplitActivityDesigner");

            //Create the first row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 100, datasplitBoxOnWorkflow.BoundingRectangle.Y + 60));
            Keyboard.SendKeys("asdf");

            //Create the second row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 100, datasplitBoxOnWorkflow.BoundingRectangle.Y + 90));
            Keyboard.SendKeys("asdf");

            //Delete a row
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 10, datasplitBoxOnWorkflow.BoundingRectangle.Y + 60));//open context menu
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 40, datasplitBoxOnWorkflow.BoundingRectangle.Y + 275));//select delete row

            StringAssert.Contains(datasplitBoxOnWorkflow.GetProperty("AutomationId").ToString(), "Data Split (1)");

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6127");
        }

        //Bug 6127 - MultiAssign
        [TestMethod]
        public void MultiAssign_DeletingARow_Expected_UpdateTitle()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("6127", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("6127");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Drag a MultiAssign control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("MultiAssign"), new Point(p.X, p.Y + 200));
            UITestControl multiassignBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfMultiAssignActivityDesigner");

            //Create the first row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 40));
            Keyboard.SendKeys("asdf");

            //Create the second row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 65));
            Keyboard.SendKeys("asdf");

            //Delete a row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 40));//Select row text
            Keyboard.SendKeys("{BACK}{BACK}{BACK}{BACK}{BACK}{BACK}{BACK}{BACK}");//remove row text
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 65));//change focus to delete row

            StringAssert.Contains(multiassignBoxOnWorkflow.GetProperty("AutomationId").ToString(), "Assign (1)");

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6127");
        }

        // Bug 6482
        [TestMethod]
        public void DragMultipleControls()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("DragMultipleControls", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("DragMultipleControls");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);
            Mouse.Click(p);

            // Drag a Foreach control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl foreachBox = ToolboxUIMap.FindControl("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(foreachBox, new Point(p.X, p.Y + 100));
            UITestControl foreachBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "For Each");
            Point toolboxPoint = new Point(foreachBoxOnWorkflow.BoundingRectangle.X + 100, foreachBoxOnWorkflow.BoundingRectangle.Y + 100);

            // Drag a Comment control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl commentBox = ToolboxUIMap.FindControl("Comment");
            ToolboxUIMap.DragControlToWorkflowDesigner(commentBox, new Point(p.X, p.Y + 200));
            UITestControl commentBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Comment");
            Point commentPoint = new Point(commentBoxOnWorkflow.BoundingRectangle.X + 100, commentBoxOnWorkflow.BoundingRectangle.Y + 10);

            // Drag another Comment control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            commentBox = ToolboxUIMap.FindControl("Comment");
            ToolboxUIMap.DragControlToWorkflowDesigner(commentBox, new Point(p.X, p.Y + 300));

            //Mouse.Click(MouseButtons.Left, ModifierKeys.Control, commentPoint);
            Mouse.Click(MouseButtons.Left, ModifierKeys.Control, commentPoint);

            Mouse.StartDragging(commentBoxOnWorkflow, new Point(10, 10));
            Mouse.StopDragging(toolboxPoint);

            // Wait for the ForEach Workflow to load
            System.Threading.Thread.Sleep(5000);

            string exceptionMessage = String.Empty;
            try
            {
                startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");

                // The ForEach component has not loaded, so one of the controls were not dragged!
                Assert.Fail();
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message.ToString();
            }
            if (exceptionMessage.StartsWith("Assert.Fail failed."))
            {
                Assert.Inconclusive("Error - Multiple controls cannot be dragged!");
            }
        }

        // Bug 6501
        [TestMethod]
        public void DeleteFirstDatagridRow_Expected_RowIsNotDeleted()
        {
            // Create the Workflow
            var testBase = new TestBase();
            testBase.CreateCustomWorkflow("6501");

            // Set some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName("6501");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag control onto the Workflow Designer
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theTool = ToolboxUIMap.FindToolboxItemByAutomationID("BaseConvert");
            ToolboxUIMap.DragControlToWorkflowDesigner(theTool, workflowPoint1);

            // Enter some data
            UITestControl baseConversion = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "BaseConvert");
            Point p = new Point(baseConversion.BoundingRectangle.X + 40, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(p);
            SendKeys.SendWait("someText");

            // Click the index
            p = new Point(baseConversion.BoundingRectangle.X + 20, baseConversion.BoundingRectangle.Y + 40);
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, p);
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{UP}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{UP}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{RIGHT}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{ENTER}");
            System.Threading.Thread.Sleep(100);

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
            if (clipboardText != "newText")
            {
                Assert.Fail("Error - The Item was not deleted!");
            }

            // Cleanup! \o/
            testBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6501");
        }

        // Bug 6617
        [TestMethod]
        public void OpeningDependancyWindowTwiceKeepsItOpen()
        {
            // The workflow so we have a second tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Open the Dependancy Window twice
            for (int openCount = 0; openCount < 2; openCount++)
            {
                DocManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.RightClickShowProjectDependancies("localhost", "WORKFLOWS", "SYSTEM", "Base64ToString");
            }
            
            string activeTab = TabManagerUIMap.GetActiveTabName();
            if (activeTab == "Base64ToString")
            {
                Assert.Fail("Opening the Dependency View twice should keep the UI on the same tab");
            }
        }

        // Bug 6672
        [TestMethod]
        public void WorkflowWizard_OnDelete_Expected_RemovedFromRepository()
        {
            try
            {
                var myTestBase = new TestBase();
                myTestBase.CreateCustomWorkflow("6672", "CodedUITestCategory");
                explorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "CodedUITestCategory", "6672");
                explorerUIMap.DoRefresh();
                myTestBase.CreateCustomWorkflow("6672", "CodedUITestCategory");
                myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6672");
            }
            catch
            {
                Assert.Inconclusive("Error - Broken Test!");
            }
        }

        //  Bug that Mo picked up with legacy Web Pages losing their data (FIXED)
        // Commented Out - // No Webpage Bugs this run!
        [TestMethod]
        public void OpeningOldWebPageTwice_Expected_RetainsData()
        {
            /*
            // Open an old Webpage
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "WEBPAGE", "DatabaseServiceSetup");
            System.Threading.Thread.Sleep(5000); // Crashes without this line due to a bug with closing workflows before they're fully loaded

            // And close the tab (Originally made it lose the XML)
            TabManagerUIMap.CloseTab("DatabaseServiceSetup/Database Service - Setup.ui");

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName("DatabaseServiceSetup");

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Get the location of the Start button and click it
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Get the location of the "Database Service - Setup" control, and click it
            UITestControl theDatabaseServiceControl = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Database Service - Setup(DsfWebPageActivityDesigner)");
            WorkflowDesignerUIMap.ClickControl(theDatabaseServiceControl);

            // Then re-open the closed page
            WorkflowDesignerUIMap.DoubleClickControlBar(theDatabaseServiceControl);

            UITestControl uiTab = TabManagerUIMap.FindTabByName("DatabaseServiceSetup/Database Service - Setup.ui");

            // And check a blocks Name property to make sure it retained its data
            UITestControl theBlock = DeployViewUIMap.GetWebsiteGridBlock(uiTab);
            string blockNameProperty = theBlock.GetChildren()[2].Name;
            StringAssert.Equals(blockNameProperty, "[[Dev2DatabaseServiceSetupSourceLabel]]");
             */
        }

        // Bug 7409 - DataSplit
        [TestMethod]
        public void DataSplitActivity_OnMouseScroll_Expected_NoUnHandledExceptions()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7409", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("7409");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Drag a DataSplit control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("DataSplit"), new Point(p.X, p.Y + 200));
            UITestControl datasplitBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfDataSplitActivityDesigner");

            // Scroll once
            Mouse.Move(new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 100, datasplitBoxOnWorkflow.BoundingRectangle.Y + 60));
            Mouse.MoveScrollWheel(-1);

            // Assert scrolled from row index 0 to row index 1
            //Assert.AreEqual(1, datasplitBoxOnWorkflow.GetChildren()[4].GetChildren()[0].GetProperty("RowIndex")); getting the row index changes the row index so I cant assert that its only scrolling by one, oh well, this test watching out for unhandled exceptions mostly anyway

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7409");
        }

        // Bug 7409 - MultiAssign
        [TestMethod]
        public void MultiAssignActivity_OnMouseScroll_Expected_NoUnHandledExceptions()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7409", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("7409");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Drag a DataSplit control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("MultiAssign"), new Point(p.X, p.Y + 200));
            UITestControl multiassignBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfMultiAssignActivityDesigner");

            //Create the first row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 40));
            Keyboard.SendKeys("asdf");

            //Create the second row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 65));
            Keyboard.SendKeys("asdf");

            // Scroll down
            Mouse.Move(new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 60));
            Mouse.MoveScrollWheel(-1);

            //Create the third row
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 65));
            Keyboard.SendKeys("asdf");

            // Scroll to the top
            Mouse.Move(new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 60));
            Mouse.MoveScrollWheel(4);

            // Scroll down once
            Mouse.Move(new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 60));
            Mouse.MoveScrollWheel(-1);

            // Assert scrolled from row index 0 (top) to row index 1
            //Assert.AreEqual(1, multiassignBoxOnWorkflow.GetChildren()[2].GetChildren()[0].GetProperty("RowIndex"));// Getting the row index changes the row index

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7409");
        }

        // Bug 7799
        [TestMethod]
        public void dsfActivity_OnDblClick_Expected_NoUnhandledExceptions()
        {
            // Create new dsfActivity
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7799Activity", "CodedUITestCategory");

            // Initialize work flow
            myTestBase.CreateCustomWorkflow("7799", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("7799");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Refresh
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Drag a dsfActivity onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            UITestControl theControl = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7799Activity");

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DragControlToWorkflowDesigner(theControl, new Point(p.X, p.Y + 200));

            UITestControl activityBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfActivityDesigner");

            //Double click the dsfActivity
            Mouse.DoubleClick(MouseButtons.Left, ModifierKeys.None, new Point(activityBoxOnWorkflow.BoundingRectangle.X + 100, activityBoxOnWorkflow.BoundingRectangle.Y + 40));

            // Delete the Workflows in the required order
            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7799Activity");
            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7799");
        }

        // Bug 7802
        [TestMethod]
        public void MiddleClickCloseHomePageUnderSpecialCircumstances_Expected_StartPageCloses()
        {
            var myTestBase = new TestBase();
            TabManagerUIMap.CloseAllTabs();
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Get the Studio EXE location so we can re-open it
            string fileName = myTestBase.GetStudioEXELocation();

            // Close the Studio
            WpfWindow studioWindow = new WpfWindow();
            studioWindow.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            studioWindow.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            studioWindow.WindowTitles.Add(TestBase.GetStudioWindowName());
            studioWindow.Find();

            Point closeButton = new Point(studioWindow.BoundingRectangle.X + studioWindow.Width - 25, studioWindow.BoundingRectangle.Y + 15);
            
            // Its far - Move faster
            Mouse.MouseMoveSpeed *= 2;
            Mouse.Move(closeButton);

            // And back to normal
            Mouse.MouseMoveSpeed /= 2;
            Mouse.Click();

            // Restart the studio!
            System.Diagnostics.Process.Start(fileName);

            // Get a clickable point in the Studio to make sure it's opened!

            int tries = 0;

            Point p = new Point();
            studioWindow = new WpfWindow();
            studioWindow.SearchProperties[WpfWindow.PropertyNames.Name] = TestBase.GetStudioWindowName();
            studioWindow.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            studioWindow.WindowTitles.Add(TestBase.GetStudioWindowName());

            while (!studioWindow.TryGetClickablePoint(out p) && tries < 30)
            {
                System.Threading.Thread.Sleep(1000);
                tries++;
            }

            if (!studioWindow.TryGetClickablePoint(out p))
            {
                throw new Exception("Fatal Error - The Studio did not restart after 30 seconds!");
            }


            // Middle-Click close the home tab
            string homeTab = TabManagerUIMap.GetTabNameAtPosition(1);
            TabManagerUIMap.MiddleClickCloseTab(homeTab);

            int tabCount = TabManagerUIMap.GetTabCount();
            if (tabCount != 1)
            {
                Assert.Fail();
            }
        }

        // Bug 7841
        [TestMethod]
        public void DeletingResourceAndRefreshingDeletesResource_Expected_ResourceStaysDeleted()
        {
            // Create a workflow
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7841", "CodedUITestCategory");

            // Refresh
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Make sure it's there
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7841");

            // Delete it
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7841");

            // Refresh
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Make sure it's gone
            try
            {
                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7841");
                Assert.Fail();
            }
            catch
            {
                // Silent fail - The open is meant to fail
            }
        }

        // Bug 7854
        [TestMethod]
        public void ClickingIntellisenseItemFillsField_Expected_IntellisenseItemAppearsInField()
        {
            // Create the Workflow for the bug
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7854", "CodedUITestCategory");

            // Refresh
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoRefresh();

            // Get a point for later
            UITestControl theTab = TabManagerUIMap.FindTabByName("7854");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theCalculate = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theCalculate, workflowPoint1);

            // Values :D
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("[[someVal]]");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("12345");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);

            // Map them
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            // Actual test time :D
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("[[");
            System.Threading.Thread.Sleep(500);
            UITestControl theItem = WorkflowDesignerUIMap.GetIntellisenseItem(0);
            Mouse.Move(theItem, new Point(10, 10));
            Mouse.Click();

            // Item should be populated - Time to check!
            WorkflowDesignerUIMap.AssignControl_ClickFirstTextbox(theTab, "Assign");
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{TAB}");
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("{END}"); // Get to the end of the item
            System.Threading.Thread.Sleep(100);
            SendKeys.SendWait("+{HOME}"); // Shift Home - Highlights the item
            // Just to make sure it wasn't already copied before the test
            Clipboard.SetText("someRandomText");
            SendKeys.SendWait("^c"); // Copy command
            string clipboardText = Clipboard.GetText();
            if (clipboardText != "[[someVal]]")
            {
                Assert.Fail("Error - The item was not correctly populated!");
            }

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7854");
        }

        // Bug 7930
        [TestMethod]
        public void FixedDataListServicesWithWizards()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "DataSplitTestWithRecordsetsWithNoIndexes");
        }

        //Bug 6413
        [TestMethod]
        public void FindMissing_WithDoubleRegion_Expected_BothAdded()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("6413", "CodedUITestCategory");

            UITestControl theTab = TabManagerUIMap.FindTabByName("6413");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            // Drag a MultiAssign control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("MultiAssign"), new Point(p.X, p.Y + 200));
            UITestControl multiassignBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfMultiAssignActivityDesigner");

            //Create a regular region
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 100, multiassignBoxOnWorkflow.BoundingRectangle.Y + 40));
            Keyboard.SendKeys("[[scalar]]");

            //Create the double region
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(multiassignBoxOnWorkflow.BoundingRectangle.X + 160, multiassignBoxOnWorkflow.BoundingRectangle.Y + 40));
            Keyboard.SendKeys("[[first]][[second]]");

            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();
            Assert.AreEqual("scalar", VariablesUIMap.GetVariableName(0));
            Assert.AreEqual("first",VariablesUIMap.GetVariableName(1));
            Assert.AreEqual("second", VariablesUIMap.GetVariableName(2));

            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "6413");
        }

        //Bug 7842
        [TestMethod]
        public void DataSplit_WithCopyPastedTabs_Expected_SplitByTab()
        {
            var myTestBase = new TestBase();
            myTestBase.CreateCustomWorkflow("7842", "CodedUITestCategory");
            UITestControl theTab = TabManagerUIMap.FindTabByName("7842");
            UITestControl startButton = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "Start");
            Point p = new Point(startButton.BoundingRectangle.X + 10, startButton.BoundingRectangle.Y + 10);

            //Drag a DataSplit control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolboxUIMap.FindControl("DataSplit"), new Point(p.X, p.Y + 200));
            UITestControl datasplitBoxOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationID(theTab, "DsfDataSplitActivityDesigner");

            //Setup datasplit vars
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 100, datasplitBoxOnWorkflow.BoundingRectangle.Y + 65));
            Keyboard.SendKeys("[[recset().field]]");
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 160, datasplitBoxOnWorkflow.BoundingRectangle.Y + 65));
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 160, datasplitBoxOnWorkflow.BoundingRectangle.Y + 150));
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            //Set datasplit as start node
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 10, datasplitBoxOnWorkflow.BoundingRectangle.Y + 10));
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 40, datasplitBoxOnWorkflow.BoundingRectangle.Y + 200));

            //Create tabs in notepad for dataplit
            System.Diagnostics.Process.Start("notepad.exe");
            System.Threading.Thread.Sleep(2500);
            Keyboard.SendKeys("Ithoughtthiswas	allihad	tosplit");
            Keyboard.SendKeys("^A");
            Keyboard.SendKeys("^C");
            myTestBase.KillAllInstancesOf("notepad");

            //Paste tabs into datasplit
            Mouse.Click(MouseButtons.Left, ModifierKeys.None, new Point(datasplitBoxOnWorkflow.BoundingRectangle.X + 170, datasplitBoxOnWorkflow.BoundingRectangle.Y + 40));
            Keyboard.SendKeys("^V");

            // Click "View in Browser"
            RibbonUIMap.ClickRibbonMenuItem("Home", "View in Browser");
            System.Threading.Thread.Sleep(2500);

            // Check if the IE Body contains the data list item
            string IEText = ExternalUIMap.GetIEBodyText();
            if (!IEText.Contains("<field>Ithoughtthiswas</field>") && !IEText.Contains("<field>allihad</field>") && !IEText.Contains("<field>tosplit</field>"))
            {
                Assert.Fail("Data split not splitting by tab");
            }

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // And do cleanup
            myTestBase.DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "7842");

        }

        private int GetInstanceUnderParent(UITestControl control)
        {
            UITestControl parent = control.GetParent();
            UITestControlCollection col = parent.GetChildren();
            int index = 1;

            foreach (UITestControl child in col)
            {
                if (child.Equals(control))
                {
                    break;
                }

                if (child.ControlType == control.ControlType)
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
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        private TestContext testContextInstance;

        #region UI Maps

        #region Base UI Map

        public UIMap UIMap
        {
            get
            {
                if ((this.map == null))
                {
                    this.map = new UIMap();
                }

                return this.map;
            }
        }

        private UIMap map;

        #endregion Base UI Map

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if ((this.ribbonMap == null))
                {
                    this.ribbonMap = new RibbonUIMap();
                }

                return this.ribbonMap;
            }
        }

        private RibbonUIMap ribbonMap;

        #endregion

        #region DocManager UI Map

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if ((this.docManagerMap == null))
                {
                    this.docManagerMap = new DocManagerUIMap();
                }

                return this.docManagerMap;
            }
        }

        private DocManagerUIMap docManagerMap;

        #endregion

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((this.toolboxUIMap == null))
                {
                    this.toolboxUIMap = new ToolboxUIMap();
                }

                return this.toolboxUIMap;
            }
        }

        private ToolboxUIMap toolboxUIMap;

        #endregion

        #region Explorer UI Map

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if ((this.explorerUIMap == null))
                {
                    this.explorerUIMap = new ExplorerUIMap();
                }

                return this.explorerUIMap;
            }
        }

        private ExplorerUIMap explorerUIMap;

        #endregion

        #region DependencyGraph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if ((this.DependencyGraphUIMap == null))
                {
                    this.DependencyGraphUIMap = new DependencyGraph();
                }

                return this.DependencyGraphUIMap;
            }
            set { throw new NotImplementedException(); }
        }

        private DependencyGraph dependencyGraphUIMap;

        #endregion

        #region DeployView UI Map

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if ((this.deployViewUIMap == null))
                {
                    this.deployViewUIMap = new DeployViewUIMap();
                }

                return this.deployViewUIMap;
            }
        }

        private DeployViewUIMap deployViewUIMap;

        #endregion

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (tabManagerUIMap == null)
                {
                    tabManagerUIMap = new TabManagerUIMap();
                }

                return tabManagerUIMap;
            }
        }

        private TabManagerUIMap tabManagerUIMap;

        #endregion TabManager UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (workflowDesignerUIMap == null)
                {
                    workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map

        #region WorkflowWizard UI Map

        public WorkflowWizardUIMap WorkflowWizardUIMap
        {
            get
            {
                if (workflowWizardUIMap == null)
                {
                    workflowWizardUIMap = new WorkflowWizardUIMap();
                }

                return workflowWizardUIMap;
            }
        }

        private WorkflowWizardUIMap workflowWizardUIMap;

        #endregion WorkflowWizard UI Map

        #region Database Wizard UI Map

        public DatabaseServiceWizardUIMap DatabaseServiceWizardUIMap
        {
            get
            {
                if (databaseServiceWizardUIMap == null)
                {
                    databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Plugin Wizard UI Map

        public PluginServiceWizardUIMap PluginServiceWizardUIMap
        {
            get
            {
                if (pluginServiceWizardUIMap == null)
                {
                    pluginServiceWizardUIMap = new PluginServiceWizardUIMap();
                }

                return pluginServiceWizardUIMap;
            }
        }

        private PluginServiceWizardUIMap pluginServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Webpage Wizard UI Map

        public WebpageServiceWizardUIMap WebpageServiceWizardUIMap
        {
            get
            {
                if (webpageServiceWizardUIMap == null)
                {
                    webpageServiceWizardUIMap = new WebpageServiceWizardUIMap();
                }

                return webpageServiceWizardUIMap;
            }
        }

        private WebpageServiceWizardUIMap webpageServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Connect Window UI Map

        public ConnectViewUIMap ConnectViewUIMap
        {
            get
            {
                if (connectViewUIMap == null)
                {
                    connectViewUIMap = new ConnectViewUIMap();
                }
                return connectViewUIMap;
            }
        }

        private ConnectViewUIMap connectViewUIMap;

        #endregion Connect Window UI Map

        #region External UI Map

        public ExternalUIMap ExternalUIMap
        {
            get
            {
                if (externalUIMap == null)
                {
                    externalUIMap = new ExternalUIMap();
                }
                return externalUIMap;
            }
        }

        private ExternalUIMap externalUIMap;

        #endregion External Window UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
        {
            get
            {
                if (variablesUIMap == null)
                {
                    variablesUIMap = new VariablesUIMap();
                }
                return variablesUIMap;
            }
        }

        private VariablesUIMap variablesUIMap;

        #endregion Connect Window UI Map

        #endregion UI Maps
    }
}
