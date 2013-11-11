using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests : UIMapBase
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExternalService_EditService")]
        public void ExternalService_EditService_EditWithNoSecondSaveDialog_ExpectOneDialog()
        {
            try
            {
                //------------Setup for test--------------------------
                // Open the workflow
                DockManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText("Edit Service Workflow");
                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", "Edit Service Workflow");
                Playback.Wait(5000);

                var newMapping = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6);

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
                SendKeys.SendWait(newMapping);
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
                SendKeys.SendWait(newMapping);
                // -- wizard closed, account for darn dialog ;(
                SendKeys.SendWait("{TAB}{TAB}{TAB}{ENTER}{TAB}{ENTER}");
                Playback.Wait(1000);

                if (ResourceChangedPopUpUIMap.WaitForDialog(5000))
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
                SendKeys.SendWait(newMapping);
                Playback.Wait(500);
                // -- wizard closed, account for darn dialog ;(
                SendKeys.SendWait("{TAB}{TAB}");
                Playback.Wait(500);
                SendKeys.SendWait("{ENTER}");


                //------------Assert Results-------------------------

                // check services for warning icon to incidate mappings out of date ;)

                if (!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "TravsTestService"))
                {
                    Assert.Fail("'Fix Errors' button not visible");
                }

                if (!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "FetchCities"))
                {
                    Assert.Fail("'Fix Errors' button not visible");
                }

                if (!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "DummyService"))
                {
                    Assert.Fail("'Fix Errors' button not visible");
                }
            }
            finally
            {
                TabManagerUIMap.CloseAllTabs();
            }
        }


        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test that clicking on the help button does indeed open an example workflow")]
        [Owner("Tshepo")]
        public void AdornerHelpButtonOpenAnExampleWorlkflowTest()
        {
            try
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
            finally
            {
                TabManagerUIMap.CloseAllTabs();
            }
        }


        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {
            try
            {
                const string resourceToUse = "Bug_10528";
                const string innerResource = "Bug_10528_InnerWorkFlow";

                // Open the Explorer
                DockManagerUIMap.ClickOpenTabPage("Explorer");
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(resourceToUse);

                ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "INTEGRATION TEST SERVICES",
                                                     resourceToUse);
                UITestControl theTab = TabManagerUIMap.GetActiveTab();

                Mouse.StartDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollBar(theTab));
                Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollDown(theTab));

                UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, innerResource);
                Mouse.Move(controlOnWorkflow, new Point(5, 5));
                var mappingsBtn =
                    WorkflowDesignerUIMap.Adorner_GetButton(theTab, innerResource, "Open Mapping") as WpfToggleButton;

                if (mappingsBtn == null)
                {
                    Assert.Fail("Could not find mapping button");
                }

                Mouse.Click(mappingsBtn);

                UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();
                Point initialResizerPoint = new Point();
                // Validate the assumption that the last child is the resizer
                var resizeThumb = controlCollection[controlCollection.Count - 1];
                if (resizeThumb.ControlType.ToString() == "Indicator")
                {
                    if (resizeThumb.BoundingRectangle.X == -1)
                    {
                        Assert.Fail("Resize indicator is not visible");
                    }

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
                if (resizeThumb.ControlType.ToString() == "Indicator")
                {
                    newResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                    newResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
                }

                if (!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y < initialResizerPoint.Y))
                {
                    Assert.Fail("The control was not resized properly.");
                }
            }
            finally
            {
            // Test complete - Delete itself
                TabManagerUIMap.CloseAllTabs();
            }
        }


        [TestMethod]
        [TestCategory("DsfActivityTests")]
        [Description("Testing when a DsfActivity is dropped onto the design surface that the mapping auto expands and the resize control is visible")]
        [Owner("Travis Frisinger")]
        public void ResizeAdornerMappingsOnDrop_Expected_AdornerMappingIsResized()
        {
            try
            {
                const string resourceToUse = "CalculateTaxReturns";
                RibbonUIMap.CreateNewWorkflow();

                UITestControl theTab = TabManagerUIMap.GetActiveTab();
                UITestControl theStartButton = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "Start");

                // Get a point underneath the start button for the workflow
                Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X,
                                                 theStartButton.BoundingRectangle.Y + 100);

                // Open the Explorer
                DockManagerUIMap.ClickOpenTabPage("Explorer");

                // Get a sample workflow
                ExplorerUIMap.ClearExplorerSearchText();
                ExplorerUIMap.EnterExplorerSearchText(resourceToUse);
                UITestControl testFlow = ExplorerUIMap.GetService("localhost", "WORKFLOWS", "MO", resourceToUse);

                // Drag it on
                ExplorerUIMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

                // Scroll down (for if the screen resolution is low)
                var scrollBar = WorkflowDesignerUIMap.ScrollViewer_GetScrollBar(theTab);
                if (scrollBar != null)
                {
                    Mouse.StartDragging(scrollBar);
                    Mouse.StopDragging(WorkflowDesignerUIMap.ScrollViewer_GetScrollDown(theTab));
                }

                UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
                Mouse.Click(controlOnWorkflow, new Point(5, 5));

                UITestControlCollection controlCollection = controlOnWorkflow.GetChildren();
                Point initialResizerPoint = new Point();
                // Validate the assumption that the last child is the resizer
                var resizeThumb = controlCollection[controlCollection.Count - 1];
                if (resizeThumb.ControlType.ToString() == "Indicator")
                {
                    if (resizeThumb.BoundingRectangle.X == -1)
                    {
                        Assert.Fail("Resize indicator is not visible");
                    }

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
                Mouse.StopDragging(new Point(initialResizerPoint.X + 50, initialResizerPoint.Y + 50));

                // Check position to see it dragged
                Point newResizerPoint = new Point();
                if (resizeThumb.ControlType.ToString() == "Indicator")
                {
                    newResizerPoint.X = resizeThumb.BoundingRectangle.X + 5;
                    newResizerPoint.Y = resizeThumb.BoundingRectangle.Y + 5;
                }

                if (!(newResizerPoint.X > initialResizerPoint.X) || !(newResizerPoint.Y > initialResizerPoint.Y))
                {
                    Assert.Fail("The control was not resized properly.");
                }
            }
            finally
            {
                // Test complete - Delete itself
                TabManagerUIMap.CloseAllTabs();
            }
        }

        // PBI 8601 (Task 8855)
        [TestMethod]
        public void QuickVariableInputFromListTest()
        {
            try
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
                UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "Assign",
                                                                               "Open Quick Variable Input");

                // Click it
                Mouse.Move(new Point(button.BoundingRectangle.X + 5, button.BoundingRectangle.Y + 5));
                Mouse.Click();

                // Enter some invalid data
                WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_EnterData(theTab, "Assign", ",",
                                                                                        "some(<).", "_suf",
                                                                                        "varOne,varTwo,varThree");

                // Click done
                WorkflowDesignerUIMap.AssignControl_QuickVariableInputControl_ClickAdd(theTab, "Assign");

                var errorControl = WorkflowDesignerUIMap.FindControlByAutomationId(theTab,
                                                                                   "Prefix contains invalid characters");
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
            finally
            {
                TabManagerUIMap.CloseAllTabs();
            }
        }
    }
}
