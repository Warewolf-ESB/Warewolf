using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Dev2.CodedUI.Tests;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
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

        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
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
