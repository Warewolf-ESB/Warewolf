using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebPut
    {
        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebPutToolClickLargeViewDoneButton()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox.Exists, "Web POST large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton.Exists, "Web POST large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Table.Exists, "Web POST large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.DoneButton.Exists, "Web POST large view done does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebPutToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Click_AddNew_Web_Source_From_PutWebtool();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Address Textbox does not exist on new DB source wizard tab after openning it from the Web PUT tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Test Connection Button does not exist on new DB source wizard tab after openning it from the Web PUT tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.Spinner.Exists, "Spinner does not exist on new DB source wizard tab after openning it from the Web PUT tool.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebPutToolEnableGenerateOutGETsButton()
        {
            UIMap.Select_Test_Source_From_PUT_Web_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton.Enabled, "Web PUT tool large view generate outGETs button is not enabled after selecting a source.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebPutToolClickGenerateOutputsButton()
        {
            UIMap.Select_Test_Source_From_PUT_Web_Large_View_Source_Combobox();
            UIMap.Click_PutWeb_GenerateOutputs_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.DoneButton.Exists, "Web PUT tool large view generate outputs done button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.CancelButton.Exists, "Web PUT tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.PasteButton.Exists, "Web PUT tool large view generate outputs paste button does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebPutToolSmallView()
        {
            UIMap.Collapse_PutWeb_RequestTool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.SourcesComboBox.Exists, "Web POST large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.GenerateOutputsButton.Exists, "Web POST large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.LargeView.Table.Exists, "Web POST large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebPut.DoneButton.Exists, "Web POST large view done does not exist.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_PutWeb_Tool_Onto_DesignSurface();
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
