using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SharepointUpdateUITest
    {
        [TestMethod]
        public void Sharepoint_Update_UITest()
        {
            UIMap.Click_New_Workflow_Ribbon_Button();
            UIMap.Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface();
            UIMap.Select_SharepointTestServer_From_SharepointUpdate_Tool();
            UIMap.Click_EditSharepointSource_Button_From_SharePointUpdate();
            UIMap.Click_Sharepoint_Server_Source_TestConnection();
            UIMap.Click_Close_SharepointSource_Tab_Button();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            UIMap.Select_AppData_From_MethodList_From_UpdateTool();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            UIMap.Click_Sharepoint_RefreshButton_From_SharepointUpdate();
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            UIMap.Open_Sharepoint_Update_Tool_Large_View();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.ValueCell.ValueComboBox.Exists);
            UIMap.Click_Close_Workflow_Tab_Button();
            UIMap.Click_MessageBox_No();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
#if !DEBUG
            UIMap.CloseHangingDialogs();
#endif
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
