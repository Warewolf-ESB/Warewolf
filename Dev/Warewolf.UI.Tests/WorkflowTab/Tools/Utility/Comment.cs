﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;

namespace Warewolf.UI.Tests.WorkflowTab.Tools.Utility
{
    [CodedUITest]
    public class Comment
    {
        private const string TestingCommentToolForHeightResize = "TestingCommentToolForHeightResize";
        private const string TestingCommentToolForWidthResize = "TestingCommentToolForWidthResize";

        [TestMethod]
        [TestCategory("Utility Tools")]
        public void CommentTool_EnterText_Save_And_Resize_Height_Then_Debug_UITest()
        {
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment Tool does not existon the design surface.");
            //Enter Text
            UtilityToolsUIMap.Enter_Text_Into_CommentTool("Some comment, some comment, some comment,");
            //Save
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            UIMap.Save_With_Ribbon_Button_And_Dialog(TestingCommentToolForHeightResize);
            //Resize
            Assert.IsFalse(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            var newHeight = UtilityToolsUIMap.Expand_Comment_Tool_Height_Size();
            Assert.AreEqual(newHeight, UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Height);
            //Debug
            UIMap.Press_F6();
            Assert.AreEqual("Comment", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CommentTreeItem.CommentButton.DisplayText);
        }

        [TestMethod]
        [TestCategory("Utility Tools")]
        public void CommentTool_EnterText_Save_And_Resize_Width_Then_Debug_UITest()
        {
            Assert.IsTrue(UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.Exists, "Comment Tool does not existon the design surface.");
            //Enter Text
            UtilityToolsUIMap.Enter_Text_Into_CommentTool("Some comment, some comment, some comment,");
            //Save
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            UIMap.Save_With_Ribbon_Button_And_Dialog(TestingCommentToolForWidthResize);
            //Resize
            Assert.IsFalse(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText.Contains("*"));
            var newWidth = UtilityToolsUIMap.Expand_Comment_Tool_Width_Size();
            Assert.AreEqual(newWidth, UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Comment.LargeViewContentCustom.Width);
            //Debug
            UIMap.Press_F6();
            Assert.AreEqual("Comment", WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.CommentTreeItem.CommentButton.DisplayText);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Comment_Onto_DesignSurface();
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

        WorkflowTabUIMap WorkflowTabUIMap
        {
            get
            {
                if (_WorkflowTabUIMap == null)
                {
                    _WorkflowTabUIMap = new WorkflowTabUIMap();
                }

                return _WorkflowTabUIMap;
            }
        }

        private WorkflowTabUIMap _WorkflowTabUIMap;

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

        #endregion
    }
}
