using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfSequenceActivityTests : UIMapBase
    {
        #region Fields


        #endregion

        #region Setup
        [TestInitialize]
        public void TestInit()
        {
            Init();
        }

        #endregion

        #region Cleanup
        [TestCleanup]
        public void MyTestCleanup()
        {
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceSmallView_DraggingDecision_NotAllowed()
        {
            Mouse.MouseDragSpeed = 500;

            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Decision);

                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(0, activityList.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceSmallView_DraggingSwitch_NotAllowed()
        {
            Mouse.MouseDragSpeed = 500;

            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Switch);

                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(0, activityList.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceSmallView_DraggingNonDecision_Allowed()
        {
            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Assign);

                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(1, activityList.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceLargeView_DraggingNonDecision_Allowed()
        {
            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.ClickOpenLargeView();
                dsfActivityUiMap.DragActivityOnLargeViewDropPoint(ToolType.Assign);
                dsfActivityUiMap.ClickCloseLargeView();
                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(1, activityList.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceLargeView_DraggingDecision_NotAllowed()
        {
            Mouse.MouseDragSpeed = 500;

            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.ClickOpenLargeView();
                dsfActivityUiMap.DragActivityOnLargeViewDropPoint(ToolType.Decision);
                dsfActivityUiMap.ClickCloseLargeView();
                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(0, activityList.Count);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_SequenceSmallView")]
        public void ToolDesigners_SequenceLargeView_DraggingSwitch_NotAllowed()
        {
            Mouse.MouseDragSpeed = 500;
            using(var dsfActivityUiMap = new DsfSequenceUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Sequence, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.ClickOpenLargeView();
                dsfActivityUiMap.DragActivityOnLargeViewDropPoint(ToolType.Switch);
                dsfActivityUiMap.ClickCloseLargeView();
                var activityList = dsfActivityUiMap.GetActivityList();
                Assert.AreEqual(0, activityList.Count);
            }
        }
    }
}