using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.Tools
{
    [CodedUITest]
    public class HttpWebDelete
    {
        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickLargeViewUITest()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.SourcesComboBox.Exists, "Web Delete large view sources combobox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Exists, "Web Delete large view generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.Table.Exists, "Web Delete large view headers table generate inputs button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.DoneButton.Exists, "Web Delete large view done does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickAddNewSourceButtonOpensNewSourceWizardTab()
        {
            UIMap.Click_HTTP_Delete_Web_Tool_New_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.AddressTextbox.Exists, "Address Textbox does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.TestConnectionButton.Exists, "Test Connection Button does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceWizardTab.WorkSurfaceContext.Spinner.Exists, "Spinner does not exist on new DB source wizard tab after openning it from the Web DELETE tool.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolEnableGenerateOutputsButton()
        {
            UIMap.Select_Test_Source_From_DELETE_Web_Large_View_Source_Combobox();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.GenerateOutputsButton.Enabled, "Web DELETE tool large view generate outputs button is not enabled after selecting a source.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolClickTestInputsDoneButton()
        {
            UIMap.Select_Test_Source_From_DELETE_Web_Large_View_Source_Combobox();
            UIMap.Click_DELETE_Web_Large_View_Generate_Outputs();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.DoneButton.Exists, "Web DELETE tool large view generate outputs done button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.CancelButton.Exists, "Web DELETE tool large view generate outputs cancel button does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.LargeView.PasteButton.Exists, "Web DELETE tool large view generate outputs paste button does not exist.");
        }

        [TestMethod]
        [TestCategory("HTTP Tools")]
        public void HttpWebDeleteToolSmallView()
        {
            UIMap.Collapse_DeleteWeb_Tool_Large_View_to_Small_View_With_Double_Click();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebDelete.SmallView.Exists, "Web DELETE small view does not exist after collapsing the large view with a double click.");
        }
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.InitializeABlankWorkflow();
            UIMap.Drag_DeleteWeb_Toolbox_Onto_Workflow_Surface();
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
