using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Utility
{
    [CodedUITest]
    public class Comment
    {
        private const string CommentToolWf = "TestingCommentToolResize";

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

        [TestMethod]
        [TestCategory("Utility Tools")]
        public void ToolDesigners_ResizeComment_MakesTheWorkflowDirty_UITest()
        {            
            UIMap.Enter_Text_Into_CommentTool("Some comment, some comment, some comment,");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            UIMap.Save_With_Ribbon_Button_And_Dialog(CommentToolWf);
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            var newHeight = UIMap.Expand_Comment_Tool_Size();            
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Filter_Explorer(CommentToolWf);
            UIMap.Open_Explorer_First_Item_With_Context_Menu();
            Assert.AreEqual(newHeight, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Comment_Onto_DesignSurface();
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
