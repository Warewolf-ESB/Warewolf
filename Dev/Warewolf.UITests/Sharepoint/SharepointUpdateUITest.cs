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
            Uimap.Click_New_Workflow_Ribbon_Button();
            Uimap.Drag_Toolbox_Sharepoint_Update_Onto_DesignSurface();
            Uimap.Select_SharepointTestServer_From_SharepointUpdate_Tool();
            Uimap.Click_EditSharepointSource_Button_From_SharePointUpdate();
            Uimap.Click_Sharepoint_Server_Source_TestConnection();
            Uimap.Click_Close_SharepointSource_Tab_Button();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            Uimap.Select_AppData_From_MethodList_From_UpdateTool();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            Uimap.Click_Sharepoint_RefreshButton_From_SharepointUpdate();
            Uimap.WaitForSpinner(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.Spinner);
            Uimap.Open_Sharepoint_Update_Tool_Large_View();
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableVariables.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.FileNameCell.FieldComboBox.Exists);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.MatchTypeCell.MatchTypeComboBox.Exists);
            Assert.IsTrue(Uimap.MainStudioWindow.DockManager.SplitPaneMiddle.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdate.LargeViewContent.DataGridTableValues.ItemRow1.ValueCell.ValueComboBox.Exists);
            Uimap.Click_Close_Workflow_Tab_Button();
            Uimap.Click_MessageBox_No();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            Uimap.SetPlaybackSettings();
#if !DEBUG
            Uimap.CloseHangingDialogs();
#endif
        }      

        UIMap Uimap
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
