using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Toolbox
{
    [CodedUITest]
    public class ToolboxTests
    {
        [TestMethod]
        [TestCategory("ToolBox")]
        public void ToolboxBehaviourCheck_ClearFilter_Then_PopUp_Then_UpdateHelpText_UITest()
        {
            //Clear Filter
            UIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Connector1.Exists, "No connectors exist on design surface after dragging tool onto start node autoconnector.");
            UIMap.Click_Clear_Toolbox_Filter_Clear_Button();
            Assert.IsTrue(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.ToolBox.SearchTextBox.Text));
            //Update Help Text
            var initialImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            UIMap.SingleClick_Toolbox();
            var assignImage = UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Help.HelpTextEditor.CaptureImage();
            Assert.AreNotEqual(initialImage, assignImage);
            //PopUp
            UIMap.DoubleClick_Toolbox();
            Assert.IsTrue(UIMap.MessageBoxWindow.OKButton.Exists);
            UIMap.Click_MessageBox_OK();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
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
