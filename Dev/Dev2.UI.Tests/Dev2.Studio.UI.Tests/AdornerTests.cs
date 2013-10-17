using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.ResourceChangedPopUpUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
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

        #region Properties

        private ExplorerUIMap _explorerUIMap;
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

        #endregion

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

            if (ResourceChangedPopUpUIMap.WaitForDialog(5000))
            {
                var popupMap = new ResourceChangedPopUpUIMap();
                popupMap.ClickCancel();
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
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}{ENTER}");


            //------------Assert Results-------------------------

            // check services for warning icon to incidate mappings out of date ;)

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "TravsTestService(DsfActivityDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "FetchCities(DsfActivityDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUIMap.Adorner_ClickFixErrors(theTab, "DummyService(DsfActivityDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

        }


        [TestMethod][Ignore]//ashley: testing 17.10.2013
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

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Ashley Lewis")]
        [TestCategory("HelpButtonAdorner_CollapseHelp")]
        public void WorkflowdesignSurface_CollapseHelp()
        {
            //New workflow with a multiassign on it
            RibbonUIMap.CreateNewWorkflow();
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            var theTab = TabManagerUIMap.GetActiveTab();
            var pointUnderStartNode = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", pointUnderStartNode);
            //click expand help
            Mouse.Click(WorkflowDesignerUIMap.GetOpenHelpButton(theTab, "Assign"));
            Assert.IsTrue(WorkflowDesignerUIMap.GetHelpPane(theTab, "Only variables go in here").Exists);
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

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, resourceToUse);
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUIMap.Adorner_ClickMapping(theTab, resourceToUse);
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
        public void DsfActivityDesigner_CodedUI_DroppingActivityOntoDesigner_MappingToBeExpanded()
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
            UITestControl button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TestForEachOutput", "OpenMappingsToggle");

            // flakey bit of code, we need to wait ;)
            Playback.Wait(500);

            //Assert button is not null
            Assert.IsTrue(button != null, "Couldnt find the mapping button");

            //Get the close mappings image
            var children = button.GetChildren();
            var images = children.FirstOrDefault(c => c.FriendlyName == "Close Mappings");

            //Check that the mapping is open
            Assert.IsTrue(images.Height > -1, "The correct images isnt visible which means the mapping isnt open");
        }
    }
}
