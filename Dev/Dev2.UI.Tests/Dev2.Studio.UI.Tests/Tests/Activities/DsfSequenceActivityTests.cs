
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
            RestartStudioOnFailure();
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
        public void ToolDesigners_SequenceLargeView_DraggingNonDecision_Allowed()
        {
            Mouse.MouseDragSpeed = 500;

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
