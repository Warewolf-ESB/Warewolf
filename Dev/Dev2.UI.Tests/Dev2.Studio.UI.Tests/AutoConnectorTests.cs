
using System.Drawing;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DocManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///     Summary description for CodedUITest1
    /// </summary>
    [CodedUITest]
    public class AutoConnectorTests
    {
        DecisionWizardUIMap _decisionWizardUIMap;
        DocManagerUIMap _docManagerMap;
        ExplorerUIMap _explorerUIMap;
        TabManagerUIMap _tabManagerDesignerUIMap;
        ToolboxUIMap _toolboxUIMap;
        WorkflowDesignerUIMap _workflowDesignerUIMap;
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

        public DecisionWizardUIMap DecisionWizardUIMap
        {
            get
            {
                if((_decisionWizardUIMap == null))
                {
                    _decisionWizardUIMap = new DecisionWizardUIMap();
                }

                return _decisionWizardUIMap;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            TabManagerUIMap.CloseAllTabs();
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragActivityOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();
            //Drag a control to the design surface
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragAToolOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragAToolOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag a tool to the design surface
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "DsfMultiAssignActivityDesigner");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a tool to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 300);
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert that the line was split
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragAnActivityOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag an activity to the design surface
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "DsfMultiAssignActivityDesigner");
            // Drag another service to over the line between two connectors
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 300);
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragADecisionOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            DocManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "DsfMultiAssignActivityDesigner");
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a decision to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 300);
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            DecisionWizardUIMap.ClickDone();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            DecisionWizardUIMap.ClickDone();
            //Assert start auto connector worked
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        public void AutoConnectorTests_DragADecisionOnStartAutoConnectorNode_ASecondConnectorIsCreated()
        {
            CreateWorkflow();
            DocManagerUIMap.ClickOpenTabPage("Toolbox");
            var point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            DecisionWizardUIMap.ClickDone();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector doesnt work");
        }

        public void CreateWorkflow()
        {
            Keyboard.SendKeys(DocManagerUIMap.UIBusinessDesignStudioWindow, "{CTRL}W");
        }
    }
}