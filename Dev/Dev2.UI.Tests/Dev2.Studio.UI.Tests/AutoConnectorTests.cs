using System.Collections.Generic;

using System;
using System.Drawing;
using Dev2.CodedUI.Tests.TabManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.DockManagerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ExplorerUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.RibbonUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.ToolboxUIMapClasses;
using Dev2.CodedUI.Tests.UIMaps.WorkflowDesignerUIMapClasses;
using Dev2.Studio.UI.Tests.UIMaps;
using Dev2.Studio.UI.Tests.UIMaps.DecisionWizardUIMapClasses;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Studio.UI.Tests
{
    /// <summary>
    ///    These are UI tests around the auto connectors
    /// </summary>
    [CodedUITest]
    public class AutoConnectorTests : UIMapBase
    {
        #region Tests

        [TestCleanup]
        public void TestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragActivityOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();
            //Drag a control to the design surface
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            CreateWorkflow();
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag a tool to the design surface
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            UITestControl control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a tool to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
            ToolboxUIMap.DragControlToWorkflowDesigner("Assign", point);
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert that the line was split
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAnActivityOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag an activity to the design surface
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");
            // Drag another service to over the line between two connectors
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("email service");
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
            ExplorerUIMap.DragControlToWorkflowDesigner("localhost", "SERVICES", "COMMUNICATION", "Email Service", point);
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            DockManagerUIMap.ClickOpenTabPage("Explorer");
            ExplorerUIMap.ClearExplorerSearchText();
            ExplorerUIMap.EnterExplorerSearchText("AutoConnectorResource");
            ExplorerUIMap.DoubleClickOpenProject("localhost", "WORKFLOWS", "BUGS", "AutoConnectorResource");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            //Drag a decision to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if (control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }

            WizardsUIMap.WaitForWizard();
            Playback.Wait(2000);
            DecisionWizardUIMap.ClickCancel();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(2, connectors.Count, "Connector line wasn't split");
        }

        [TestMethod][Ignore]//ashley: testing 17.10.2013
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnStartAutoConnectorNode_ASecondConnectorIsCreated()
        {
            CreateWorkflow();
            DockManagerUIMap.ClickOpenTabPage("Toolbox");
            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint();
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner("Decision", point);
            WizardsUIMap.WaitForWizard();
            Playback.Wait(2000);
            DecisionWizardUIMap.ClickCancel();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector doesnt work");
        }

        #endregion

        #region Utils

        public void CreateWorkflow()
        {
            RibbonUIMap.CreateNewWorkflow();
        }

        #endregion
    }
}