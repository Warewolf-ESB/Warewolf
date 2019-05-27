using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.LoopConstructs.LoopConstructToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.StorageDropbox.DropboxToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Utility.UtilityToolsUIMapClasses;
using Warewolf.Test.Agent;
using System.IO;
using System.Reflection;

namespace Warewolf.UI.Tests.DebugOutputTests
{
    [CodedUITest]
    public class DebugOutputWindowTests
    {
        const string SelectionHighlightWf = "SelectionHighlightWf";
        const string DropboxSelectionHighlightWf = "DropboxSelectionHighlightWf";

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        public void Change_Value_On_HelloWorld_Assign_Marks_Workflow_As_Dirty_UITest()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            DataToolsUIMap.Click_Assign_Row_1_Value_TextBox();
            DataToolsUIMap.Add_Space_At_The_End_Of_Row1_Value();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
        }
        
        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void HelloWorld_Is_Highlighted_After_Execution_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            ExplorerUIMap.Filter_Explorer("Hello World");
            WorkflowTabUIMap.Drag_Explorer_workflow_Onto_Workflow_Design_Surface();
            UIMap.Press_F6();
            WorkflowTabUIMap.WaitForDebugOutput();
            var helloWorldIsHighlighted = WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.HelloWorldWorkFlow.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(helloWorldIsHighlighted, "Sub-workflow is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void WebRequestTool_Is_Highlighted_After_Execution_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Web_Request_Onto_DesignSurface();
            UIMap.Press_F6();
            var webRequest = UtilityToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.WebRequest.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(webRequest, "Web request tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void WorkFlowSelection_Validation_UITest()
        {
            ExplorerUIMap.Filter_Explorer(SelectionHighlightWf);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
            WorkflowTabUIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            WorkflowTabUIMap.Click_DecisionStep_InDebugOutput();
            var assignHasNoFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus, "Assign tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void DropboxWorkFlowSelection_Validation_UITest()
        {
            ExplorerUIMap.Filter_Explorer(DropboxSelectionHighlightWf);
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            UIMap.Press_F6();
            WorkflowTabUIMap.Click_AssignStep_InDebugOutput();
            var assignFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(assignFocus);
            WorkflowTabUIMap.Click_DecisionStep_InDebugOutput();
            var assignHasNoFocus = DataToolsUIMap.MainStudioWindow.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.WorkflowDesigner_Custom.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.MultiAssign.ItemStatus.Contains("IsPrimarySelection=False IsSelection=False");
            Assert.IsTrue(assignHasNoFocus, "Dropbox delete tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void SelectAndApplyWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_SelectAndApply_Onto_DesignSurface();
            UIMap.Press_F6();
            var selectAndApplyFocus = LoopConstructToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.SelectAndApply.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(selectAndApplyFocus, "Select and Apply tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void DropboxDownloadWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_Download_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxDownloadHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDownload.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxDownloadHasFocus, "Dropbox download tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void DropboxDeleteWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_Delete_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxDeleteHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxDelete.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxDeleteHasFocus, "Dropbox delete tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void DropboxUploadWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            DropboxToolsUIMap.Drag_Toolbox_Dropbox_Upload_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxUploadHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxUpload.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxUploadHasFocus, "Dropbox upload tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        [Ignore]//Cannot get ItemStatus property of activities' WpfControls on the design surface if the Studio under test is built targetting .NET Framework 4.7.2
        public void DropboxListWorkFlowSelection_Validation_UITest()
        {
            UIMap.InitializeABlankWorkflow();
            WorkflowTabUIMap.Drag_Toolbox_Dropbox_FileList_Onto_DesignSurface();
            UIMap.Press_F6();
            var dropboxListHasFocus = DropboxToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.DropboxFileList.ItemStatus.Contains("IsPrimarySelection=True IsSelection=True");
            Assert.IsTrue(dropboxListHasFocus, "Dropbox list tool is not selected on the design surface after it was executed with the F6 keyboard shortcut.");
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("MSSql")]
        public void Executing_Sql_Procedure_With_Errors()
        {
            ExplorerUIMap.Filter_Explorer("Raise_Hell_Procedure");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            UIMap.Press_F6();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UISQLServerDatabaseTreeItem.UISQLErrorError15700seText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.UISQLServerDatabaseTreeItem.UIErrorText.Exists);
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Debug Input")]
        public void DebugOutput_MoreLink_UITest()
        {
            ExplorerUIMap.Open_Item_With_Double_Click("More Link UI Testing");
            UIMap.Press_F6();
            WorkflowTabUIMap.WaitForDebugOutput();
            WorkflowTabUIMap.Click_Step_1_More_Link();
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();

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

        UtilityToolsUIMap UtilityToolsUIMap
        {
            get
            {
                if (_UtilityToolsUIMap == null)
                {
                    _UtilityToolsUIMap = new UtilityToolsUIMap();
                }

                return _UtilityToolsUIMap;
            }
        }

        private UtilityToolsUIMap _UtilityToolsUIMap;

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

        LoopConstructToolsUIMap LoopConstructToolsUIMap
        {
            get
            {
                if (_LoopConstructToolsUIMap == null)
                {
                    _LoopConstructToolsUIMap = new LoopConstructToolsUIMap();
                }

                return _LoopConstructToolsUIMap;
            }
        }

        private LoopConstructToolsUIMap _LoopConstructToolsUIMap;

        DropboxToolsUIMap DropboxToolsUIMap
        {
            get
            {
                if (_DropboxToolsUIMap == null)
                {
                    _DropboxToolsUIMap = new DropboxToolsUIMap();
                }

                return _DropboxToolsUIMap;
            }
        }

        private DropboxToolsUIMap _DropboxToolsUIMap;

        #endregion
    }
}