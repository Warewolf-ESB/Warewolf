using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UITests.WindowsDesignSurfaceContextMenu
{
    [CodedUITest]
    public class DesignSurfaceContextMenuTests
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        public void CopyAndPasteWorkflowToItselfDoesNotCopy()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            ExplorerUIMap.Filter_Explorer("stackoverflowTestWorkflow");
            WorkflowTabUIMap.Drag_Explorer_Localhost_First_Item_Onto_Workflow_Design_Surface();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkflowSurfaceContext.ContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
            WorkflowTabUIMap.RightClick_StackOverFlowService_OnDesignSurface();
            UIMap.Select_Copy_FromContextMenu();
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            DataToolsUIMap.RightClick_AssignOnDesignSurface();
            UIMap.Select_Paste_FromContextMenu();
            var controlExistsNow = UIMap.ControlExistsNow(WorkflowTabUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkflowSurfaceContext.ContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF);
            Assert.IsFalse(controlExistsNow);
            UIMap.Click_Close_Workflow_Tab_Button();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkflowSurfaceContext.ContentDockManagerCustom.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.stackOverflowTestWF.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            screenRecorder.StartRecording(TestContext);
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        [TestCleanup]
        public void StopScreenRecording()
        {
            screenRecorder.StopRecording(TestContext);
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

        ExplorerUIMap ExplorerUIMap
        {
            get
            {
                if (_ExplorerUIMap == null)
                {
                    _ExplorerUIMap = new ExplorerUIMap();
                }

                return _ExplorerUIMap;
            }
        }

        private ExplorerUIMap _ExplorerUIMap;

        DataToolsUIMap DataToolsUIMap
        {
            get
            {
                if (_DataToolsUIMap == null)
                {
                    _DataToolsUIMap = new DataToolsUIMap();
                }

                return _DataToolsUIMap;
            }
        }

        private DataToolsUIMap _DataToolsUIMap;

        #endregion
    }
}
