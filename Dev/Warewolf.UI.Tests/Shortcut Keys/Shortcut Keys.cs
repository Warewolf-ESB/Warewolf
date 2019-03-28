using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Deploy.DeployUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.Tools.Data.DataToolsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Search.SearchUIMapClasses;
using System.Windows.Forms;

namespace Warewolf.UI.Tests.Workflow
{
    [CodedUITest]
    public class Shortcut_Keys
    {
        private const string Folder = "Acceptance Tests";
        private const string HelloWorld = "Hello World";
        
        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Open_And_Save_Workflow_With_ShortcutKeys()
        {
            //ShortCut W Opens New Workflow
            ExplorerUIMap.Click_LocalHost_Once();
            ExplorerUIMap.Create_New_Workflow_Using_Shortcut();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            //Short S Opens SaveWorkflow Dialog
            WorkflowTabUIMap.Drag_Toolbox_MultiAssign_Onto_DesignSurface();
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.Exists);
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_S_Saves_Dirty_Workflows()
        {
            ExplorerUIMap.Filter_Explorer("Shortcut_Control_S_Saves_Dirty_Workflows");
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            WorkflowTabUIMap.Make_Workflow_Savable();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
            Assert.IsFalse(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
        }
        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_Shift_S_Saves_All_Workflows_Without_Closing()
        {
            ExplorerUIMap.Filter_Explorer("ResourceForSaveAllTabs1");
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            ExplorerUIMap.Filter_Explorer("ResourceForSaveAllTabs2");
            ExplorerUIMap.Open_ExplorerFirstItem_From_ExplorerContextMenu();
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            WorkflowTabUIMap.Save_Workflow_Using_Shift_Control_Shortcut();
            Assert.IsFalse(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.UIUI_TabManager_AutoIDTabList.ResourceForSaveAllTabs1.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.UIUI_TabManager_AutoIDTabList.ResourceForSaveAllTabs2.Exists);
        }
        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_D_Opens_DeployTabWith_Resource_Selected()
        {
            ExplorerUIMap.Filter_Explorer(HelloWorld);
            ExplorerUIMap.Click_Explorer_Localhost_First_Item();
            UIMap.Open_Deploy_Using_Shortcut();
            UIMap.WaitForControlEnabled(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.CloseButton);
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "Deploy Tab does not exist.");
            UIMap.WaitForSpinner(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Spinner);
            DeployUIMap.Filter_Deploy_Source_Explorer(HelloWorld);
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.WorkSurfaceContext.DockManager.DeployView.SourceServerExplorer.ExplorerTree.LocalHost.Item1.CheckBox.Checked);

        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_W_Opens_NewWorkflow_In_The_Selected_Folder()
        {
            ExplorerUIMap.Filter_Explorer(Folder);
            ExplorerUIMap.Click_Explorer_Localhost_First_Item();
            ExplorerUIMap.Create_New_Workflow_In_Explorer_First_Item_With_Shortcut();
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.Exists);
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.HttpLocalHostText.NewWorkflowHyperLink.Alt.Contains(Folder));
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Control_D_Opens_DeployWizardTab()
        {
            ExplorerUIMap.Click_LocalHost_Once();
            UIMap.Open_Deploy_Using_Shortcut();
            UIMap.WaitForControlVisible(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab);
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists);
        }

        [TestMethod]
        [DeploymentItem(@"lib\win32\x86\git2-6311e88.dll", @"lib\win32\x86")]
        [DeploymentItem(@"lib\win32\x64\git2-6311e88.dll", @"lib\win32\x64")]
        [TestCategory("Shortcut Keys")]
        public void Shortcut_Cntr_Shift_F_Opens_Search_View()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            SendKeys.SendWait("^+F");
            var isSearchTabOpen = SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists;
            if (!isSearchTabOpen)
            {
                Keyboard.SendKeys(SearchUIMap.MainStudioWindow, "^+F");
            }
        }

        #region Additional test attributes

        [TestInitialize()]
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

        DeployUIMap DeployUIMap
        {
            get
            {
                if (_DeployUIMap == null)
                {
                    _DeployUIMap = new DeployUIMap();
                }

                return _DeployUIMap;
            }
        }

        private DeployUIMap _DeployUIMap;

        SearchUIMap SearchUIMap
        {
            get
            {
                if (_SearchUIMap == null)
                {
                    _SearchUIMap = new SearchUIMap();
                }

                return _SearchUIMap;
            }
        }

        private SearchUIMap _SearchUIMap;

        #endregion
    }
}
