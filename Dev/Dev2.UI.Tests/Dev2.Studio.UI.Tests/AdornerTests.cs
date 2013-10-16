using System.Drawing;
using System.Windows.Forms;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DatabaseServiceWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUiMap.CloseAllTabs();
        }

        #region Properties

        private ExplorerUIMap _explorerUiMap;
        public ExplorerUIMap ExplorerUiMap
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

        #endregion

        #region UI Maps

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUiMap
        {
            get
            {
                if(_tabManagerUiMap == null)
                {
                    _tabManagerUiMap = new TabManagerUIMap();
                }

                return _tabManagerUiMap;
            }
        }

        private TabManagerUIMap _tabManagerUiMap;

        #endregion TabManager UI Map

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUiMap
        {
            get
            {
                if(_workflowDesignerUiMap == null)
                {
                    _workflowDesignerUiMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUiMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUiMap;

        #endregion WorkflowDesigner UI Map

        #region DocManager UI Map

        public DocManagerUIMap DocManagerUiMap
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

        public ToolboxUIMap ToolboxUiMap
        {
            get
            {
                if((_toolboxUiMap == null))
                {
                    _toolboxUiMap = new ToolboxUIMap();
                }

                return _toolboxUiMap;
            }
        }

        private ToolboxUIMap _toolboxUiMap;

        #endregion

        #region Ribbon UI Map

        private RibbonUIMap _ribbonUiMap;
        public RibbonUIMap RibbonUiMap
        {
            get
            {
                if((_ribbonUiMap == null))
                {
                    _ribbonUiMap = new RibbonUIMap();
                }

                return _ribbonUiMap;
            }
        }

        private DatabaseServiceWizardUIMap _databaseServiceWizardUiMap;
        public DatabaseServiceWizardUIMap DatabaseServiceWizardUiMap { get { return _databaseServiceWizardUiMap ?? (_databaseServiceWizardUiMap = new DatabaseServiceWizardUIMap()); } }

        #endregion
        
        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExternalService_EditService")]
        public void ExternalService_EditService_EditWithNoSecondSaveDialog_ExpectOneDialog()
        {
            //------------Setup for test--------------------------
            // Open the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText("Edit Service Workflow");
            ExplorerUiMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", "Edit Service Workflow");

            //------------Execute Test---------------------------

            // Get some design surface
            UITestControl theTab = TabManagerUiMap.GetActiveTab();

            //Get Adorner buttons
            var button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "TravsTestService", "Edit");

            // -- DO DB Services --

            WorkflowDesignerUiMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUiMap.ClickMappingTab();
            SendKeys.SendWait("{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            // -- wizard closed
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            

            // -- DO Web Services --

            //Get Adorner buttons
            button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "FetchCities", "Edit");

            // move to show adorner buttons ;)
            WorkflowDesignerUiMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);

            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUiMap.ClickMappingTab();
            //Mouse.Click(new Point(780, 270)); // click on the second tab ;)
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}{TAB}{ENTER}{TAB}{ENTER}");
            Playback.Wait(1000);


            // -- DO Plugin Services --

            //Get Adorner buttons
            button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "DummyService", "Edit");

            // move to show adorner buttons ;)
            WorkflowDesignerUiMap.MoveMouseForAdornersToAppear(button.BoundingRectangle);

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);

            WizardsUIMap.WaitForWizard();

            DatabaseServiceWizardUiMap.ClickMappingTab(); // click on the second tab ;)
            SendKeys.SendWait("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            SendKeys.SendWait("zzz");
            // -- wizard closed, account for darn dialog ;(
            SendKeys.SendWait("{TAB}{TAB}{ENTER}");


            //------------Assert Results-------------------------

            // check services for warning icon to incidate mappings out of date ;)

            if(!WorkflowDesignerUiMap.Adorner_ClickFixErrors(theTab, "TravsTestService(DsfActivityDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUiMap.Adorner_ClickFixErrors(theTab, "FetchCities(DsfActivityDesigner)"))
            {
                Assert.Fail("'Fix Errors' button not visible");
            }

            if(!WorkflowDesignerUiMap.Adorner_ClickFixErrors(theTab, "DummyService(DsfActivityDesigner)"))
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
            RibbonUiMap.CreateNewWorkflow();
            // Get some design surface
            UITestControl theTab = TabManagerUiMap.GetActiveTab();
            //Get a point
            Point requiredPoint = WorkflowDesignerUiMap.GetPointUnderStartNode(theTab);
            //Open toolbox tab
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a control to the design surface
            ToolboxUiMap.DragControlToWorkflowDesigner("Assign", requiredPoint);
            //Get Adorner buttons
            var button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "Assign", "Open Help");
            Mouse.Click(button);
            //Get 'View Sample' link button
            var findViewSampleLink = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, "View Sample Workflow");
            Mouse.Click(findViewSampleLink.GetChildren()[0]);

            //Wait for sample workflow
            UITestControl waitForTabToOpen = null;
            var count = 10;
            while (waitForTabToOpen == null && count > 0)
            {
                waitForTabToOpen = TabManagerUiMap.FindTabByName("Utility - Assign");
                Playback.Wait(500);
                count--;
            }

            //Assert workflow opened after a time out.
            Assert.IsNotNull(waitForTabToOpen);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("HelpButtonAdorner_CollapseHelp")]
        public void WorkflowdesignSurface_CollapseHelp()
        {
            //New workflow with a multiassign on it
            RibbonUiMap.CreateNewWorkflow();
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var theTab = TabManagerUiMap.GetActiveTab();
            var pointUnderStartNode = WorkflowDesignerUiMap.GetPointUnderStartNode(theTab);
            ToolboxUiMap.DragControlToWorkflowDesigner("Assign", pointUnderStartNode);
            //click expand help
            Mouse.Click(WorkflowDesignerUiMap.GetOpenHelpButton(theTab, "Assign"));
            Assert.IsTrue(WorkflowDesignerUiMap.GetHelpPane(theTab, "Only variables go in here").Exists);
        }

        [TestMethod]
        public void ResizeAdornerMappings_Expected_AdornerMappingIsResized()
        {
            const string resourceToUse = "CalculateTaxReturns";
            RibbonUiMap.CreateNewWorkflow();

            UITestControl theTab = TabManagerUiMap.GetActiveTab();
            UITestControl theStartButton = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, "Start");

            // Get a point underneath the start button for the workflow
            Point workflowPoint1 = new Point(theStartButton.BoundingRectangle.X, theStartButton.BoundingRectangle.Y + 100);

            // Open the Explorer
            DocManagerUIMap.ClickOpenTabPage("Explorer");

            // Get a sample workflow
            ExplorerUiMap.ClearExplorerSearchText();
            ExplorerUiMap.EnterExplorerSearchText(resourceToUse);
            UITestControl testFlow = ExplorerUiMap.GetService("localhost", "WORKFLOWS", "MO", resourceToUse);

            // Drag it on
            ExplorerUiMap.DragControlToWorkflowDesigner(testFlow, workflowPoint1);

            // Click it
            UITestControl controlOnWorkflow = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, resourceToUse);
            Mouse.Click(controlOnWorkflow, new Point(5, 5));
            WorkflowDesignerUiMap.Adorner_ClickMapping(theTab, resourceToUse);
            controlOnWorkflow = WorkflowDesignerUiMap.FindControlByAutomationId(theTab, resourceToUse);
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
            TabManagerUiMap.CloseAllTabs();
        }
    }
}
