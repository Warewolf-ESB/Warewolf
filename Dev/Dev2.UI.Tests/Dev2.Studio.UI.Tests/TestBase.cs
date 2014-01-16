using System.Globalization;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
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

        #region Cleanup


        [ClassInitialize]
        public static void ClassInit(TestContext tctx)
        {
            Playback.Initialize();
            Playback.PlaybackSettings.ContinueOnError = true;
            Playback.PlaybackSettings.ShouldSearchFailFast = true;
            Playback.PlaybackSettings.SmartMatchOptions = SmartMatchOptions.None;
            Playback.PlaybackSettings.MatchExactHierarchy = true;
            Playback.PlaybackSettings.DelayBetweenActions = 1;

            // make the mouse quick ;)
            Mouse.MouseMoveSpeed = 10000;
            Mouse.MouseDragSpeed = 10000;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

        #region New PBI Tests

        //PBI_8853
        [TestMethod]
        public void NewWorkflowShortcutKeyExpectedWorkflowOpens()
        {
            var preCount = TabManagerUIMap.GetTabCount();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.AllThreads;
            StudioWindow.WaitForControlReady();
            Playback.PlaybackSettings.WaitForReadyLevel = WaitForReadyLevel.UIThreadOnly;
            StudioWindow.SetFocus();
            Keyboard.SendKeys(StudioWindow, "{CTRL}W");
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
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_AssignLargeView")]
        public void ToolDesigners_AssignLargeView_EnteringMultipleRows_IndexingWorksFine()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", workflowPoint1);

            //Get Large View button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign",
                                                                           "Open Large View");

            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_LargeViewClickLeftTextboxInRow(theTab, "Assign", 0);
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

            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();

            Playback.Wait(500);
            var leftTextBoxInRowLastRow = WorkflowDesignerUIMap.AssignControl_GetLeftTextboxInRow("Assign", 19) as WpfEdit;
            string text = leftTextBoxInRowLastRow.Text;
            StringAssert.Contains(text, "[[theVar19]]");
        }

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
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            // moved from 100 to 20 for time
            for(int j = 0; j < 20; j++)
            {
                // Sleeps are due to the delay when adding a lot of items
                Playback.Wait(15);
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

            Assert.IsFalse(string.IsNullOrEmpty(text));

            // Yet if it did not crash the act of copy and paste should provide something ;)
            //StringAssert.Contains(text, "[[theVar19]]");
        }

        ////PBI 9461
        [TestMethod]
        public void ChangingResourceExpectedPopUpWarningWithShowAffected()
        {
            // Open the workflow
            ExplorerUIMap.EnterExplorerSearchText("NewForeachUpgrade");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "NewForeachUpgradeDifferentExecutionTests");

            //Edit the inputs and outputs
            VariablesUIMap.CheckScalarInputAndOuput(0);
            VariablesUIMap.CheckScalarInput(0);

            //Save the workflow
            RibbonUIMap.ClickRibbonMenuItem("Save");
            PopupDialogUIMap.WaitForDialog();

            //Click the show affected button
            ResourceChangedPopUpUIMap.ClickViewDependancies();

            Playback.Wait(2000);

            var result = TabManagerUIMap.GetActiveTabName().Contains("ForEachUpgradeTest");
            Assert.IsTrue(result, "Affected workflow not shown after show affected workflow button pressed.");
        }

        [TestMethod]
        public void UnsavedStar_UITest_WhenWorkflowIsChanged_ExpectStarIsShowing()
        {
            //------------Setup for test--------------------------
            RibbonUIMap.CreateNewWorkflow();
            // Get some data
            Playback.Wait(100);
            var tabName = TabManagerUIMap.GetActiveTabName();
            UITestControl theTab = TabManagerUIMap.FindTabByName(tabName);
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Multi Assign on
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", workflowPoint1);

            // Click away
            Mouse.Click(new Point(workflowPoint1.X + 50, workflowPoint1.Y + 50));

            Playback.Wait(500);
            //------------Execute Test---------------------------
            var theUnsavedTab = TabManagerUIMap.GetActiveTab();
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
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Calculate control on
            ToolboxUIMap.DragControlToWorkflowDesigner("Calculate", workflowPoint1);

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

            Playback.Wait(3000);

            string helpText = realfxBox.GetProperty("Helptext").ToString();

            if(!helpText.Contains("sum(number{0}, number{N})") || (!helpText.Contains("Sums all the numbers given as arguments and returns the sum.")))
            {
                Assert.Fail("The tooltip for the Sum box does not appear.");
            }
        }

        [TestMethod]
        public void CheckAddMissingIsWorkingWhenManuallyAddingVariableExpectedToShowVariablesAsUnUsed()
        {
            //Open the correct workflow
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(1500);

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            VariablesUIMap.ClickScalarVariableName(5);
            SendKeys.SendWait("codedUITestVar");
            VariablesUIMap.ClickScalarVariableName(4);

            Assert.IsFalse(VariablesUIMap.CheckIfVariableIsUsed(5));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(1));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(2));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(3));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(4));
            Playback.Wait(150);
        }

        [TestMethod]
        // Regression Test
        public void ValidDatalistSearchTest()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Open the Variables tab, and enter the invalid value
            VariablesUIMap.ClickScalarVariableName(0);
            SendKeys.SendWait("test@");

            // Click below to fire the validity check
            VariablesUIMap.ClickScalarVariableName(1);

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
            ToolboxUIMap.DragControlToWorkflowDesigner("ForEach", workflowPoint1, "For Each");

            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            var targetPoint = new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25);
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns", targetPoint);

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
            Mouse.StopDragging(new Point(workflowPoint1.X - 200, workflowPoint1.Y + 100));

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

            // Get a sample workflow
            ExplorerUIMap.EnterExplorerSearchText("TestFlow");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "TEST", "TestFlow", workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "TestFlow");
            Mouse.Click(controlOnWorkflow, new Point(265, 5));
        }

        #region Tests Requiring Designer access

        // vi - Can I drop a tool onto the designer?
        [TestMethod]
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

            // Get the comment box

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner("Workflow", p);

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

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner("Workflow", p);

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

        #region Groomed Test

        [TestMethod]
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Feedback");
            Playback.Wait(500);
            var dialogPrompt = StudioWindow.GetChildren()[0];
            if(dialogPrompt.GetType() != typeof(WpfWindow))
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }

            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("{ENTER}");

            // Wait for the init, then click around a bit

            // Click stop, then make sure the Feedback window has appeared.
            FeedbackUIMap.ClickStartStopRecordingButton();
            Playback.Wait(1500);
            if(!FeedbackUIMap.DoesFeedbackWindowExist())
            {
                Assert.Fail("The Feedback window did not appear after the recording has been stopped.");
            }

            Playback.Wait(500);
            FeedbackUIMap.FeedbackWindow_ClickCancel();
        }

        // Bug 8747
        [TestMethod]
        // 05/11 - Failure is Intermittent ;)
        public void DebugBuriedErrors_Expected_OnlyErrorStepIsInError()
        {
            //Open the correct workflow
            ExplorerUIMap.EnterExplorerSearchText("Bug8372");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "Bug8372");

            // Run debug
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            PopupDialogUIMap.WaitForDialog();
            DebugUIMap.ClickExecute();
            OutputUIMap.WaitForExecution();

            var result = OutputUIMap.IsAnyStepsInError();

            // Get nested steps
            Assert.IsTrue(result, "Cannot see nested error steps in the debug output.");
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
