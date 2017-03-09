using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;

// ReSharper disable InconsistentNaming

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RenameExplorerResource
    {
        private const string Folder = "FolderRenameInExplorer";
        private const string newFolderName = "FolderItem2";
        private const string newResourceName = "FolderItem2";
        private const string ResourceToRename = "KeepNewName";
        const string newName = ResourceToRename + "Renamed";

        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_ThenFolderItem()
        {
            ExplorerUIMap.Filter_Explorer("FolderItem");
            ExplorerUIMap.Rename_LocalFolder_To_SecondFolder(newFolderName);
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            ExplorerUIMap.Rename_FolderItem_ToNewFolderItem(newResourceName);
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            ExplorerUIMap.Click_Explorer_Refresh_Button();
            var itemEdit = ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ResourceNameTextBlock;
            Assert.AreEqual(newResourceName, itemEdit.DisplayText);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_UsingF2_Shortcut()
        {            
            var renamedFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\";
            ExplorerUIMap.Filter_Explorer(Folder);
            ExplorerUIMap.Rename_Folder_Using_Shortcut(Folder + "_Renamed");
            Assert.IsTrue(Directory.Exists(renamedFolder + Folder + "_Renamed"), "Folder did not rename");
            
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void RenameFolder_Updates_The_Workflow_Url()
        {
            //const string ExistingFloder = "Acceptance Tests";
            const string AcceptanceTestsRenamed = "Acceptance Tests_Renamed";
            const string WorkflowName = "LoopTest";
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            var renamedFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests_Renamed";

            ExplorerUIMap.Filter_Explorer(WorkflowName);
            ExplorerUIMap.Open_ExplorerFirstSubItem_From_ExplorerContextMenu();
            ExplorerUIMap.Rename_Folder_Using_Shortcut(AcceptanceTestsRenamed);
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            UIMap.Click_Close_Workflow_Tab_Button();
            ExplorerUIMap.Open_ExplorerFirstSubItem_From_ExplorerContextMenu();
            Directory.Move(renamedFolder, resourcesFolder);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Rename_Resource_Close_And_ReOpen_Resource_Keeps_New_Name()
        {
            ExplorerUIMap.Filter_Explorer(ResourceToRename);
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();

            ExplorerUIMap.Rename_Explorer_First_Item(newName);
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            UIMap.Click_Close_Workflow_Tab_Button();
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.BreadcrumbbarList.KeepNewNameRenamedListItem.DisplayText, newName);
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

        #endregion
    }
}
