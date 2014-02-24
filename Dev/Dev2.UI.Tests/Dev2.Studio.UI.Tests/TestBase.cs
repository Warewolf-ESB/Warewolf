using System.Globalization;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
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

        #endregion New PBI Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ToolDesigners_AssignLargeView")]
        public void ToolDesigners_AssignLargeView_EnteringMultipleRows_IndexingWorksFine()
        {
            DsfMultiAssignUiMap activityUiMap = new DsfMultiAssignUiMap();
            activityUiMap.ClickOpenLargeView();
            // Add the data!
            // moved from 100 to 10 for time
            for(int j = 0; j < 10; j++)
            {
                activityUiMap.EnterTextIntoVariable(j, "[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                activityUiMap.EnterTextIntoValue(j, j.ToString(CultureInfo.InvariantCulture));
            }

            // Click it
            Assert.AreEqual("[[theVar9]]", activityUiMap.GetTextFromVariable(9));
        }

        [TestMethod]
        public void AddLargeAmountsOfDataListItems_Expected_NoHanging()
        {
            DsfMultiAssignUiMap activityUiMap = new DsfMultiAssignUiMap();
            // Add the data!
            // moved from 100 to 10 for time
            for(int j = 0; j < 10; j++)
            {
                activityUiMap.EnterTextIntoVariable(j, "[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
                activityUiMap.EnterTextIntoValue(j, j.ToString(CultureInfo.InvariantCulture));
            }

            // Click it
            Assert.AreEqual("[[theVar9]]", activityUiMap.GetTextFromVariable(9));
        }

        ////PBI 9461
        [TestMethod]
        public void ChangingResourceExpectedPopUpWarningWithShowAffected()
        {

            const string ResourceName = "NewForeachUpgradeDifferentExecutionTests";
            // Open the workflow
            ExplorerUIMap.DoubleClickWorkflow(ResourceName, "INTEGRATION TEST SERVICES");

            //Edit the inputs and outputs
            VariablesUIMap.CheckScalarInputOrOuput(0, Dev2MappingType.Input);
            VariablesUIMap.CheckScalarInputOrOuput(0, Dev2MappingType.Output);

            VariablesUIMap.CheckScalarInputOrOuput(0, Dev2MappingType.Input);

            //Save the workflow
            RibbonUIMap.ClickRibbonMenuItem("Save");
            PopupDialogUIMap.WaitForDialog();

            //Click the show affected button
            ResourceChangedPopUpUIMap.ClickViewDependancies();
            ResourceChangedPopUpUIMap.WaitForDependencyTab();

            var result = TabManagerUIMap.GetActiveTabName().Contains("ForEachUpgradeTest");
            Assert.IsTrue(result, "Affected workflow not shown after show affected workflow button pressed.");
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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, workflowPoint1);

            // Click away
            MouseCommands.ClickPoint(new Point(workflowPoint1.X + 50, workflowPoint1.Y + 50), 500);

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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Calculate, workflowPoint1);

            MouseCommands.WaitAndClick(500);

            KeyboardCommands.SendKey("sum{(}");

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
            RibbonUIMap.CreateNewWorkflow(1000);

            // Open the Variables tab, and enter the invalid value
            VariablesUIMap.ClickScalarVariableName(0);
            SendKeys.SendWait("test@");

            // Click below to fire the validity check
            VariablesUIMap.ClickScalarVariableName(1);

            // The box should be invalid, and have the tooltext saying as much.
            bool isValid = VariablesUIMap.CheckVariableNameIsValid(0);

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
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, workflowPoint1, "For Each");

            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            var targetPoint = new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25);
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns", targetPoint);

            // Now - Onto Part 2!

            // 5792.2

            // Get the location of the ForEach box
            UITestControl forEachControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "ForEach");
            MouseCommands.MoveAndClick(new Point(forEachControl.BoundingRectangle.X + 175, forEachControl.BoundingRectangle.Y + 75));

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

        #region Groomed Test

        [TestMethod]
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Feedback", 600);
            var dialogPrompt = StudioWindow.GetChildren()[0];
            if(dialogPrompt.GetType() != typeof(WpfWindow))
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }

            KeyboardCommands.SendEnter();
            KeyboardCommands.SendEnter();

            // Wait for the init, then click around a bit

            // Click stop, then make sure the Feedback window has appeared.
            FeedbackUIMap.ClickStartStopRecordingButton();

            if(!FeedbackUIMap.DoesFeedbackWindowExist())
            {
                Assert.Fail("The Feedback window did not appear after the recording has been stopped.");
            }

            FeedbackUIMap.FeedbackWindow_ClickCancel();
        }

        // Bug 8747
        [TestMethod]
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
