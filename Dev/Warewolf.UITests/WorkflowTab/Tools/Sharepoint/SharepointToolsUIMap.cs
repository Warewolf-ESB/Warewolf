using TechTalk.SpecFlow;
using Warewolf.UITests.DialogsUIMapClasses;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Drawing;
using System.Windows.Input;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.DotNetPluginSource.DotNetPluginSourceUIMapClasses;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UITests.WorkflowTab.Tools.Sharepoint.SharepointToolsUIMapClasses
{
    [Binding]
    public partial class SharepointToolsUIMap
    {
        [When(@"I Open Sharepoint Copy Tool Large View")]
        [Then(@"I Open Sharepoint Copy Tool Large View")]
        [Given(@"I Open Sharepoint Copy Tool Large View")]
        public void Open_SharepointCopyTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile, new Point(230, 11));
        }

        [When(@"I Open Sharepoint Create Tool Large View")]
        public void Open_SharepointCreateTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem, new Point(195, 11));
        }

        [When(@"I Open Sharepoint Delete List Item Tool Large View")]
        public void Open_SharepointDeleteListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem, new Point(218, 11));
        }

        [When(@"I Open Sharepoint Delete File Tool Large View")]
        public void Open_SharepointDeleteFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, new Point(218, 11));
        }

        [Given(@"I Open Sharepoint Download File Tool Large View")]
        [When(@"I Open Sharepoint Download File Tool Large View")]
        [Then(@"I Open Sharepoint Download File Tool Large View")]
        public void Open_SharepointDownloadFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile, new Point(185, 9));
        }

        [Given(@"I Open Sharepoint MoveFile Tool Large View")]
        [When(@"I Open Sharepoint MoveFile Tool Large View")]
        [Then(@"I Open Sharepoint MoveFile Tool Large View")]
        public void Open_SharepointMoveFileTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Read Folder Tool Large View")]
        [When(@"I Open Sharepoint Read Folder Tool Large View")]
        [Then(@"I Open Sharepoint Read Folder Tool Large View")]
        public void Open_SharepointReadFolderTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Read List Item Tool Large View")]
        [When(@"I Open Sharepoint Read List Item Tool Large View")]
        [Then(@"I Open Sharepoint Read List Item Tool Large View")]
        public void Open_SharepointReadListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, new Point(195, 7));
        }

        [Given(@"I Open Sharepoint Update List Item Tool Large View")]
        [When(@"I Open Sharepoint Update List Item Tool Large View")]
        [Then(@"I Open Sharepoint Update List Item Tool Large View")]
        public void Open_SharepointUpdateListItemTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem, new Point(230, 11));
        }

        [Given(@"I Open Sharepoint Upload Tool Large View")]
        [When(@"I Open Sharepoint Upload Tool Large View")]
        [Then(@"I Open Sharepoint Upload Tool Large View")]
        public void Open_SharepointUploadTool_LargeView()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile, new Point(230, 11));
        }

        [When(@"I Enter Sharepoint Server Path From OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.FromDirectoryComboBox.TextEdit.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path From OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_From_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.LocalPathFromIntellisenseCombobox.Textbox.Text = "clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnCopyFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnCopyFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnMoveFile Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnMoveFile_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.SmallView.PathDirectoryComboBox.TextEdit.Text = "TestFolder/clocks.dat";
        }

        [When(@"I Enter Sharepoint Server Path To OnUpload Tool")]
        public void Enter_Sharepoint_Server_Path_To_OnUpload_Tool()
        {
            MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.SmallView.ServerPathToIntellisenseCombobox.Textbox.Text = "TestFolder/clocks.dat";
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointRead()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        public void Click_Sharepoint_RefreshButton_From_SharepointUpdate()
        {
            var refreshButton = MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.RefreshButton;
            Mouse.Click(refreshButton);
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_UpdateTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }


        [Given(@"I Select Acceptance Test in delete")]
        [When(@"I Select Acceptance Test in delete")]
        [Then(@"I Select Acceptance Test in delete")]
        public void Select_AcceptanceTestin_From_DeleteListItemsTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [Given(@"I Select AppData From MethodList")]
        [When(@"I Select AppData From MethodList")]
        [Then(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList_From_DeleteTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [Given(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [When(@"I Click EditSharepointSource Button From SharePointUpdate")]
        [Then(@"I Click EditSharepointSource Button From SharePointUpdate")]
        public void Click_EditSharepointSource_Button_From_SharePointUpdate()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [When(@"I Click EditSharepointSource Button FromSharePointDelete")]
        [Then(@"I Click EditSharepointSource Button FromSharePointDelete")]
        public void Click_EditSharepointSource_Button_FromSharePointDelete()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button FromSharePointRead")]
        [When(@"I Click EditSharepointSource Button FromSharePointRead")]
        [Then(@"I Click EditSharepointSource Button FromSharePointRead")]
        public void Click_EditSharepointSource_Button_FromSharePointRead()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click EditSharepointSource Button")]
        [When(@"I Click EditSharepointSource Button")]
        [Then(@"I Click EditSharepointSource Button")]
        public void Click_EditSharepointSource_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.EditSourceButton, new Point(98, 12));
        }

        [Given(@"I Click New Source Button From SharepointCopyFile Tool")]
        [When(@"I Click New Source Button From SharepointCopyFile Tool")]
        [Then(@"I Click New Source Button From SharepointCopyFile Tool")]
        public void Click_NewSource_From_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointCreateListItem Tool")]
        [When(@"I Click New Source Button From SharepointCreateListItem Tool")]
        [Then(@"I Click New Source Button From SharepointCreateListItem Tool")]
        public void Click_NewSource_From_SharepointCreateListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDeleteFile Tool")]
        [When(@"I Click New Source Button From SharepointDeleteFile Tool")]
        [Then(@"I Click New Source Button From SharepointDeleteFile Tool")]
        public void Click_NewSource_From_SharepointDeleteFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        [When(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        [Then(@"I Click New Source Button From SharepointDeleteListItem Tool")]
        public void Click_NewSource_From_SharepointDeleteListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointDownloadFile Tool")]
        [When(@"I Click New Source Button From SharepointDownloadFile Tool")]
        [Then(@"I Click New Source Button From SharepointDownloadFile Tool")]
        public void Click_NewSource_From_SharepointDownloadFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDownloadFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointUploadFile Tool")]
        [When(@"I Click New Source Button From SharepointUploadFile Tool")]
        [Then(@"I Click New Source Button From SharepointUploadFile Tool")]
        public void Click_NewSource_From_SharepointUploadFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUploadFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointMoveFile Tool")]
        [When(@"I Click New Source Button From SharepointMoveFile Tool")]
        [Then(@"I Click New Source Button From SharepointMoveFile Tool")]
        public void Click_NewSource_From_SharepointMoveFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointMoveFile.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointReadFolder Tool")]
        [When(@"I Click New Source Button From SharepointReadFolder Tool")]
        [Then(@"I Click New Source Button From SharepointReadFolder Tool")]
        public void Click_NewSource_From_SharepointReadFolderTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadFolder.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointReadListItem Tool")]
        [When(@"I Click New Source Button From SharepointReadListItem Tool")]
        [Then(@"I Click New Source Button From SharepointReadListItem Tool")]
        public void Click_NewSource_From_SharepointReadListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.LargeView.Server.NewSharePointSource);
        }

        [Given(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        [When(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        [Then(@"I Click New Source Button From SharepointUpdateListItem Tool")]
        public void Click_NewSource_From_SharepointUpdateListItemTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.LargeView.Server.NewSharePointSource);
        }

        [When(@"I RightClick SharepointCreateListItem OnDesignSurface")]
        public void RightClick_SharepointCreateListItem_OnDesignSurface()
        {
            Mouse.Click(UIMap.MainStudioWindow.DesignSurfaceContextMenu.Copy, MouseButtons.Right, ModifierKeys.None, new Point(199, 12));
        }

        [When(@"I RightClick SharepointDelete OnDesignSurface")]
        public void RightClick_SharepointDelete_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointDeleteFile, MouseButtons.Right, ModifierKeys.None, new Point(217, 8));
        }

        [When(@"I RightClick SharepointRead OnDesignSurface")]
        public void RightClick_SharepointRead_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem, MouseButtons.Right, ModifierKeys.None, new Point(203, 9));
        }

        [When(@"I RightClick SharepointUpdate OnDesignSurface")]
        public void RightClick_SharepointUpdate_OnDesignSurface()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem, MouseButtons.Right, ModifierKeys.None, new Point(210, 5));
        }

        [When(@"I Select AcceptanceTestin create")]
        public void Select_AcceptanceTestin_create()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(119, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAcceptanceTesting_CrListItem, new Point(114, 13));
        }

        [When(@"I Select AppData From MethodList")]
        public void Select_AppData_From_MethodList()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Select AppData From MethodList From ReadTool")]
        public void Select_AppData_From_MethodList_From_ReadTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList, new Point(174, 7));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.MethodList.UIAppdataListItem, new Point(43, 15));
        }

        [When(@"I Select NewSharepointSource FromServer Lookup")]
        public void Select_NewSharepointSource_FromServer_Lookup()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(107, 13));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, "{Down}{Enter}", ModifierKeys.None);
        }

        [When(@"I Select SharepointTestServer")]
        public void Select_SharepointTestServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCreateListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
        }

        [When(@"I Select SharepointTestServer From SharepointRead Tool")]
        public void Select_SharepointTestServer_From_SharepointRead_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointReadListItem.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        [When(@"I Select SharepointTestServer From SharepointUpdateListItem Tool")]
        public void Select_SharepointTestServer_From_SharepointUpdateListItem_Tool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.Server, new Point(98, 12));
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.Server.SharepointTestServer, new Point(67, 13));
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointUpdateListItem.SmallView.EditSourceButton.Enabled, "edit sharepoint source is disabled after selecting a source");
        }

        public void Select_Source_From_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.Server.SharepointSourceFromToolListItem);
        }

        public void Click_EditSourceButton_On_SharepointCopyFileTool()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SharepointCopyFile.LargeView.EditSourceButton);
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

        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;

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

        DotNetPluginSourceUIMap DotNetPluginSourceUIMap
        {
            get
            {
                if (_DotNetPluginSourceUIMap == null)
                {
                    _DotNetPluginSourceUIMap = new DotNetPluginSourceUIMap();
                }

                return _DotNetPluginSourceUIMap;
            }
        }

        private DotNetPluginSourceUIMap _DotNetPluginSourceUIMap;

        ComPluginSourceUIMap ComPluginSourceUIMap
        {
            get
            {
                if (_ComPluginSourceUIMap == null)
                {
                    _ComPluginSourceUIMap = new ComPluginSourceUIMap();
                }

                return _ComPluginSourceUIMap;
            }
        }

        private ComPluginSourceUIMap _ComPluginSourceUIMap;

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
    }
}
