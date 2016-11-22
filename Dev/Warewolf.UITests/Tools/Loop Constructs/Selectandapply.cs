using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Selectandapply
    {
        [TestMethod]
		[TestCategory("Tools")]
        public void SelectandapplyToolUITest()
        {
            UIMap.Drag_Toolbox_Selectandapply_Onto_DesignSurface();
            Assert.AreEqual(true, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.SelectFromIntellisenseTextbox.Exists, "Select and apply select from textbox does not exist after dropping tool from toolbox.");
            Assert.AreEqual(true, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.AliasIntellisenseTextbox.Exists, "Select and apply alias textbox does not exist after dropping tool from toolbox.");
            Assert.AreEqual(true, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.SmallView.DropActivityHere.Exists, "Select and apply activity drop box does not exist after dropping tool from toolbox.");
            UIMap.Open_Selectandapply_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
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
