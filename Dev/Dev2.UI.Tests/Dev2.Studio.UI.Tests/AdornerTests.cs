using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    [CodedUITest]
    public class AdornerTests
    {

        [TestCleanup]
        public void TestCleanup()
        {
            //TabManagerUIMap.CloseAllTabs();
        }

        #region Properties

        private ExplorerUIMap _explorerUiMap;
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

        #region WorkflowDesigner UI Map

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if (_workflowDesignerUIMap == null)
                {
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        #endregion WorkflowDesigner UI Map

        #region DocManager UI Map

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

        #endregion

        #region Toolbox UI Map

        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if ((_toolboxUIMap == null))
                {
                    _toolboxUIMap = new ToolboxUIMap();
                }

                return _toolboxUIMap;
            }
        }

        private ToolboxUIMap _toolboxUIMap;

        #endregion

        #region Ribbon UI Map

        public RibbonUIMap RibbonUIMap
        {
            get
            {
                if((_ribbonUIMap == null))
                {
                    _ribbonUIMap = new RibbonUIMap();
                }

                return _ribbonUIMap;
            }
        }

        private RibbonUIMap _ribbonUIMap;

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ExternalService_EditService")]
        public void ExternalService_EditService_EditWithNoSecondSaveDialog_ExpectOneDialog()
        {
            //------------Setup for test--------------------------
            // Open the workflow
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("Edit Service Workflow");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "UI TEST", "Edit Service Workflow");

            //------------Execute Test---------------------------

            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();

            //Get Adorner buttons
            var button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "TravsTestService", "Edit");

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
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "FetchCities", "Edit");

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
            button = WorkflowDesignerUIMap.Adorner_GetButton(theTab, "DummyService", "Edit");

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


        [TestMethod]
        [TestCategory("UITest")]
        [Description("Test that clicking on the help button does indeed open an example workflow")]
        [Owner("Tshepo")][Ignore]//Ashley: WORKING OK - Bring back in when all the tests are OK like this one
        public void AdornerHelpButtonOpenAnExampleWorlkflowTest()
        {
            // Create the workflow
            RibbonUIMap.CreateNewWorkflow();
            // Get some design surface
            UITestControl theTab = TabManagerUIMap.GetActiveTab();
            //Get a point
            Point requiredPoint = WorkflowDesignerUIMap.GetPointUnderStartNode(theTab);
            //Open toolbox tab
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
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
    }
}
