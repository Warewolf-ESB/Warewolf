
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
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

        #region Cleanup

        [TestInitialize]
        public void TestInit()
        {
            Init();
        }


        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
            Halt();
        }

        #endregion

        #region Tests

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragActivityOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            UITestControl theTab = CreateWorkflow();
            ExplorerUIMap.DragResourceOntoWorkflowDesigner(theTab, "Email Service", "Integration Test Resources");
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnStartAutoConnectorNode_AConnectorIsCreated()
        {
            UITestControl theTab = CreateWorkflow();

            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, point);
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            Point newPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(theTab);
            if(point != newPoint)
            {
                WorkflowDesignerUIMap.DragControl("Assign", newPoint);
            }
            List<UITestControl> connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector was not created");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAToolOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag a tool to the design surface
            var theTab = ExplorerUIMap.DoubleClickWorkflow("AutoConnectorResource", "UI Test Resources");
            UITestControl control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "MultiAssignDesigner");

            //Drag a tool to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if(control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Assign, point);
                //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                if(point != newPoint)
                {
                    var theControl = WorkflowDesignerUIMap.GetAllControlsOnDesignSurface(theTab)[5];
                    Mouse.StartDragging(theControl, new Point(10, 10));
                    Mouse.StopDragging(newPoint);
                }
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert that the line was split
            Assert.IsTrue(connectors.Count >= 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragAnActivityOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {
            //Drag an activity to the design surface
            var theTab = ExplorerUIMap.DoubleClickWorkflow("AutoConnectorResource", "UI Test Resources");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(TabManagerUIMap.GetActiveTab(), "MultiAssignDesigner");

            // slow it down so it works ;)
            Mouse.MouseMoveSpeed = 500;
            Mouse.MouseDragSpeed = 500;

            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if(control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 140);
                ExplorerUIMap.DragResourceOntoWorkflowDesigner(theTab, "Email Service", "Communication", "localhost", point);
                if(WorkflowDesignerUIMap.TryCloseMappings("Email Service"))
                {
                    //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                    var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 140);
                    if(point != newPoint)
                    {
                        WorkflowDesignerUIMap.DragControl("Email Service", new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 40));
                    }
                }
                else
                {
                    //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
                    var newPoint = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 140);
                    if(point != newPoint)
                    {
                        WorkflowDesignerUIMap.DragControl("Email Service", newPoint);
                    }
                }
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.IsTrue(connectors.Count >= 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnALineBetweenConnectors_ASecondConnectorIsCreated()
        {

            var theTab = ExplorerUIMap.DoubleClickWorkflow("AutoConnectorResource", "UI Test Resources");
            var control = WorkflowDesignerUIMap.FindControlByAutomationId(theTab, "MultiAssignDesigner");

            //Drag a decision to the design surface
            //Note that this point is a position relative to the multi assign on the design surface. This is to ensure that the tool is dropped exactly on the line
            if(control != null)
            {
                var point = new Point(control.BoundingRectangle.X + 120, control.BoundingRectangle.Y - 150);
                ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, point);
            }
            else
            {
                throw new Exception("MultiAssignDesigner not found on active tab");
            }

            DecisionWizardUIMap.CancelWizard();
            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.IsTrue(connectors.Count == 2, "Connector line wasn't split");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("AutoConnectorTests")]
        public void AutoConnectorTests_DragADecisionOnStartAutoConnectorNode_ASecondConnectorIsCreated()
        {

            Mouse.MouseMoveSpeed = 500;
            Mouse.MouseDragSpeed = 500;

            CreateWorkflow();

            Point point = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(TabManagerUIMap.GetActiveTab());
            //Drag a control to the design surface
            ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Decision, point);
            DecisionWizardUIMap.ClickDone(2500);
            //If the screen resolution is low or if the studio is windowed this point can jump as soon as the control is dragged over the work surface, the control might need to be re-dragged to hit the connector line
            Point newPoint = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(TabManagerUIMap.GetActiveTab());
            if(point != newPoint)
            {
                WorkflowDesignerUIMap.DragControl("Decision", newPoint);
            }

            var connectors = WorkflowDesignerUIMap.GetAllConnectors();
            //Assert start auto connector worked
            Assert.AreEqual(1, connectors.Count, "Start auto connector doesnt work");
        }

        #endregion

        #region Utils

        public UITestControl CreateWorkflow()
        {
            return RibbonUIMap.CreateNewWorkflow();
        }

        #endregion
    }
}
