using System.Drawing;
using System.Windows.Forms;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests : UIMapBase
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExternalService_EditService")]
        public void ExternalService_EditService_EditWithNoSecondSaveDialog_ExpectOneDialog()
        {
            //------------Setup for test--------------------------
            // Open the workflow
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Edit Service Workflow");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", "Edit Service Workflow");
            Playback.Wait(5000);

            //------------Execute Test---------------------------

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Get Adorner buttons
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TravsTestService", "Edit");

            // -- DO DB Services --

            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUIMap.ClickMappingTab();
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            // -- wizard closed
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");


            // -- DO Web Services --

            //Get Adorner buttons
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "FetchCities", "Edit");

            // move to show adorner buttons ;)
            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);

            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUIMap.ClickMappingTab();

            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}{TAB}{ENTER}{TAB}{ENTER}");
            Playback.Wait(1000);

            if(ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                ResourceChangedPopUpUIMap.ClickCancel();
            }

            // -- DO Plugin Services --

            //Get Adorner buttons
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DummyService", "Edit");

            // move to show adorner buttons ;)
            WorkflowDesignerUIMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);

            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUIMap.ClickMappingTab(); // click on the second tab ;)
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            Playback.Wait(500);
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("{ENTER}");


            //------------Assert Results-------------------------

            // check services for warning icon to incidate mappings out of date ;)

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "TravsTestService(ServiceDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "FetchCities(ServiceDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "DummyService(ServiceDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

        }


        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test that clicking on the help button does indeed open an example workflow")]
        [Owner("Tshepo")]
        public void AdornerHelpButtonOpenAnExampleWorlkflowTest()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            //Get a point
            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            //Open toolbox tab
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", requiredPoint);
            //Get Adorner buttons
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign", "Open Help");
            Mouse.Click(button);
            //Get 'View Sample' link button
            var findViewSampleLink = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "View Sample Workflow");
            Mouse.Click(findViewSampleLink.GetChildren()[0]);

            //Wait for sample workflow
            UITestControl waitForTabToOpen = null;
            var count = 10;
            while (waitForTabToOpen == null && count > 0)
            {
                waitForTabToOpen = TabManagerUIMap.FindTabByName("Utility - Assign");
                Playback.Wait(500);
                count--;
            }

            //Assert workflow opened after a time out.
            Assert.IsNotNull(waitForTabToOpen);
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
            DockManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText(resourceToUse);
            UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", resourceToUse);

            // Drag it on
            ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Prepare it
            Mouse.StartDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollBar(theTab));
            Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollDown(theTab));
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            var mappingsBtn = WorkflowDesignerUIMap.Adorner_GetButton(theTab, resourceToUse, "Open Mapping") as WpfToggleButton;
            if (mappingsBtn != null && !mappingsBtn.Pressed)
            {
                Mouse.Click(mappingsBtn);
            }

            UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();
            Point initialResizerPoint = new Point();
            // Validate the assumption that the last child is the resizer
            var resizeThumb = controlCollection[controlCollection.Count - 1];
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                initialResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                initialResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }
            else
            {
                Assert.Fail("Cannot find resize indicator");
            }

            // Drag
            Mouse.Move(new Point(resizeThumb.Left + 5, resizeThumb.Top + 5));
            Mouse.StartDragging();

            // Y - 50 since it starts at the lowest point
            Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y - 50));

            // Check position to see it dragged
            Point newResizerPoint = new Point();
            if(resizeThumb.ControlType.ToString() == "Indicator")
            {
                newResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                newResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
            }

            if(!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
            {
                Assert.Fail("The control was not resized properly.");
            }

            // Test complete - Delete itself
            TabManagerUIMap.CloseAllTabs();
        }

        //PBI 9939
        [TestMethod]
        [TestCategory("DsfActivityTests")]
        [Description("Testing when a DsfActivity is dropped onto the design surface that the mapping auto expands.")]
        [Owner("Massimo Guerrera")]
        // ReSharper disable InconsistentNaming
        public void ServiceDesigner_CodedUI_DroppingActivityOntoDesigner_MappingToBeExpanded()
        // ReSharper restore InconsistentNaming
        {
            //Create a new workflow
            RibbonUIMap.CreateNewWorkflow();

            // Get the tab
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            // Wait a bit for user noticability            
            Playback.Wait(500);

            // Get the location of the Start button
            UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button
            Point p = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 200);

            // Open the Explorer
            DockManagerUIMap.ClickOpenTabPage("Explorer");

            //Drag workflow onto surface
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("TestForEachOutput");
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "WORKFLOWS", "MO", "TestForEachOutput", p);

            //Get Mappings button
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TestForEachOutput", "Close Mapping") as WpfToggleButton;

            //Do Assert
            Assert.IsNotNull(button, "Mappings do not expand on activity drop");
        }

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
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
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
        }
    }
}
