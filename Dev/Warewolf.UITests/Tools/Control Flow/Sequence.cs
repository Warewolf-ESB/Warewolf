using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools.Control_Flow
{
    [CodedUITest]
    public class Sequence
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void Sequence_DraggingNonDecision_Allowed_LargeView_UITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.Exists, "Sequence on the design surface does not exist");
            UIMap.SequenceTool_ChangeView_With_DoubleClick();
            UIMap.Drag_Toolbox_AssignObject_Onto_Sequence_LargeTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceLargeView.AddModeNewActivity.MultiAssignObject.Exists, "Multi Assign Object Tool does not exist.");
        }

        [TestMethod]
        [TestCategory("Tools")]
        public void Sequence_DraggingNonDecision_Allowed_SmallView_UITest()
        {
            UIMap.Drag_Toolbox_AssignObject_Onto_Sequence_SmallTool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Sequence.SequenceSmallView.ElementTable.AssignObject.Exists, "Assign Object Tool does not exist.");
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Sequence_DraggingSwitch_NotAllowed_BothViews_UITest()
        {
            //Large View
            UIMap.SequenceTool_ChangeView_With_DoubleClick();
            UIMap.Drag_Toolbox_Switch_Onto_Sequence_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_DropNotAllowed_MessageBox_OK();
            //Small View
            UIMap.SequenceTool_ChangeView_With_DoubleClick();
            UIMap.Drag_Toolbox_Switch_Onto_Sequence_SmallTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_DropNotAllowed_MessageBox_OK();
        }

        [TestMethod]
		[TestCategory("Tools")]
        public void Sequence_DraggingDecision_NotAllowed_BothViews_UITest()
        {        
            //Large View 
            UIMap.SequenceTool_ChangeView_With_DoubleClick();
            UIMap.Drag_Toolbox_Decision_Onto_Sequence_LargeTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_DropNotAllowed_MessageBox_OK();
            //Small View
            UIMap.SequenceTool_ChangeView_With_DoubleClick();
            UIMap.Drag_Toolbox_Decision_Onto_Sequence_SmallTool();
            Assert.IsTrue(UIMap.MessageBoxWindow.Exists, "Message box does not exist");
            UIMap.Click_DropNotAllowed_MessageBox_OK();
        }
        

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sequence_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if (_uiMap == null)
                {
                    _uiMap = new UIMap();
                }

                return _uiMap;
            }
        }

        private UIMap _uiMap;

        #endregion
    }
}
