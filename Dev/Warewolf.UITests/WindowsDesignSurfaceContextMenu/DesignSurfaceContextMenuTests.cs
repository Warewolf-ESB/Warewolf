using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            UIMap.Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
            UIMap.RightClick_StackOverFlowService_OnDesignSurface();
            UIMap.Select_Copy_FromContextMenu();
            UIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.RightClick_AssignOnDesignSurface();
            UIMap.Select_Paste_FromContextMenu();
            var controlExistsNow = UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.StackoverflowWorkflow);
            Assert.IsFalse(controlExistsNow);
            UIMap.Click_Close_Workflow_Tab_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
        }


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
    }
}
