using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests.WindowsDesignSurfaceContextMenu
{
    [CodedUITest]
    public class DesignSurfaceContextMenuTests
    {
        [TestMethod]
        [TestCategory("WindowsDesignSurfaceContextMenu")]
        public void CopyAndPasteWorkflowToItselfDoesNotCopy()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            UIMap.Filter_Explorer("stackoverflowTestWorkflow");
            ToolsUIMap.Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
            ToolsUIMap.RightClick_StackOverFlowService_OnDesignSurface();
            UIMap.Select_Copy_FromContextMenu();
            UIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            ToolsUIMap.RightClick_AssignOnDesignSurface();
            UIMap.Select_Paste_FromContextMenu();
            var controlExistsNow = UIMap.ControlExistsNow(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StackoverflowWorkflow);
            Assert.IsFalse(controlExistsNow);
            UIMap.Click_Close_Workflow_Tab_Button();
            Assert.IsTrue(ToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        public UIMap UIMap
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

        ToolsUIMap ToolsUIMap
        {
            get
            {
                if (_ToolsUIMap == null)
                {
                    _ToolsUIMap = new ToolsUIMap();
                }

                return _ToolsUIMap;
            }
        }

        private ToolsUIMap _ToolsUIMap;

        #endregion
    }
}
