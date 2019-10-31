﻿
using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class Folders
    {
        const string ResourceCreatedInFolder = "Resource Created In Folder";

        [TestMethod]
        [TestCategory("Explorer")]
        public void Create_Resource_InFolderUITest()
        {
            var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources\Acceptance Tests";
            ExplorerUIMap.Filter_Explorer("Acceptance Tests");
            ExplorerUIMap.Create_NewWorkflow_Of_ExplorerFirstItem_With_ExplorerContextMenu();
            WorkflowTabUIMap.Make_Workflow_Savable();
            UIMap.Save_With_Ribbon_Button_And_Dialog(ResourceCreatedInFolder);
            var allFiles = Directory.GetFiles(resourcesFolder, "*.xml", SearchOption.AllDirectories);
            var firstOrDefault = allFiles.FirstOrDefault(s => s.Contains("Resource Created In Folder.xml"));
            if (firstOrDefault != null)
                File.Delete(firstOrDefault);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void CreateNewFolderInLocalHostUsingShortcutKeysUITest()
        {
            ExplorerUIMap.Click_LocalHost_Once();
            ExplorerUIMap.Create_New_Folder_Using_Shortcut();
            ExplorerUIMap.Filter_Explorer("New Folder");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Right_Click_On_The_FolderCount_ContextMenu_UITest()
        {
            WorkflowTabUIMap.Right_Click_On_The_Folder_Count();
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.ErrorWindow), "The studio throws an error when you right click on the folder count part of the explorer.");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Save_Resource_In_Nested_Folder_Saves_Correctly_UITest()
        {
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Make_Workflow_Savable();
            UIMap.Click_Save_RibbonButton();
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderD");
            Mouse.Click(DialogsUIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.FolderC.FolderD);
            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog("SuperNestedResource");
            DialogsUIMap.Click_SaveDialog_Save_Button();
            ExplorerUIMap.Filter_Explorer("SuperNestedResource");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FolderC.FolderD.SuperNestedResource.UIItemEdit.Exists);
            Assert.AreEqual(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.FolderC.FolderD.SuperNestedResource.UIItemEdit.Text, "SuperNestedResource");
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

        #endregion
    }
}