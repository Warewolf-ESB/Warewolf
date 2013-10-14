using System.Windows.Input;
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
using Dev2.Studio.UI.Tests.UIMaps.ActivityDropWindowUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServerWizardClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SwitchUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.VideoTestUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using Point = System.Drawing.Point;

namespace Dev2.CodedUI.Tests
{
    [CodedUITest]
    public class TestBase
    {
        public string ServerExeLocation;

        public static string GetStudioWindowName()
        {
            return "Warewolf";
        }

        #region New PBI Tests

        

        // PBI 8601 (Task 8855)
        [TestMethod]
        public void QuickVariableInputFromListTest()
        {
           Clipboard.Clear();
           // Create the workflow
           RibbonUIMap.CreateNewWorkflow();

           // Get some variables
           UITestControl theTab = TabManagerUIMap.GetActiveTab();
           Point startPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
           Point point = new Point(startPoint.X, startPoint.Y + 200);

           // Drag the tool onto the workflow
           DocManagerUIMap.ClickOpenTabPage("Toolbox");
           UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
           ToolboxUIMap.DragControlToWorkflowDesigner(theControl, point);

           //Get Mappings button
           UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "Open Quick Variable Input");

           // Click it
           Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
           Mouse.Click();

           // Enter some invalid data
           WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",", "some(<).", "_suf", "varOne,varTwo,varThree");

           // Click done
           WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

           var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Prefix contains invalid characters");
           Assert.IsNotNull(errorControl, "No error displayed for incorrect QVI input");

            // Assert clicking an error focusses the correct textbox
            Mouse.Click(errorControl.GetChildren()[0]);

            // enter some correct data
            SendKeys.SendWait("^a^xpre_");

            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

            // Check the data
            string varName = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(varName, "[[pre_varOne_suf]]");

            // All good - Clean up!
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        //PBI_8853
        [TestMethod]
        public void NewWorkflowShortcutKeyExpectedWorkflowOpens()
        {
            var preCount = TabManagerUIMap.GetTabCount();
            Mouse.Click(DocManagerUIMap.UIBusinessDesignStudioWindow);
            SendKeys.SendWait("^w");
            string activeTabName = TabManagerUIMap.GetActiveTabName();
            var postCount = TabManagerUIMap.GetTabCount();
            Assert.IsTrue(postCount == preCount + 1, "Tab quantity has not been increased");
            Assert.IsTrue(activeTabName.Contains("Unsaved"), "Active workflow is not an unsaved workflow");
            TabManagerUIMap.CloseAllTabs();
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
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
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
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
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

            string text = WorkflowDesignerUIMap.AssignControl_GetVariableName(theTab, "Assign", 0);
            StringAssert.Contains(text, "[[theVar0]]");
          
            // All good - Cleanup time!
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        ////PBI 9461
        [TestMethod]
        public void ChangingResourceExpectedPopUpWarningWithShowAffected()
        {
            // Open the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("NewForeachUpgrade");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "NewForeachUpgradeDifferentExecutionTests");
            //Edit the inputs and outputs
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.CheckScalarInputAndOuput(0);
            VariablesUIMap.CheckScalarInput(0);

            //Save the workflow
            RibbonUIMap.ClickRibbonMenuItem("Save");

            PopupDialogUIMap.WaitForDialog();
        
            //Click the show affected button
            ResourceChangedPopUpUIMap.ClickViewDependancies();

            Playback.Wait(5000);

            Assert.AreEqual(TabManagerUIMap.GetActiveTabName(),"ForEachUpgradeTest");

            TabManagerUIMap.CloseAllTabs();    
        }

        #region Auto Expand Of Mapping On Drop

        //PBI 9939
        [TestMethod]
        [TestCategory("DsfActivityTests")]
        [Description("Testing when a DsfActivity is dropped onto the design surface that the mapping auto expands.")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void DsfActivityDesigner_CodedUI_DroppingActivityOntoDesigner_MappingToBeExpanded()
        // ReSharper restore InconsistentNaming
        {
            //Create a new workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get the tab
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability            
            Playback.Wait(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // And click it for UI responsiveness :P
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Explorer
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("MO");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(500);

            //Drag workflow onto surface
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("TestForEachOutput");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "TestForEachOutput", p);
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();

            //Get Mappings button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TestForEachOutput", "OpenMappingsToggle");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(1500);

            //Assert button is not null
            Assert.IsTrue(button != null, "Couldnt find the mapping button");

            //Get the close mappings image
            var children = button.GetChildren();
            var images = children.FirstOrDefault(c => c.FriendlyName == "Close Mappings");

            //Check that the mapping is open
            Assert.IsTrue(images.Height > -1, "The correct images isnt visible which means the mapping isnt open");

            //Clean up
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
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
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl asssignControlInToolbox = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(asssignControlInToolbox, workflowPoint1);

            // Click away
            Mouse.Click(new Point(workflowPoint1.X + 50, workflowPoint1.Y + 50));

            //------------Execute Test---------------------------
            var theUnsavedTab = TabManagerUIMap.FindTabByName(tabName+" *");
            //------------Assert Results-------------------------
            Assert.IsTrue(theUnsavedTab.Exists);
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
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
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
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

            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        [TestMethod]
        // Regression Test
        public void CheckAddMissingIsWorkingWhenManuallyAddingVariableExpectedToShowVariablesAsUnUsed()
        {
            //Open the correct workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(1500);

            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("codedUITestVar");
            VariablesUIMap.ClickVariableName(1);

            Assert.IsFalse(VariablesUIMap.CheckIfVariableIsUsed(0));
            Assert.IsTrue(VariablesUIMap.CheckIfVariableIsUsed(1));
            Playback.Wait(150);
            DoCleanup("CalculateTaxReturns", true);
        }

        [TestMethod]
        // Regression Test
        public void ValidDatalistSearchTest()
        {

            //// Create the workflow
            RibbonUIMap.CreateNewWorkflow();

            // Open the Variables tab, and enter the invalid value
            DocManagerUIMap.ClickOpenTabPage("Variables");
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

            // Clean Up! \o/
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion

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
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);


            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("CalculateTaxReturns");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, new Point(workflowPoint1.X + 25, workflowPoint1.Y + 25));

            // Wait for the ForEach thing to do its init-y thing
            Playback.Wait(1500);

            // And click below the tab to get us back to the normal screen
            Mouse.Move(new Point(theTab.BoundingRectangle.X + 50, theTab.BoundingRectangle.Y + 50));
            Mouse.Click();

            // Now - Onto Part 2!

            // 5792.2

            // Get the location of the ForEach box
            UITestControl forEachControl = workflowDesignerUIMap.FindControlByAutomationId(theTab, "ForEach");

            // Move the mouse to the contained CalculateTaxReturns box
            Mouse.Move(new Point(forEachControl.BoundingRectangle.X + 75, forEachControl.BoundingRectangle.Y + 75));

            // Click it
            Mouse.Click();

            // And drag it down
            Mouse.StartDragging();
            Mouse.StopDragging(new Point(workflowPoint1.X, workflowPoint1.Y + 100));

            // Now get its position
            UITestControl calcTaxReturnsControl = workflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");

            // Its not on the design surface, must be in foreach
            Assert.IsNotNull(calcTaxReturnsControl, "Could not drop it ;(");

            TabManagerUIMap.CloseAllTabs();

        }

        [TestMethod]
        public void DragADecisionIntoForEachExpectNotAddedToForEach()
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
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Open the toolbox, and drag the control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", requiredPoint);

            // Cancel Decision Wizard
            if (WizardsUIMap.TryWaitForWizard(5000))
            {
                var decisionWizardUiMap = new DecisionWizardUIMap();
                decisionWizardUiMap.ClickCancel();
            }
            else
            {
                Assert.Fail("Got droped ;(");
            }

            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod]
        public void DragASwitchIntoForEachExpectNotAddedToForEach()
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
            UITestControl tcForEach = ToolboxUIMap.FindToolboxItemByAutomationId("ForEach");
            ToolboxUIMap.DragControlToWorkflowDesigner(tcForEach, workflowPoint1);

            // Open the toolbox, and drag the control onto the Workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            ToolboxUIMap.DragControlToWorkflowDesigner("Switch", requiredPoint);
            Playback.Wait(500);
            // Cancel Decision Wizard
            try
            {
                var decisionWizardUiMap = new SwitchWizardUIMap();
                decisionWizardUiMap.ClickCancel();
                Assert.Fail("Got droped ;(");
            }
            catch
            {
                Assert.IsTrue(true);
            }

            TabManagerUIMap.CloseAllTabs();
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
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("TestFlow");
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = workflowDesignerUIMap.FindControlByAutomationId(theTab, "TestFlow");
            Mouse.Click(controlOnWorkflow, new Point(265, 5));

            Playback.Wait(2500);

            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {
            const string resourceToUse = "CalculateTaxReturns";
            RibbonUIMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 100);

            // Open the Explorer
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(resourceToUse);
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", resourceToUse);

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, resourceToUse);
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();

            Point initialResizerPoint = new Point();
            Point newResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            var resizeThumb = controlCollection[controlCollection.Count - 1];
            if (resizeThumb.ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = resizeThumb;
                initialResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                initialResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }
            else
            {
                Assert.Fail("Cannot find resize indicator");
            }

            // Drag
            Mouse.Click(initialResizerPoint);
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y - 50));

            // Check position to see it dragged
            if (resizeThumb.ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = resizeThumb;
                newResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                newResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }

            if (!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }

            // Test complete - Delete itself
            TabManagerUIMap.CloseAllTabs();
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

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            #region Checking Ok Button enabled property

            //Get the Ok button from the window
            UITestControl buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the buttton is disabled
            Assert.IsFalse(buttonControl.Enabled);

            //Open the folder in the tree
            ActivityDropUIMap.DoubleClickFirstWorkflowFolder();

            //Single click a resource in the tree
            ActivityDropUIMap.SingleClickAResource();

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
            Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));
            SendKeys.SendWait("{DELETE}");

            #endregion

            #region Checking the click of the Cacnel button doesnt Adds the resource to the design surface

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            //Wait for the window to show up
            Playback.Wait(2000);

            //Single click a folder in the tree
            ActivityDropUIMap.SingleClickAResource();

            //Click the Ok button on the window
            ActivityDropUIMap.ClickCancelButton();

            // Check if it exists on the designer
            Assert.IsFalse(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));

            #endregion

            // Delete the workflow
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion Tests Requiring Designer access

        #region Additional test methods

        /// <summary>
        /// Deletes a service (Workflow) - Generally used at the end of a Coded UI Test
        /// </summary>
        /// <param name="server">The servername (EG: localhost)</param>
        /// <param name="serviceType">The Service Type (Eg: WORKFLOWS)</param>
        /// <param name="category">The Category(EG: CODEDUITESTCATEGORY)</param>
        /// <param name="workflowName">The Workflow Name (Eg: MyCustomWorkflow)</param>
        public void DoCleanup(string workflowName, bool clickNo = false)
        {
            try
            {
                TabManagerUIMap.CloseAllTabs();
                //// Test complete - Delete itself  
                //if (clickNo)
                //{
                //    TabManagerUIMap.CloseTab_Click_No(workflowName);
                //}
                //else
                //{
                //    TabManagerUIMap.CloseTab(workflowName);
                //}
            }
            catch (Exception e)
            {
                // Log it so the UI Test still passes...
                Trace.WriteLine(e.Message);
            }

        }

        #endregion

        #region Groomed Test

        [TestMethod]
        public void CheckIfDebugProcessingBarIsShowingDurningExecutionExpectedToShowDuringExecutionOnly()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("LargeFileTesting");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();

            RibbonUIMap.ClickRibbonMenuItem("Debug");
            if (DebugUIMap.WaitForDebugWindow(5000))
            {
                SendKeys.SendWait("{F5}");
                Playback.Wait(1000);
            }
            DocManagerUIMap.ClickOpenTabPage("Output");
            UITestControl control = OutputUIMap.GetStatusBar();

            UITestControlCollection statusBarChildren = control.GetChildren();
            var progressbar = statusBarChildren.First(c => c.ClassName == "Uia.CircularProgressBar");
            var label = statusBarChildren.First(c => c.ClassName == "Uia.Text");
            Assert.IsTrue(label.FriendlyName == "Executing");
            Assert.IsTrue(progressbar.Height != -1);
            DoCleanup("LargeFileTesting", true);
        }

        [TestMethod]
        [Ignore]//needs outlook installed on the ui testing environment
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Feedback");
            Playback.Wait(500);
            var dialogPrompt = DocManagerUIMap.UIBusinessDesignStudioWindow.GetChildren()[0];
            if(dialogPrompt.GetType() != typeof(WpfWindow))
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }
            SendKeys.SendWait("{ENTER}");
            SendKeys.SendWait("{ENTER}");

            // Wait for the init, then click around a bit
            Playback.Wait(2500);
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            Playback.Wait(500);
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            Playback.Wait(500);

            // Click stop, then make sure the Feedback window has appeared.
            FeedbackUIMap.ClickStartStopRecordingButton();
            Playback.Wait(500);
            if(!FeedbackUIMap.DoesFeedbackWindowExist())
            {
                Assert.Fail("The Feedback window did not appear after the recording has been stopped.");
            }

            // Click Open default email
            FeedbackUIMap.FeedbackWindow_ClickOpenDefaultEmail();

            Playback.Wait(2500);
            bool hasOutlookOpened = ExternalUIMap.Outlook_HasOpened();
            if (!hasOutlookOpened)
            {
                Assert.Fail("Outlook did not open when ClickOpenDefaultEmail was clicked!");
            }
            else
            {
                ExternalUIMap.CloseAllInstancesOfOutlook();
            }
        }

        // Bug 8747
        [TestMethod]
        [Ignore]//Jurie wrote this very badly needs to be revisitted
        public void DebugBuriedErrors_Expected_OnlyErrorStepIsInError()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Bug8372");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Bugs", "Bug8372");
            ExplorerUIMap.ClearExplorerSearchText();

            // Run debug
            SendKeys.SendWait("{F5}");
            PopupDialogUIMap.WaitForDialog();
            SendKeys.SendWait("{F5}");

            // Open the Output
            DocManagerUIMap.ClickOpenTabPage("Output");

            // Due to the complexity of the OutputUIMap, this test has been primarily hard-coded until a further rework
            Assert.IsTrue(OutputUIMap.DoesBug8747Pass());
            TabManagerUIMap.CloseAllTabs();
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

        #region UI Maps

        public DocManagerUIMap DocManagerUIMap
        {
            get
            {
                if ((_docManagerMap == null))
                {
                    _docManagerMap = new DocManagerUIMap();
                }

                return _docManagerMap;
            }
        }
        private DocManagerUIMap _docManagerMap;

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((_toolboxUiMap == null))
                {
                    _toolboxUiMap = new ToolboxUIMap();
                }

                return _toolboxUiMap;
            }
        }
        private ToolboxUIMap _toolboxUiMap;

        public SaveDialogUIMap SaveDialogUIMap
        {
            get
            {
                if ((_saveDialogUIMap == null))
                {
                    _saveDialogUIMap = new SaveDialogUIMap();
                }

                return _saveDialogUIMap;
            }
        }
        private SaveDialogUIMap _saveDialogUIMap;

        #region Explorer UI Map

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if((_explorerUiMap == null))
                {
                    _explorerUiMap = new ExplorerUIMap();
                }

                return _explorerUiMap;
            }
        }
        private ExplorerUIMap _explorerUiMap;
        
        #endregion

        #region Deploy UI Map

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if((_deployViewUiMap == null))
                {
                    _deployViewUiMap = new DeployViewUIMap();
                }

                return _deployViewUiMap;
            }
        }
        private DeployViewUIMap _deployViewUiMap;
        
        #endregion

        #region Connect Window UI Map

        public ServerWizard ConnectViewUIMap
        {
            get
            {
                if (_connectViewUIMap == null)
                {
                    _connectViewUIMap = new ServerWizard();
                }
                return _connectViewUIMap;
            }
        }

        private ServerWizard _connectViewUIMap;

        #endregion Connect Window UI Map

        #region Debug UI Map

        public DebugUIMap DebugUIMap
        {
            get
            {
                if (_debugUIMap == null)
                {
                    _debugUIMap = new DebugUIMap();
                }
                return _debugUIMap;
            }
        }

        private DebugUIMap _debugUIMap;

        #endregion

        #region ActivityDrop Window UI Map

        public ActivityDropWindowUIMap ActivityDropUIMap
        {
            get
            {
                if (_activityDropUIMap == null)
                {
                    _activityDropUIMap = new ActivityDropWindowUIMap();
                }
                return _activityDropUIMap;
            }
        }

        private ActivityDropWindowUIMap _activityDropUIMap;

        #endregion

        #region DependencyGraph UI Map

        public DependencyGraph DependencyGraphUIMap
        {
            get
            {
                if (DependencyGraphUIMap == null)
                {
                    DependencyGraphUIMap = new DependencyGraph();
                }

                return DependencyGraphUIMap;
            }
            set { throw new NotImplementedException(); }
        }

        private DependencyGraph DependencyGraph;

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
                if (_databaseServiceWizardUIMap == null)
                {
                    _databaseServiceWizardUIMap = new DatabaseServiceWizardUIMap();
                }

                return _databaseServiceWizardUIMap;
            }
        }

        private DatabaseServiceWizardUIMap _databaseServiceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Database Source Wizard UI Map

        public DatabaseSourceUIMap DatabaseSourceWizardUIMap
        {
            get
            {
                if (_databaseSourceWizardUIMap == null)
                {
                    _databaseSourceWizardUIMap = new DatabaseSourceUIMap();
                }

                return _databaseSourceWizardUIMap;
            }
        }

        private DatabaseSourceUIMap _databaseSourceWizardUIMap;

        public PluginSourceMap PluginSourceMap
        {
            get
            {
                if (_pluginSourceWizardUIMap == null)
                {
                    _pluginSourceWizardUIMap = new PluginSourceMap();
                }

                return _pluginSourceWizardUIMap;
            }
        }

        private PluginSourceMap _pluginSourceWizardUIMap;

        #endregion Database Wizard UI Map

        #region Feedback UI Map

        public FeedbackUIMap FeedbackUIMap
        {
            get
            {
                if (_feedbackUIMap == null)
                {
                    _feedbackUIMap = new FeedbackUIMap();
                }

                return _feedbackUIMap;
            }
        }

        private FeedbackUIMap _feedbackUIMap;

        #endregion Feedback UI Map

        #region New Server UI Map

        public NewServerUIMap NewServerUIMap
        {
            get
            {
                if (_newServerUIMap == null)
                {
                    _newServerUIMap = new NewServerUIMap();
                }

                return _newServerUIMap;
            }
        }

        private NewServerUIMap _newServerUIMap;

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

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if (_ribbonUIMap == null)
                {
                    _ribbonUIMap = new RibbonUIMap();
                }

                return _ribbonUIMap;
            }
        }

        private RibbonUIMap _ribbonUIMap;

        #endregion Ribbon UI Map

        #region Resource Changed PopUp UI Map

        public ResourceChangedPopUpUIMap ResourceChangedPopUpUIMap
        {
            get
            {
                if (_resourceChangedPopUpUIMap == null)
                {
                    _resourceChangedPopUpUIMap = new ResourceChangedPopUpUIMap();
                }
                return _resourceChangedPopUpUIMap;
            }
        }

        private ResourceChangedPopUpUIMap _resourceChangedPopUpUIMap;

        #endregion

        #region TabManager UI Map


        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if (_tabManagerUIMap == null)
                {
                    _tabManagerUIMap = new TabManagerUIMap();
                }

                return _tabManagerUIMap;
            }
        }





        private TabManagerUIMap _tabManagerUIMap;

        #endregion TabManager UI Map

        #region Variables UI Map

        public VariablesUIMap VariablesUIMap
        {
            get
            {
                if (_variablesUIMap == null)
                {
                    _variablesUIMap = new VariablesUIMap();
                }
                return _variablesUIMap;
            }
        }

        private VariablesUIMap _variablesUIMap;

        #endregion Connect Window UI Map

        #region Service Details UI Map

        public ServiceDetailsUIMap ServiceDetailsUIMap
        {
            get
            {
                if (_serviceDetailsUIMap == null)
                {
                    _serviceDetailsUIMap = new ServiceDetailsUIMap();
                }
                return _serviceDetailsUIMap;
            }
        }

        private ServiceDetailsUIMap _serviceDetailsUIMap;

        #endregion

        #region External UI Map

        public ExternalUIMap ExternalUIMap
        {
            get
            {
                if (_externalUIMap == null)
                {
                    _externalUIMap = new ExternalUIMap();
                }
                return _externalUIMap;
            }
        }

        private ExternalUIMap _externalUIMap;

        #endregion External UI Map

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

        #region Output UI Map

        public OutputUIMap OutputUIMap
        {
            get
            {
                if (_outputUIMap == null)
                {
                    _outputUIMap = new OutputUIMap();
                }

                return _outputUIMap;
            }
        }

        private OutputUIMap _outputUIMap;

        #endregion Output UI Map

        #region VideoTest UI Map

        public VideoTestUIMap VideoTestUIMap
        {
            get
            {
                if (_videoTestUIMap == null)
                {
                    _videoTestUIMap = new VideoTestUIMap();
                }

                return _videoTestUIMap;
            }
        }

        private VideoTestUIMap _videoTestUIMap;

        #endregion VideoTest UI Map

        #region UIErrorWindow

        public class UIErrorWindow : WpfWindow
        {

            public UIErrorWindow()
            {
                #region Search Criteria
                SearchProperties[UITestControl.PropertyNames.Name] = "Error";
                SearchProperties.Add(new PropertyExpression(UITestControl.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
                WindowTitles.Add("Error");
                #endregion
            }
        }

        #endregion

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

        #endregion UI Maps
    }
}
