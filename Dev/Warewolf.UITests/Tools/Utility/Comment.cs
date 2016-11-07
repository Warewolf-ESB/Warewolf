using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Comment
    {
        [TestMethod]
        [TestCategory("Utility Tools")]
        public void CommentTool_OpenLargeViewUITest()
        {
            UIMap.Drag_Toolbox_Comment_Onto_DesignSurface();
        }

        [TestMethod]
        [TestCategory("Utility Tools")]
        public void ToolDesigners_CommentSmallView_Debug_DebugOutputWorksFine_UITest()
        {
            UIMap.Drag_Toolbox_Comment_Onto_DesignSurface();
            UIMap.Press_F6();
            Assert.AreEqual("Comment", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CommentTreeItem.CommentButton.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
            UIMap.Click_New_Workflow_Ribbon_Button();            
        }


        UIMap UIMap
        {
            get
            {
                if (_UIMap == null)
                {
                    _UIMap = new UIMap();
                }

                return _UIMap;
            }
        }

        private UIMap _UIMap;

        #endregion
    }
}
