using System.Globalization;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Point = System.Drawing.Point;

namespace Dev2.CodedUI.Tests
{
    [CodedUITest]
    public class TestBase : UIMapBase
    {
        public string ServerExeLocation;

        public static string GetStudioWindowName()
        {
            return "Warewolf";
        }

        #region New PBI Tests

        //PBI_8853
        [TestMethod]
        // 05/11 - Failure is Correct - Broken Functionality ;)
        public void NewWorkflowShortcutKeyExpectedWorkflowOpens()
        {
            StudioWindow.SetFocus();
            var preCount = TabManagerUIMap.GetTabCount();
            SendKeys.SendWait("^w");
            string activeTabName = TabManagerUIMap.GetActiveTabName();
            var postCount = TabManagerUIMap.GetTabCount();
            Assert.IsTrue(postCount == preCount + 1, "Tab quantity has not been increased");
            Assert.IsTrue(activeTabName.Contains("Unsaved"), "Active workflow is not an unsaved workflow");
        }

        [TestMethod]
        public void ClickNewWorkflowExpectedWorkflowOpens()
        {
            var preCount = TabManagerUIMap.GetTabCount();
            RibbonUIMap.ClickRibbonMenuItem("UI_RibbonHomeTabWorkflowBtn_AutoID");
            string activeTabName = TabManagerUIMap.GetActiveTabName();
            var postCount = TabManagerUIMap.GetTabCount();
            Assert.IsTrue(postCount == preCount + 1, "Tab quantity has not been increased");
            Assert.IsTrue(activeTabName.Contains("Unsaved"), "Active workflow is not an unsaved workflow");
        }

        #endregion New PBI Tests

        [TestMethod]
        public void AddLargeAmountsOfDataListItems_Expected_NoHanging()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            // moved from 100 to 20 for time
            for(int j = 0; j < 20; j++)
            {
                // Sleeps are due to the delay when adding a lot of items
                SendKeys.SendWait("[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                Playback.Wait(15);
                SendKeys.SendWait("{TAB}");
                Playback.Wait(15);
                SendKeys.SendWait(j.ToString(CultureInfo.InvariantCulture));
                Playback.Wait(15);
                SendKeys.SendWait("{TAB}");
                Playback.Wait(15);
            }

            Playback.Wait(500);
            var leftTextBoxInRowLastRow = WorkflowDesignerUIMap.AssignControl_GetLeftTextboxInRow("Assign", 19) as WpfEdit;
            string text = leftTextBoxInRowLastRow.Text;
            StringAssert.Contains(text, "[[theVar19]]");
        }

        ////PBI 9461
        [TestMethod]
        public void ChangingResourceExpectedPopUpWarningWithShowAffected()
        {
            // Open the workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("NewForeachUpgrade");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "NewForeachUpgradeDifferentExecutionTests");
            //Edit the inputs and outputs
            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.CheckScalarInputAndOuput(0);
            VariablesUIMap.CheckScalarInput(0);

            //Save the workflow
            RibbonUIMap.ClickRibbonMenuItem("Save");

            PopupDialogUIMap.WaitForDialog();

            //Click the show affected button
            ResourceChangedPopUpUIMap.ClickViewDependancies();

            Playback.Wait(5000);

            Assert.IsTrue(TabManagerUIMap.GetActiveTabName().Contains("ForEachUpgradeTest"), "Affected workflow not shown after show affected workflow button pressed.");
        }

        [TestMethod]
        public void UnsavedStar_UITest_WhenWorkflowIsChanged_ExpectStarIsShowing()
        {
            //------------Setup for test--------------------------
            RibbonUIMap.CreateNewWorkflow();
            // Get some data
            var tabName = TabManagerUIMap.GetActiveTabName();
            UITestControl theTab = TabManagerUIMap.FindTabByName(tabName);
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Multi Assign on
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl asssignControlInToolbox = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(asssignControlInToolbox, workflowPoint1);

            // Click away
            Mouse.Click(new Point(workflowPoint1.X + 50, workflowPoint1.Y + 50));

            //------------Execute Test---------------------------
            var theUnsavedTab = TabManagerUIMap.FindTabByName(tabName + " *");
            //------------Assert Results-------------------------
            Assert.IsNotNull(theUnsavedTab, "Editted workflow does not have the unsaved * after its name on the tab");
        }

        [TestMethod]
        // Should be unit test
        public void TypeInCalcBoxExpectedTooltipAppears()
        {
            //Create the Workflow for the test
            RibbonUIMap.CreateNewWorkflow();

            // For later
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Calculate control on
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl calculateControl = ToolboxUIMap.FindToolboxItemByAutomationId("Calculate");
            ToolboxUIMap.DragControlToWorkflowDesigner(calculateControl, workflowPoint1);

            Playback.Wait(500);
            Mouse.Click();

            Playback.Wait(500);
            SendKeys.SendWait("sum{(}");

            // Find the control
            UITestControl calculateOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Calculate");

            // Find the fxBox - This seemed resilient to filter properties for some odd reason...
            WpfEdit fxBox = new WpfEdit(calculateOnWorkflow);

            UITestControlCollection boxCollection = fxBox.FindMatchingControls();
            Playback.Wait(150);
            WpfEdit realfxBox = new WpfEdit();
            foreach(WpfEdit theBox in boxCollection)
            {
                string autoId = theBox.AutomationId;
                if(autoId == "UI__fxtxt_AutoID")
                {
                    realfxBox = theBox;
                }
            }

            Playback.Wait(2000);

            string helpText = realfxBox.GetProperty("Helptext").ToString();

            if(!helpText.Contains("sum(number{0}, number{N})") || (!helpText.Contains("Sums all the numbers given as arguments and returns the sum.")))
            {
                Assert.Fail("The tooltip for the Sum box does not appear.");
            }
        }

        [TestMethod]
        // 05/11 - Failure is Correct - Functionality is broken ;)
        public void CheckAddMissingIsWorkingWhenManuallyAddingVariableExpectedToShowVariablesAsUnUsed()
        {
            //Open the correct workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(1500);

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("codedUITestVar");
            VariablesUIMap.ClickVariableName(1);

            Assert.IsFalse(VariablesUIMap.CheckIfVariableIsUsed(0));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(1));
            Playback.Wait(150);
        }

        [TestMethod]
        // Regression Test
        public void ValidDatalistSearchTest()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Open the Variables tab, and enter the invalid value
            DockManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("test@");

            // Click below to fire the validity check
            VariablesUIMap.ClickVariableName(1);

            // The box should be invalid, and have the tooltext saying as much.
            bool isValid = VariablesUIMap.CheckVariableIsValid(0);

            if(isValid)
            {
                Assert.Fail("The DataList accepted the invalid variable name.");
            }
        }

        [TestMethod]
        public void DragAWorkflowIntoAndOutOfAForEach_Expected_NoErrors()
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
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);


            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // Now - Onto Part 2!

            // 5792.2

            // Get the location of the ForEach box
            UITestControl forEachControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "ForEach");

            // Move the mouse to the contained CalculateTaxReturns box
            Mouse.Move(new Point(forEachControl.BoundingRectangle.X + 175, forEachControl.BoundingRectangle.Y + 75));

            // Click it
            Mouse.Click();

            // And drag it down
            Mouse.StartDragging();
            Mouse.StopDragging(new Point(workflowPoint1.X, workflowPoint1.Y + 100));

            // Now get its position
            UITestControl calcTaxReturnsControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");

            // Its not on the design surface, must be in foreach
            Assert.IsNotNull(calcTaxReturnsControl, "Could not drop it ;(");
            }

        [TestMethod]
        public void ClickShowMapping_Expected_InputOutputAdornersAreDisplayed()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DockManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("TestFlow");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "TestFlow");
            Mouse.Click(controlOnWorkflow, new Point(265, 5));
        }

        #region Tests Requiring Designer access

        // vi - Can I drop a tool onto the designer?
        [TestMethod]
        // 05/11 - Failure is Intermittent ;)
        public void DropAWorkflowOrServiceOnFromTheToolBoxAndTestTheWindowThatPopsUp()
        {
            // Create the Workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get the tab
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DockManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            // Wait for dialog
            PopupDialogUIMap.WaitForDialog();

            // Wait for refresh
            Playback.Wait(5000);

            #region Checking Ok Button enabled property

            //Get the Ok button from the window
            UITestControl buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the buttton is disabled
            Assert.IsFalse(buttonControl.Enabled);

            //Open the folder in the tree
            ActivityDropUIMap.DoubleClickFirstWorkflowFolder();

            //Single click a resource in the tree
            ActivityDropUIMap.SingleClickFirstResource();
            Playback.Wait(300);
            //get the ok button from the window
            buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the button is enabled
            Assert.IsTrue(buttonControl.Enabled);

            //Single click on a folder again
            ActivityDropUIMap.SingleClickAFolder();

            //Get the ok button from the window
            buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the button is disabled
            Assert.IsFalse(buttonControl.Enabled);

            #endregion

            #region Checking the double click of a resource puts it on the design surface

            //Select a resource in the explorer view
            ActivityDropUIMap.DoubleClickAResource();

            // Check if it exists on the designer
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "ServiceDesigner"));
            SendKeys.SendWait("{DELETE}");

            #endregion

            #region Checking the click of the Cacnel button doesnt Adds the resource to the design surface

            // Open the Toolbox
            DockManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            // Wait for the window to show up
            PopupDialogUIMap.WaitForDialog();

            // Wait for refresh
            Playback.Wait(5000);

            //Open the folder in the tree
            ActivityDropUIMap.DoubleClickFirstWorkflowFolder();

            // Single click a folder in the tree
            ActivityDropUIMap.SingleClickFirstResource();

            Playback.Wait(2000);
            // Click the Ok button on the window
            ActivityDropUIMap.ClickOkButton();

            // Check if it exists on the designer
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));

            #endregion
        }

        #endregion Tests Requiring Designer access

        #region Additional test methods

        [TestCleanup]
        public void DoCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        #region Groomed Test

        [TestMethod]
        public void CheckIfDebugProcessingBarIsShowingDurningExecutionExpectedToShowDuringExecutionOnly()
        {
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("LargeFileTesting");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            if(DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F5}");
                Playback.Wait(1000);
            }
            DockManagerUIMap.ClickOpenTabPage("Output");
            var status = OutputUIMap.GetStatusBarStatus();
            var spinning = OutputUIMap.IsSpinnerSpinning();
            Assert.AreEqual("Executing", status, "Debug output status text does not say executing when executing");
            Assert.IsTrue(spinning, "Debug output spinner not spinning during execution");
        }

        [TestMethod]
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Feedback");
            Playback.Wait(500);
            var dialogPrompt = DockManagerUIMap.UIBusinessDesignStudioWindow.GetChildren()[0];
            if(dialogPrompt.GetType() != typeof(WpfWindow))
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("{ENTER}");

            // Wait for the init, then click around a bit
            Playback.Wait(2500);
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            Playback.Wait(500);
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            Playback.Wait(500);

            // Click stop, then make sure the Feedback window has appeared.
            FeedbackUIMap.ClickStartStopRecordingButton();
            Playback.Wait(500);
            if(!FeedbackUIMap.DoesFeedbackWindowExist())
            {
                Assert.Fail("The Feedback window did not appear after the recording has been stopped.");
            }

            Playback.Wait(1000);
            FeedbackUIMap.FeedbackWindow_ClickCancel();

            // TODO: FIX THIS ON TEST SERVER!!
            // Click Open default email
            //FeedbackUIMap.FeedbackWindow_ClickOpenDefaultEmail();

            //Playback.Wait(2500);
            //bool hasOutlookOpened = ExternalUIMap.Outlook_HasOpened();
            //if (!hasOutlookOpened)
            //{
            //    Assert.Fail("Outlook did not open when ClickOpenDefaultEmail was clicked!");
            //}
            //else
            //{
            //    ExternalUIMap.CloseAllInstancesOfOutlook();
            //}
        }

        // Bug 8747
        [TestMethod]
        // 05/11 - Failure is Intermittent ;)
        public void DebugBuriedErrors_Expected_OnlyErrorStepIsInError()
        {
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Bug8372");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Bugs", "Bug8372");

            // Run debug
            SendKeys.SendWait("{F5}");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");
            OutputUIMap.WaitForExecution();

            // Open the Output
            DockManagerUIMap.ClickOpenTabPage("Output");

            // Get nested steps
            Assert.IsTrue(OutputUIMap.IsAnyStepsInError(), "Cannot see nested error steps in the debug output.");
        }

        #endregion Deprecated Test

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///
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

        #endregion
    }
}
