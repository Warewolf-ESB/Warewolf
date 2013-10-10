using System.Drawing;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
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

        #region TabManager UI Map

        public TabManagerUIMap TabManagerUiMap
        {
            get
            {
                if (_tabManagerUiMap == null)
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
                if (_workflowDesignerUiMap == null)
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
                if ((_docManagerMap == null))
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
                if ((_toolboxUiMap == null))
                {
                    _toolboxUiMap = new ToolboxUIMap();
                }

                return _toolboxUiMap;
            }
        }

        private ToolboxUIMap _toolboxUiMap;

        #endregion

        #region Ribbon UI Map

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

        private RibbonUIMap _ribbonUiMap;

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

            // -- DO DB Services

            // move to show adorner buttons ;)
            Mouse.Move(new Point(1030, 445));

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);
            Mouse.Click(new Point(840, 270)); // click on the second tab ;)
            Keyboard.SendKeys("{TAB}");
            Playback.Wait(500);
            Keyboard.SendKeys("zzz");
            // -- wizard closed
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}{ENTER}");
            

            // -- DO Web Services

            //Get Adorner buttons
            button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "FetchCities", "Edit");

            // move to show adorner buttons ;)
            Mouse.Move(new Point(1030, 600));

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);
            Mouse.Click(new Point(780, 270)); // click on the second tab ;)
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            Keyboard.SendKeys("zzz");
            // -- wizard closed, account for darn dialog ;(
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{ENTER}{TAB}{ENTER}");
            Playback.Wait(1000);


            // DO Plugin Services

            //Get Adorner buttons
            button = WorkflowDesignerUiMap.Adorner_GetButton(theTab, "DummyService", "Edit");

            // move to show adorner buttons ;)
            Mouse.Move(new Point(1030, 740));

            Playback.Wait(500);
            Mouse.Click(button);
            Playback.Wait(1000);
            Mouse.Click(new Point(780, 270)); // click on the second tab ;)
            Keyboard.SendKeys("{TAB}{TAB}{TAB}{TAB}");
            Playback.Wait(500);
            Keyboard.SendKeys("zzz");
            // -- wizard closed, account for darn dialog ;(
            Keyboard.SendKeys("{TAB}{TAB}{ENTER}");
            

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
        [Owner("Tshepo")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
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
    }
}
