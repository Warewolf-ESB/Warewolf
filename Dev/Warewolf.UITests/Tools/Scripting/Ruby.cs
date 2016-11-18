using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class Ruby
    {

        [TestMethod]
        [TestCategory("Tools")]
        public void RubyScriptToolSmallViewUITest()
        {
            Assert.AreEqual(true, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.SmallView.ScriptIntellisenseCombobox.Exists, "Ruby script textbox does not exist after dragging on tool from the toolbox.");
            Assert.AreEqual(true, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Ruby.SmallView.ResultIntellisenseCombobox.Exists, "Ruby result textbox does not exist after dragging on tool from the toolbox.");}

    [TestMethod]
		[TestCategory("Tools")]
        public void RubyScriptToolLargeViewUITest()
        {            
            UIMap.Open_Ruby_Large_View();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Ruby_Onto_DesignSurface();
        }

        UIMap UIMap
        {
            get
            {
                if ((_UIMap == null))
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
