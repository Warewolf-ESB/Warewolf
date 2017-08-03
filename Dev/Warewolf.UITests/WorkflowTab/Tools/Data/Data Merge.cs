using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UITests.WorkflowTab.Tools.Data
{
    [CodedUITest]
    public class Data_Merge
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Data Tools")]
        public void DataMergeTool_Small_And_Large_Then_QVIView_UITest()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.Exists, "Data Merge on the design surface does not exist");
            //Small View
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Exists, "Data Grid does not exist");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.ResultTextbox.Exists, "Results Textbox does not exist");
            //Large View
            DataToolsUIMap.Open_DataMerge_LargeView();
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.DataGrid.Exists, "Data Grid does not exist.");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.ResultTextbox.Exists, "Result Textbox does not exist");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.LargeView.OnErrorGroup.Exists, "On Error Pane does not exist");
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.DoneButton.Exists, "Done Button does not exist");
            //QVI View
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QVIToggleButton.Exists, "QVI Toggle Button on DataMerge Tool does not exist");
            DataToolsUIMap.Open_DataMergeTool_QVIView();
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.QuickVariableInputContent.Exists, "QVI on DataMerge Tool is not open");
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Data Tools")]
        public void DataMerge_ScrollingUITest()
        {
            DataToolsUIMap.Open_DataMerge_LargeView();
            DataToolsUIMap.Enter_Values_Into_DataMergeTool();
            DataToolsUIMap.Open_DataMergeTool_SmallView();
            DataToolsUIMap.Scroll_DownThenUp_On_DataMergeTool_SmallView();
            Assert.AreEqual("1", DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.UsingCell.Row1UsingDComboBox.TextEdit.Text);
            Assert.AreEqual("2", DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.UsingCell.Row2UsingComboBox.TextEdit.Text);
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Data Tools")]
        public void DataMerge_KeyboardDelete_ShouldNot_DeleteRow()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Delete}", ModifierKeys.None);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Delete}", ModifierKeys.None);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Data Tools")]
        public void DataMerge_KeyboardAnyKeys_ShouldNot_ChangeAnything()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Z}", ModifierKeys.Control);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Z}", ModifierKeys.Control);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Data Tools")]
        public void DataMerge_KeyboardSelection_Should_AllowSelection()
        {
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Tab}", ModifierKeys.None);
            DataToolsUIMap.Click_DataMerge_SmallView_UsingDropdown();
            Keyboard.SendKeys(DataToolsUIMap.MainStudioWindow, "{Tab}", ModifierKeys.Shift);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row.Exists);
            Assert.IsTrue(DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DataMerge.SmallView.DataGrid.Row2.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            screenRecorder.StartRecording(TestContext);
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Drag_Toolbox_Data_Merge_Onto_DesignSurface();
        }

        [TestCleanup]
        public void StopScreenRecording()
        {
            screenRecorder.StopRecording(TestContext);
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
