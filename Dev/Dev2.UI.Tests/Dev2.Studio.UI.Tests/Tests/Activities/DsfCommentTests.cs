using System.Drawing;
using Dev2.Studio.UI.Tests.Enums;
using Dev2.Studio.UI.Tests.UIMaps.Activities;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Studio.UI.Tests.Tests.Activities
{
    /// <summary>
    /// Summary description for DsfActivityTests
    /// </summary>
    [CodedUITest]
    public class DsfCommentActivityTests : UIMapBase
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
            TabManagerUIMap.CloseAllTabs();
        }

        #endregion


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDesigners_CommentSmallView")]
        public void ToolDesigners_AssignSmallView_EnteringMultipleRows_IndexingWorksFine()
        {
            using(var dsfActivityUiMap = new DsfCommentUiMap(false, false) { TheTab = RibbonUIMap.CreateNewWorkflow(2000) })
            {
                Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(dsfActivityUiMap.TheTab);
                dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Comment, dsfActivityUiMap.TheTab, pointToDragTo);
                RibbonUIMap.DebugShortcutKeyPress();
                var stepType = OutputUIMap.GetStep(2);

                Assert.AreEqual("Comment", stepType.FriendlyName);
            }

        }

    }
}
