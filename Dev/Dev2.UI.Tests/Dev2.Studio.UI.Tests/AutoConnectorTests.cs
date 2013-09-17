using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;


namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    /// Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class AutoConnectorTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            TabManagerUIMap.CloseAllTabs();
        }

        public WorkflowDesignerUIMap WorkflowDesignerUIMap
        {
            get
            {
                if(_workflowDesignerUIMap == null)
                {
                    _workflowDesignerUIMap = new WorkflowDesignerUIMap();
                }

                return _workflowDesignerUIMap;
            }
        }

        private WorkflowDesignerUIMap _workflowDesignerUIMap;

        public DocManagerUIMap DocManagerUIMap
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

        private TabManagerUIMap _tabManagerDesignerUIMap;

        public TabManagerUIMap TabManagerUIMap
        {
            get
            {
                if((_tabManagerDesignerUIMap == null))
                {
                    _tabManagerDesignerUIMap = new TabManagerUIMap();
                }

                return _tabManagerDesignerUIMap;
            }
        }


        public ToolboxUIMap ToolboxUIMap
        {
            get
            {
                if((_toolboxUIMap == null))
                {
                    _toolboxUIMap = new ToolboxUIMap();
                }

                return _toolboxUIMap;
            }
        }


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

        private ToolboxUIMap _toolboxUIMap;

        private DocManagerUIMap _docManagerMap;
        ExplorerUIMap _explorerUIMap;

        [TestMethod]
        public void AutoConnetorTests_DragActivityOnStartAutoConnectorNode_ConnectorIsCreated()
        {
            CreateWorkflow();
            //Drag a control to the design surface
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            //Assert start auto connector worked
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            Assert.AreEqual(1, connectors.Count, "Start auto connector doesnt work");
        }

        //[TestMethod]
        //public void CodedUITestMethod1()
        //{
        //    CreateWorkflow();
        //    Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();

        //    //Open toolbox tab
        //    DocManagerUIMap.ClickOpenTabPage("Toolbox");
        //    //Drag a control to the design surface
        //    ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
        //    //ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);

        //    var connectors = WorkflowDesignerUIMap.GetAllConnectors();
        //    foreach(var uiTestControl in connectors)
        //    {
        //        uiTestControl.DrawHighlight();
        //    }
        //}

        public void CreateWorkflow()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}W");
        }
    }

}
