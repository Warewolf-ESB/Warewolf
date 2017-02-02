using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.DebugOutputTests
{
    [CodedUITest]
    public class DebugOutputWindowTests
    {
        const string SelectionHighlightWf = "SelectionHighlightWf";
        [TestMethod]
        [TestCategory("Debug Input")]
        // ReSharper disable once InconsistentNaming
        public void WorkFlowSelection_Validation_UITest()
        {
            UIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            UIMap.Click_DesicionStep_InDebugOutput();
            var assignHasNoFocus = UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Filter_Explorer(SelectionHighlightWf);
            UIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
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