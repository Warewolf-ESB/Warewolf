using System.Drawing;
using System.Globalization;
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
        #region Fields

        private static DsfCommentUiMap _dsfActivityUiMap;

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
            _dsfActivityUiMap.Dispose();
        }

        #endregion



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ToolDesigners_CommentSmallView")]
        public void ToolDesigners_AssignSmallView_EnteringMultipleRows_IndexingWorksFine()
        {
            _dsfActivityUiMap = new DsfCommentUiMap(false,false);
            _dsfActivityUiMap.TheTab = RibbonUIMap.CreateNewWorkflow(2000);
            Point pointToDragTo = WorkflowDesignerUIMap.GetStartNodeBottomAutoConnectorPoint(_dsfActivityUiMap.TheTab);
            _dsfActivityUiMap.Activity = ToolboxUIMap.DragControlToWorkflowDesigner(ToolType.Comment, _dsfActivityUiMap.TheTab, pointToDragTo);
            _dsfActivityUiMap.EnterTextIntoComment("sometext");
            RibbonUIMap.DebugShortcutKeyPress();
            var stepType = OutputUIMap.GetStep(2);

            Assert.AreEqual("Comment",stepType.FriendlyName);


        }

    }
}
