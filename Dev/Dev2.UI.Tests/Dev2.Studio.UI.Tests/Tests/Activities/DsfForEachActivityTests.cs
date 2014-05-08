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
    public class DsfForEachActivityTests : UIMapBase
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
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingDecision_NotAllowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Decision);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNull(forEachActivity);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingSwitch_NotAllowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Switch);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNull(forEachActivity);
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ToolDesigners_ForEach")]
        public void ToolDesigners_ForEach_DraggingNonDecision_Allowed()
        {
            using(var dsfActivityUiMap = new DsfForEachUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.ForEach, dsfActivityUiMap.TheTab, pointToDragTo);
                dsfActivityUiMap.DragActivityOnDropPoint(ToolType.Assign);

                var forEachActivity = dsfActivityUiMap.GetActivity();
                Assert.IsNotNull(forEachActivity);
            }
        }

    }
}