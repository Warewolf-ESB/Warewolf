using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class ForEach
    {

        [TestMethod]
        [TestCategory("Tools")]
        public void ForEachTool_SmallViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ForEachTypeComboBox.Exists, "Type dropdown does not exist on for each on the design surface.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.FromIntellisenseTextbox.Textbox.Exists, "Start textbox in in range foreach on the design surface does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.DropActivityHere.Exists, "Activity drop box does not exist on for each.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.ForEach.SmallView.ToIntellisenseTextbox.Textbox.Exists, "End textbox in in range foreach on the design surface does not exist.");
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void ForEachTool_OpenLargeViewUITest()
        {
            UIMap.Open_ForEach_Large_View();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragADecisionIntoForEachExpectNotAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_Decision_Onto_Foreach_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", UIMap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragASwitchIntoForEachExpectNotAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_ASwitch_Onto_Foreach_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists);
            Assert.AreEqual("Drop not allowed", UIMap.MessageBoxWindow.DropnotallowedText.DisplayText
                , "Error message is not about being unable to drop switch onto the Sequence tool");
            UIMap.Click_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void DragAnAssignIntoForEachExpectAddedToForEach_UITest()
        {
            UIMap.Drag_Toolbox_AssignObject_Onto_Foreach_LargeTool();
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
            UIMap.Drag_Toolbox_For_Each_Onto_DesignSurface();
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
