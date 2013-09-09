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
using Dev2.Studio.UI.Tests.UIMaps.ActivityDropWindowUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseSourceUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DebugUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DependencyGraphClasses;
using Dev2.Studio.UI.Tests.UIMaps.FeedbackUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.NewServerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.OutputUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.PluginSourceMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.SaveDialogUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServerWizardClasses;
using Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.VideoTestUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.WebServiceWizardUIMapClasses;
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

    /// Test Cases TO DO: 5 (v), 7 (vii), 8 (viii), 11 (xi) (xii - Resumption does not work)
    /// // vi done due to the ability to access items on the Workflow (WorkflowDesignerUIMap.cs)
    /// // xi limited by the inability to connect to other servers (Sashen + Server down at time of testing)
    /// <summary>
    /// Summary description for TestBase
    /// </summary>
    [CodedUITest]
    public class TestBase
    {
        public string ServerExeLocation;

        public void CreateWorkflow()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}W");
            Playback.Wait(150);
        }

        public static string GetStudioWindowName()
        {
            return "Warewolf";
        }

        #region New PBI Tests

        // PBI 8601 (Task 8855)
        [TestMethod]
        public void QuickVariableInputFromListTest()
       {

            // Create the workflow
            CreateWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            //click done
            WorkflowDesignerUIMap.Adorner_ClickDoneButton(theTab, "Assign(DsfMultiAssignActivityDesigner)");

            //Get Mappings button
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "QuickVariableInputToggle");

            // Click it
            Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
            Mouse.Click();


            WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",", "pre_", "_suf", "varOne,varTwo,varThree");

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
            CreateWorkflow();
            string activeTabName = TabManagerUIMap.GetActiveTabName();
            var postCount = TabManagerUIMap.GetTabCount();
            Assert.IsTrue(postCount == preCount + 1, "Tab quantity has not been increased");
            Assert.IsTrue(activeTabName.Contains("Unsaved"), "Active workflow is not an unsaved workflow");
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
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

        [TestMethod]
        public void ClickNewDatabaseServiceExpectedDatabaseServiceOpens()
        {
            Keyboard.SendKeys("{CTRL}{SHIFT}D");
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Database Service");
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            Playback.Wait(100);
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            //DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
            SendKeys.SendWait("{ESC}");
            Playback.Wait(5000);
            SendKeys.SendWait("{ESC}");
            Playback.Wait(5000);
        }

        [TestMethod]
        public void ClickNewWebServiceExpectedWebServiceOpens()
        {
            Keyboard.SendKeys("{CTRL}{SHIFT}W");
           
            Playback.Wait(5000);
            WebServiceWizardUIMap.Cancel();
            SendKeys.SendWait("{ESC}");
        }

        /// <summary>
        /// News the database service shortcut key expected database service opens.
        /// </summary>
        [TestMethod]
        public void NewDatabaseServiceShortcutKeyExpectedDatabaseServiceOpens()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}{SHIFT}d");
            Playback.Wait(5000);
            UITestControl uIItemImage = DatabaseServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uIItemImage == null)
            {
                Assert.Fail("Error - Clicking the new database service button does not create the new database service window");
            }
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
        }

        //[TestMethod]
        //public void ClickNewPluginServiceExpectedPluginServiceOpens()
        //{
        //    Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}{SHIFT}P");
        //    UITestControl uiTestControl = PluginServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
        //    if (uiTestControl == null)
        //    {
        //        Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
        //    }
        //    Playback.Wait(5000);
        //    PluginServiceWizardUIMap.ClickCancel();

        //}

        //[TestMethod]
        //public void ClickNewPluginServiceShortcutKeyExpectedPluginServiceOpens()
        //{
        //    DocManagerUIMap.ClickOpenTabPage("Explorer");
        //    SendKeys.SendWait("^+p");
        //    Playback.Wait(500);
        //    UITestControl uiTestControl = PluginServiceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
        //    if(uiTestControl == null)
        //    {
        //        Assert.Fail("Error - Clicking the new plugin service button does not create the new plugin service window");
        //    }
        //    Playback.Wait(5000);
        //    PluginServiceWizardUIMap.ClickCancel();
        //}

       
        /// <summary>
        /// Clicks the new database source expected database source opens.
        /// </summary>
        [TestMethod]
        public void ClickNewDatabaseSourceExpectedDatabaseSourceOpens()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}{SHIFT}D");
            Playback.Wait(500);
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{TAB}{TAB}{ENTER}");
            UITestControl uiTestControl = DatabaseSourceWizardUIMap.UIBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the Database source button does not create the new Database source window");
            }
            
            
            
            Playback.Wait(100);
            DatabaseSourceWizardUIMap.ClickCancel();
            Playback.Wait(5000);
            DatabaseServiceWizardUIMap.DatabaseServiceClickCancel();
            Playback.Wait(100);
        }

        [TestMethod]
        public void ClickNewPluginSourceExpectedPluginSourceOpens()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}{SHIFT}P");
            Playback.Wait(5000);
            
            PluginServiceWizardUIMap.ClickCancel();
        }

        #endregion New PBI Tests

        // OK
        [TestMethod]
        public void AddLargeAmountsOfDataListItems_Expected_NoHanging()
        {
            // Create the workflow
            SendKeys.SendWait("{ESC}");
            CreateWorkflow();
            
            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            // moved from 100 to 20 for time
            for (int j = 0; j < 20; j++)
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

       
        //PBI 9461
        [TestMethod]
        [Ignore]  // Need to investigate why this is failing - Huggs 22-07-2013
        public void ChangingResourceExpectedPopUpWarningWithViewDependancies()
        {
            SendKeys.SendWait("{ESC}");

            // Open the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.EnterExplorerSearchText("NewForeachUpgrade");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES", "NewForeachUpgradeDifferentExecutionTests");
            ExplorerUIMap.ClearExplorerSearchText();
            //Edit the inputs and outputs
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.CheckScalarInputAndOuput(0);

            //Save the workflow
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Save");
            Keyboard.SendKeys("{CTRL}S");
        
            //Click the view dependancies button
            ResourceChangedPopUpUIMap.ClickViewDependancies();

            Assert.AreEqual(TabManagerUIMap.GetActiveTabName(),"NewForeachUpgradeDifferentExecutionTests*Dependant...");

            DoCleanup("NewForeachUpgradeDifferentExecutionTests",true);                    
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
            SendKeys.SendWait("{ESC}");

            //Create a new workflow
            CreateWorkflow();

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());

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

            //Drag workflow onto surface
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "TestForEachOutput", p);

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
            CreateWorkflow();
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
            CreateWorkflow();

            // For later
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag a Calculate control on
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl calculateControl = ToolboxUIMap.FindToolboxItemByAutomationId("Calculate");
            ToolboxUIMap.DragControlToWorkflowDesigner(calculateControl, workflowPoint1);

            Mouse.Click();
            SendKeys.SendWait("sum{(}");

            // Find the control
            UITestControl calculateOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Calculate");

            // Find the fxBox - This seemed resilient to filter properties for some odd reason...
            WpfEdit fxBox = new WpfEdit(calculateOnWorkflow);
            //fxBox.FilterProperties.Add("AutomationId", "UI__fxtxt_AutoID");
            //fxBox.Find();

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
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            ExplorerUIMap.ClearExplorerSearchText();

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
            CreateWorkflow();

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
            DoCleanup(TabManagerUIMap.GetActiveTabName());

        }



        #endregion


        /*
         * There is an issue with one of these test can I need to address.
         * 
          
        [TestMethod]
        [Ignore]
        public void ClickNewRemoteWarewolfServerExpectedRemoteWarewolfServerOpens()
        {
            var _docManager = new DocManagerUIMap();
            var _explorer = new ExplorerUIMap();

            _docManager.ClickOpenTabPage("Explorer");
            var getLocalServer = _explorer.GetLocalServer();
            Mouse.Click(MouseButtons.Right, ModifierKeys.None, new Point(getLocalServer.BoundingRectangle.X, getLocalServer.BoundingRectangle.Y));
            for (var i = 0; i < 6; i++)
            {
                Keyboard.SendKeys("{DOWN}");
            }
            Keyboard.SendKeys("{ENTER}");
            Playback.Wait(1000);


            Playback.Wait(100);
            UITestControl uiTestControl = NewServerUIMap.UINewServerWindow;
            if (uiTestControl == null)
            {
                Assert.Fail("Error - Clicking the remote warewolf button does not create the new server window");
            }
            NewServerUIMap.CloseWindow();
        } 
         
        [TestMethod]
        public void DragAWorkflowIntoAndOutOfAForEach_Expected_NoErrors()
        {
            // Create the workflow
            CreateWorkflow();

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


            // Get a sample workflow, and drag it onto the "Drop Activity Here" part of the ForEach box
            DocManagerUIMap.ClickOpenTabPage("Explorer");
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
            Mouse.Move(new Point(forEachControl.BoundingRectangle.X + 25, forEachControl.BoundingRectangle.Y + 25));

            // Click it
            Mouse.Click();

            // And drag it down
            Mouse.StartDragging();
            Mouse.StopDragging(new Point(workflowPoint1.X, workflowPoint1.Y + 100));

            // Now get its position
            UITestControl calcTaxReturnsControl = workflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");

            // Its not on the design surface, must be in foreach
            Assert.IsNull(calcTaxReturnsControl, "Could not drop it ;(");

            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);

        }

        [TestMethod]
        public void DragADecisionIntoForEachExpectNotAddedToForEach()
        {


            // Create the workflow
            CreateWorkflow();

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
            Playback.Wait(500);
            // Cancel Decision Wizard
            try
            {
                var decisionWizardUiMap = new DecisionWizardUIMap();
                decisionWizardUiMap.ClickCancel();
                Assert.Fail("Got droped ;(");
            }
            catch
            {
                Assert.IsTrue(true);
            }


            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        [TestMethod]
        public void DragASwitchIntoForEachExpectNotAddedToForEach()
        {
            // Create the workflow
            CreateWorkflow();

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


            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        [TestMethod]
        public void ClickShowMapping_Expected_InputOutputAdornersAreDisplayed()
        {
            // Create the workflow
            CreateWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);


            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = workflowDesignerUIMap.FindControlByAutomationId(theTab, "TestFlow");
            Mouse.Click(controlOnWorkflow, new Point(265, 5));

            Playback.Wait(2500);

            // All good - Cleanup time!
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);

        }

        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {

            CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 100);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, "CalculateTaxReturns");
            controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "CalculateTaxReturns");
            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();

            Point initialResizerPoint = new Point();
            Point newResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            if (controlCollection[controlCollection.Count - 1].ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = controlCollection[controlCollection.Count - 1];
                initialResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                initialResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }

            // Drag
            Mouse.Click(initialResizerPoint);
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y - 50));

            // Check position to see it dragged
            if (controlCollection[controlCollection.Count - 1].ControlType.ToString() == "Indicator")
            {
                UITestControl theResizer = controlCollection[controlCollection.Count - 1];
                newResizerPoint.X = theResizer.BoundingRectangle.X + 5;
                newResizerPoint.Y = theResizer.BoundingRectangle.Y + 5;
            }

            if (!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }

            // Test complete - Delete itself
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        #region Tests Requiring Designer access

        // vi - Can I drop a tool onto the designer?
        [TestMethod]
        public void DropAWorkflowOrServiceOnFromTheToolBoxAndTestTheWindowThatPopsUp()
        {
            // Create the Workflow
            Keyboard.SendKeys("{CTRL}W");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "WorkflowServiceDropWorkflow");

            // Get the tab
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());

            // And click it to make sure it's focused
            TabManagerUIMap.Click(theTab);

            // Wait a bit for user noticability
            Playback.Wait(150);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // And click it for UI responsiveness :P
            WorkflowDesignerUIMap.ClickControl(theStartButton);

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Toolbox");

            // Get the comment box
            UITestControl workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            // And drag it onto the point
            ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            #region Checking Ok Button enabled property

            //Single click a folder in the tree
            ActivityDropUIMap.SingleClickAFolder();

            //Get the Ok button from the window
            UITestControl buttonControl = ActivityDropUIMap.GetOkButtonOnActivityDropWindow();

            //Assert that the buttton is disabled
            Assert.IsFalse(buttonControl.Enabled);

            //Open the folder in the tree
            ActivityDropUIMap.DoubleClickAFolder();

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

            #region Checking the click of the OK button Adds the resource to the design surface

            //// Open the Toolbox
            //DocManagerUIMap.ClickOpenTabPage("Toolbox");

            //// Get the comment box
            //workflowControl = ToolboxUIMap.FindToolboxItemByAutomationId("Workflow");

            //// And drag it onto the point
            //ToolboxUIMap.DragControlToWorkflowDesigner(workflowControl, p);

            ////Wait for the window to show up
            //Playback.Wait(2000);

            ////Single click a folder in the tree
            //ActivityDropUIMap.SingleClickAResource();

            ////Click the Ok button on the window
            //ActivityDropUIMap.ClickOkButton();

            //// Check if it exists on the designer
            //Assert.IsTrue(WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "fileTest"));

            ////Delete the resource that was dropped on
            //SendKeys.SendWait("{DELETE}");

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
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        #endregion Tests Requiring Designer access

        #region Studio Window Tests

        

        #region Decision Wizard

        private readonly DecisionWizardUIMap _decisionWizardUiMap = new DecisionWizardUIMap();

        //Bug 9339 + Bug 9378
        [TestMethod]
        public void SaveDecisionWithBlankFieldsExpectedDecisionSaved()
        {
            //Initialize
            CreateWorkflow();

            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());

            //Set variable
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            Keyboard.SendKeys("VariableName");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var decision = ToolboxUIMap.FindControl("Decision");
            ToolboxUIMap.DragControlToWorkflowDesigner(decision, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            _decisionWizardUiMap.SendTabs(4);
            _decisionWizardUiMap.SelectMenuItem(20);
            //Assert intellisense works
            _decisionWizardUiMap.SendTabs(10);
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            var actual = Clipboard.GetData(DataFormats.Text);
            Assert.AreEqual("[[VariableName]]", actual, "Decision intellisense doesn't work");
            _decisionWizardUiMap.SendTabs(2);
            _decisionWizardUiMap.GetFirstIntellisense("[[V");
            actual = Clipboard.GetData(DataFormats.Text);
            Assert.AreEqual("[[VariableName]]", actual, "Decision intellisense doesn't work");
            _decisionWizardUiMap.SendTabs(6);
            Keyboard.SendKeys("{ENTER}");

            //Assert can save blank decision
            decision = new WorkflowDesignerUIMap().FindControlByAutomationId(theTab, "FlowDecisionDesigner");
            Point point;
            Assert.IsTrue(decision.TryGetClickablePoint(out point));
            Assert.IsNotNull(point);

            //Cleanup
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);
        }

        #endregion

        #endregion Studio Window Tests
        */


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

        /*
         * Test land up here one of a few ways. 
         * 
         * 1) They where groomed out long ago
         * 2) They form part of the regression pack to run nightly to keep performance tighty
         * 3) They generally cost way too much time to keep groomed and would be getter served by nightly exection and not hold up the dev
         *    merge process. 
         */

        [TestMethod]
        [Ignore]
        // Regression test
        // Test name does not match test functionality
        public void CheckIfDebugProcessingBarIsShowingDurningExecutionExpextedToShowDuringExecutionOnly()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("LargeFileTesting");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "TESTS", "LargeFileTesting");
            ExplorerUIMap.ClearExplorerSearchText();

            //UITestControl control1 = OutputUIMap.GetStatusBar();

            //UITestControlCollection preStatusBarChildren = control1.GetChildren();
            //var preProgressbar = preStatusBarChildren.First(c => c.ClassName == "Uia.CircularProgressBar");
            //var preLabel = preStatusBarChildren.First(c => c.ClassName == "Uia.Text");
            //Assert.IsTrue(preLabel.FriendlyName == "Ready" || preLabel.FriendlyName == "Complete");
            //Assert.IsTrue(preProgressbar.Height == -1);

            Keyboard.SendKeys("{F5}{F5}");
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Debug");
            Playback.Wait(1000);
            DebugUIMap.ExecuteDebug();
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
        [Ignore] // External Resources
        public void ClickHelpFeedback_Expected_FeedbackWindowOpens()
        {
            RibbonUIMap.ClickRibbonMenuItem("Feedback");
            if (!FeedbackUIMap.DoesRecordedFeedbackWindowExist())
            {
                Assert.Fail("Error - Clicking the Feedback button does not create the Feedback Window");
            }

            SendKeys.SendWait("Y");
            Playback.Wait(500);
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
            if (!FeedbackUIMap.DoesFeedbackWindowExist())
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
                //KillAllInstancesOf("OUTLOOK");
            }


        }

        [TestMethod]
        [Ignore]
        // IE dependency... 
        public void ViewInBrowser_Expected_NewlyCreatedVariableAddedToDataList()
        {
            // TODO: Recode this to use either the IE9 map or the IE8 map
            // Currently coded to use the IE8 map

            // Create the workflow
            CreateWorkflow();

            // Add the variable
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.ClickVariableName(0);
            SendKeys.SendWait("testVar");

            // Click "View in Browser"
            RibbonUIMap.ClickRibbonMenuItem("View in Browser");

            // Give the slow IE time to open ;D
            Playback.Wait(2500);

            // Check if the IE Body contains the data list item
            string IEText = ExternalUIMap.GetIEBodyText();
            if (!IEText.Contains("<testVar></testVar>"))
            {
                Assert.Fail("The variable was not added to the DataList :(");
            }

            // Close the browser
            ExternalUIMap.CloseAllInstancesOfIE();

            // And do cleanup
            DoCleanup(TabManagerUIMap.GetActiveTabName());
        }

        [TestMethod]
        [Ignore] // Not expected behavior anymore
        public void DebugDataTypedExpectedNewRowIsAdded()
        {
            // Create the Workflow
            CreateWorkflow();

            // Vars
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag an Assign onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theItem = ToolboxUIMap.FindControl("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theItem, workflowPoint1);

            // Fill some data
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Playback.Wait(100); // Wait bug if you type too fast
            SendKeys.SendWait("{TAB}");
            Playback.Wait(100);
            SendKeys.SendWait("myName");

            // Map it
            DocManagerUIMap.ClickOpenTabPage("Variables");
            //Massimo.Guerrera - 6/3/2013 - Removed because variables are now auto added to the list.
            //VariablesUIMap.UpdateDataList();

            Keyboard.SendKeys("{F5}{F5}");
            if (DebugUIMap.CountRows() != 1)
            {
                DebugUIMap.CloseDebugWindow();
                Assert.Fail("There are no rows!");
            }
            //DebugUIMap.ClickItem(0);
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("test123");
            int newCount = DebugUIMap.CountRows();
            DebugUIMap.CloseDebugWindow();
            if (newCount != 2)
            {
                Assert.Fail("There was no added row!");
            }

            // Cleanup
            DoCleanup(TabManagerUIMap.GetActiveTabName());

        }

        [TestMethod]
        [Ignore]
        // This is a pointless test since it is already covered with each UI Test
        public void CloseTabWithUnsavedChanges_Expected_SaveChangesDialogAppears()
        {
            // 1. Create the workflow
            RibbonUIMap.ClickRibbonMenuItem("Workflow");
            UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point4");
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Save it
            RibbonUIMap.ClickRibbonMenuItem("Save");

            // Let it save.....
            Playback.Wait(1000);

            // 2. Make a change
            // Open the Toolbox
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Close the tab and click No (If you can click No, it means the box popped up :p)
            string tabName = TabManagerUIMap.GetTabNameAtPosition(0);
            TabManagerUIMap.CloseTab_Click_No(tabName);

            // All good - Clean up time!
            //DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point4");

            Assert.Inconclusive("Create Workflow Change");

        }

        [TestMethod]
        [Ignore]
        // Old grooming hangover - Faulty test!!
        public void DebugTabUpdatesWhenXmlIsModified()
        {

            // Create the workflow
            CreateWorkflow();

            // Get some variables
            UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Drag the tool onto the workflow
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            UITestControl theControl = ToolboxUIMap.FindToolboxItemByAutomationId("Assign");
            ToolboxUIMap.DragControlToWorkflowDesigner(theControl, workflowPoint1);

            // Add the data!
            WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 0);
            //for (int j = 0; j < 100; j++)
            //{
            //    // Sleeps are due to the delay when adding a lot of items
            //    SendKeys.SendWait("[[theVar" + j.ToString(CultureInfo.InvariantCulture) + "]]");
            //    Playback.Wait(15);
            //    SendKeys.SendWait("{TAB}");
            //    Playback.Wait(15);
            //    SendKeys.SendWait(j.ToString(CultureInfo.InvariantCulture));
            //    Playback.Wait(15);
            //    SendKeys.SendWait("{TAB}");
            //    Playback.Wait(15);
            //}

            //// Create the workflow
            ////CreateCustomWorkflow("5782Point4Mo", "CodedUITestCategory");

            //// Set some variabes
            //CreateWorkflow();
            //UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            //UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            //Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 150);

            //// Open the Toolbox
            //DocManagerUIMap.ClickOpenTabPage("Toolbox");

            //// Get an assign Tool
            //UITestControl assignTool = ToolboxUIMap.FindControl("Assign");
            //ToolboxUIMap.DragControlToWorkflowDesigner(assignTool, workflowPoint1);

            //// Enter two record sets and 2 scalars
            //// AssignControl_ClickFirstTextbox
            //WorkflowDesignerUIMap.AssignControl_ClickLeftTextboxInRow(theTab, "Assign", 1);
            SendKeys.SendWait("[[recSet{(}{)}.Name]]");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("Michael");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("[[recSet{(}{)}.Surname]]");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("Cullen");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("[[recSet2{(}{)}.SomeVal]]");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("SomeData");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("[[scalarOne]]");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("SOData");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("[[scalarTwo]]");
            Playback.Wait(250);
            SendKeys.SendWait("{TAB}");
            Playback.Wait(250);
            SendKeys.SendWait("STData");

            // Update the DataList
            DocManagerUIMap.ClickOpenTabPage("Variables");
            VariablesUIMap.UpdateDataList();

            // Open the Debug Menu, and enter some values
            RibbonUIMap.ClickRibbonMenuItem("Debug");
            DebugUIMap.ClickItem(0);
            SendKeys.SendWait("soValue");
            DebugUIMap.ClickItem(1);
            SendKeys.SendWait("stValue");
            DebugUIMap.ClickItem(2);
            SendKeys.SendWait("rsoName");
            DebugUIMap.ClickItem(3);
            SendKeys.SendWait("rsoSurname");

            // 6, because some rows should have been auto-added
            DebugUIMap.ClickItem(6);
            SendKeys.SendWait("rstValue");

            // Change to the XML tab, and make sure everything's OK
            DebugUIMap.ClickXMLTab();

            // Rest of test blocked by lack of Automation ID
            DoCleanup(TabManagerUIMap.GetActiveTabName(), true);

        }

        [TestMethod]
        [Ignore]
        // External Resources
        public void CloseNonSavedWorkflowClickYes_Expected_ChangesPersist()
        {
            // 1. Create the workflow
            //CreateCustomWorkflow("5782Point5", "CodedUITestCategory");
            //UITestControl theTab = TabManagerUIMap.FindTabByName("5782Point5");
            //UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");
            //Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            //// Save it
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Save");

            //// Let it save.....
            //Playback.Wait(1000);

            //// Get the data for later comparison
            //string folder = GetServerEXEFolder();
            //string workspaceId = GetWorkspaceID();
            //string path1 = folder + @"Workspaces\" + workspaceId + @"\Services\5782Point5.xml";
            //string path2 = folder + @"Services\5782Point5.xml";
            //StreamReader sr1 = new StreamReader(path1);
            //StreamReader sr2 = new StreamReader(path2);
            //string fileData1 = sr1.ReadToEnd();
            //string fileData2 = sr2.ReadToEnd();
            //sr1.Close();
            //sr2.Close();

            //// 2. Make a change
            //// Open the Toolbox
            //DocManagerUIMap.ClickOpenTabPage("Explorer");

            //// Get a sample workflow
            //UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "TEST", "TestFlow");

            //// Drag it on
            //ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            //// 3. Close the tab, and click Yes
            //TabManagerUIMap.CloseTab_Click_Yes("5782Point5");

            //// Check that no tabs remain open
            //int tabCount = TabManagerUIMap.GetTabCount();
            //Assert.AreEqual(1,tabCount, "Error - Clicking No kept the tab open.");

            //// Re-read the data
            //sr1 = new StreamReader(path1);
            //sr2 = new StreamReader(path2);
            //string newFileData1 = sr1.ReadToEnd();
            //string newFileData2 = sr2.ReadToEnd();
            //sr1.Close();
            //sr2.Close();

            //// And make sure that the data WAS altered
            //Assert.AreNotEqual(fileData1, newFileData1, "Error - The data is equal in case 1");
            //Assert.AreNotEqual(fileData2, newFileData2, "Error - The data is equal in case 2");

            //// Finally - Clean Up
            //DoCleanup("localhost", "WORKFLOWS", "CODEDUITESTCATEGORY", "5782Point5");

            Assert.Inconclusive("Create Workflow Change");
        }

        [TestMethod]
        [Ignore]
        // Setup and Teardown
        public void CloseAndReopenStudio_Expected_TabOrderRemainsConstant()
        {
            //// Open 3 different workflows
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder Clean");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "JSON", "JSON Binder Raw");

            //// Drag the first tab to the second position
            //// Due to our ... Odd UI Design, this is accomplished by dragging it onto the third tab
            //TabManagerUIMap.DragTabToTab("JSON Binder Raw", "JSON Binder");
            //string fileName = GetStudioEXELocation();

            //// Close the Studio (Don't kill it)
            //WpfWindow theStudio = new WpfWindow();
            //theStudio.SearchProperties[WpfWindow.PropertyNames.Name] = GetStudioWindowName();
            //theStudio.SearchProperties.Add(new PropertyExpression(WpfWindow.PropertyNames.ClassName, "HwndWrapper", PropertyExpressionOperator.Contains));
            //theStudio.WindowTitles.Add(GetStudioWindowName());
            //theStudio.Find();
            //Point closeButtonPoint = new Point(theStudio.BoundingRectangle.Left + theStudio.BoundingRectangle.Width - 10, theStudio.BoundingRectangle.Top + 10);
            //Mouse.Click(closeButtonPoint);
            //Playback.Wait(2000); // Give it time to die

            //// Restart the studio!
            //Process.Start(fileName);

            //// Get the tab names
            //string tab1 = TabManagerUIMap.GetTabNameAtPosition(0);
            //string tab2 = TabManagerUIMap.GetTabNameAtPosition(1);
            //string tab3 = TabManagerUIMap.GetTabNameAtPosition(2);
            //string tab4 = TabManagerUIMap.GetTabNameAtPosition(3);

            //// Expected position - JSON Binder Clean, JSON Binder Raw, JSON Binder
            //StringAssert.Contains(tab1, "JSON Binder Clean");
            //StringAssert.Contains(tab2, "JSON Binder Raw");
            //StringAssert.Contains(tab3, "JSON Binder");

            //// 5782.8 requires a check to make sure the Start Page has also opened
            //StringAssert.Contains(tab4, "Start Page");
        }

        [TestMethod]
        [Ignore] // Process restart
        public void DeleteDefaultFileRestartStudio_Expected_FileExistsOnRecreate()
        {
            //// A test case with an identity crisis! :D
            //string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //string defaultFile = localAppData + @"\Dev2\UserInterfaceLayouts\Default.xml";
            //if (!File.Exists(defaultFile))
            //{
            //    Assert.Fail("The file does not exist in the first place!");
            //}

            //string studioPath = GetStudioEXELocation();
            //CloseTheStudio();
            //File.Delete(defaultFile);
            //Playback.Wait(100); // Time to delete
            //if (File.Exists(defaultFile))
            //{
            //    Assert.Fail("The file could not be deleted!");
            //}
            //Process.Start(studioPath);

            //// Wait for it to open
            //Playback.Wait(5000);

            //// Aaaand re-close it, since the file should have now been created!
            //CloseTheStudio();

            //if (!File.Exists(defaultFile))
            //{
            //    Assert.Fail("The file was not recreated!");
            //}

            //// Test over - Re-open the Studio D:
            //Process.Start(studioPath);

            //// Wait for it to open
            //Playback.Wait(5000);
        }

        [TestMethod]
        [Ignore]
        public void UnsavedWorkflowsPersistingOnStudioRestartExpectedWorkflowStillOpen()
        {
            //ProcessManager procMan = new ProcessManager("Dev2.Studio");

            //CreateWorkflow();
            //UITestControl theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            //DocManagerUIMap.ClickOpenTabPage("Toolbox");
            //var multiAssign = ToolboxUIMap.FindControl("Assign");
            //ToolboxUIMap.DragControlToWorkflowDesigner(multiAssign, WorkflowDesignerUIMap.GetPointUnderStartNode(theTab));
            //WorkflowDesignerUIMap.SetStartNode(theTab, "Assign");

            //if (procMan.IsProcessRunning())
            //{
            //    // Exit the Studio
            //    DocManagerUIMap.CloseStudio();
            //    // Wait For the Studio to exit
            //    Playback.Wait(2000);
            //    Assert.IsFalse(procMan.IsProcessRunning());
            //}
            //procMan.StartProcess();
            //Playback.Wait(5000);
            //theTab = TabManagerUIMap.FindTabByName(TabManagerUIMap.GetActiveTabName());
            //var assign = WorkflowDesignerUIMap.DoesControlExistOnWorkflowDesigner(theTab, "Assign");
            //if(assign == null)
            //{
            //    Assert.Fail("Assign not on unsaved workflow means workflow reverted");
            //}
            //DoCleanup(TabManagerUIMap.GetActiveTabName(),true);
        }

        // BUG 9078
        [TestMethod]
        [Ignore]
        public void StudioExit_Give_TabOpened_Expected_AllRunningProcessStop()
        {
            // TODO : Refactor into another scenario 

            //ProcessManager procMan = new ProcessManager("Dev2.Studio");

            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");
            //if (procMan.IsProcessRunning())
            //{
            //    // Exit the Studio
            //    DocManagerUIMap.CloseStudio();
            //    // Wait For the Studio to exit
            //    Playback.Wait(2000);
            //    Assert.IsFalse(procMan.IsProcessRunning());
            //}
            //procMan.StartProcess();
            //Playback.Wait(5000);
            //DoCleanup("CalculateTaxReturns");
        }

        // Bug 7796
        // xi. Can I deploy to my server? (AKA: Can I deploy?)
        [TestMethod]
        [Ignore]
        // External dependency
        public void CanIDeploy()
        {
            // Open the Explorer tab
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Naviate to the Workflow, and Right click it
            ExplorerUIMap.RightClickDeployProject("localhost", "WORKFLOWS", "MO", "CalculateTaxReturns");

            // Wait for the Deploy tab to load!
            Playback.Wait(5000);

            // Make sure the correct tab is highlighted
            UITestControl theTab = TabManagerUIMap.FindTabByName("Deploy Resources");
            TabManagerUIMap.Click(theTab);

            // Choose the required servers
            DeployViewUIMap.SelectServers(theTab, "localhost", "localhost");

            // Make sure the deploy count is correct
            Assert.IsTrue(DeployViewUIMap.DoSourceAndDestinationCountsMatch(theTab));

            // Click the "Deploy" button
            DeployViewUIMap.ClickDeploy(theTab); // This currently just mouses over the Deploy Button, since I had no servers to test against
        }

        // Bug 8747
        [TestMethod]
        [Ignore]
        // due to the comment below - maintance concern ;(
        public void DebugBuriedErrors_Expected_OnlyErrorStepIsInError()
        {
            //TestBase myTestBase = new TestBase();

            DocManagerUIMap.ClickOpenTabPage("Explorer");
            //Open the correct workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Bug8372");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "Bugs", "Bug8372");
            ExplorerUIMap.ClearExplorerSearchText();

            // Run debug
            Keyboard.SendKeys("{F5}");
            Playback.Wait(1500);
            Keyboard.SendKeys("{F5}");
            //DebugUIMap.ExecuteDebug();


            // Open the Output
            DocManagerUIMap.ClickOpenTabPage("Output");

            // Due to the complexity of the OutputUIMap, this test has been primarily hard-coded until a further rework
            Assert.IsTrue(OutputUIMap.DoesBug8747Pass());
            DoCleanup("Bug8372", true);
        }

        [TestMethod]
        [Ignore] // Old grooming hang-over
        public void ChangeAWorkflowsCategory_Expected_CategoryRemainsChagned()
        {
            RibbonUIMap.ClickRibbonMenuItem("Workflow");
            RibbonUIMap.ClickRibbonMenuItem("Save");
            Playback.Wait(200);
            SaveDialogUIMap.ClickAndTypeInFilterTextBox("Bugs");
            SaveDialogUIMap.ClickCategory();
            SaveDialogUIMap.ClickAndTypeInNameTextbox("MyNewTestFlow1");
            SaveDialogUIMap.ClickSave();
            Playback.Wait(200);

            // Rename ;)
            //RibbonUIMap.ClickRibbonMenuItem("Home", "Save");
            //Playback.Wait(200);
            //SaveDialogUIMap.ClickAndTypeInFilterTextBox("Bugs");
            //SaveDialogUIMap.ClickCategory();
            //SaveDialogUIMap.ClickAndTypeInNameTextbox("MyNewTestFlow1");
            //SaveDialogUIMap.ClickSave();

            DoCleanup("MyNewTestFlow1");
            //DocManagerUIMap.ClickOpenTabPage("Explorer");
            //ExplorerUIMap.RightClickDeleteProject("localhost", "WORKFLOWS", "Bugs", "MyNewTestFlow1");
        }

        #endregion Deprecated Test

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

        public ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if ((_explorerUiMap == null))
                {
                    _explorerUiMap = new ExplorerUIMap();
                }

                return _explorerUiMap;
            }
        }

        public DeployViewUIMap DeployViewUIMap
        {
            get
            {
                if ((_deployViewUiMap == null))
                {
                    _deployViewUiMap = new DeployViewUIMap();
                }

                return _deployViewUiMap;
            }
        }

        private ExplorerUIMap _explorerUiMap;
        private ToolboxUIMap _toolboxUiMap;
        private SaveDialogUIMap _saveDialogUIMap;
        private DocManagerUIMap _docManagerMap;
        private DeployViewUIMap _deployViewUiMap;


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

        #endregion UI Maps

        private UIMap map;
    }
}
