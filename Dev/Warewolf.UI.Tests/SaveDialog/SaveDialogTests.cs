using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using System.Drawing;

namespace Warewolf.UI.Tests.SaveDialog
{
    [CodedUITest]
    public class SaveDialogTests
    {
        private const string HelloWorld = "Hello World";
        private const string FolderToRename = "FolderToRename";
        private const string FolderRenamed = "FolderToRename_Renamed";

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Save_Dialog_Filter_Given_HelloWorld_Filters_Explorer_Tree()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer(HelloWorld);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.Exists, "No items in the explorer tree after filtering when there should be at exactly 1.");
            Assert.IsFalse(UIMap.ControlExistsNow(DialogsUIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem), "Too many items in the explorer tree after filtering when there should be at exactly 1.");
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }
        
        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Server_Context_Menu_Has_New_Folder_Only()
        {
            DialogsUIMap.RightClick_Save_Dialog_Localhost();
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem.Exists);
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Folder_Items_Context_Menu_Has_New_Folder_And_Rename()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer(FolderToRename);
            DialogsUIMap.RightClick_Save_Dialog_Localhost_First_Item();
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem.Exists);
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Resources_Items_Context_Menu_Has_Delete_And_Rename()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer(HelloWorld);
            DialogsUIMap.RightClick_Save_Dialog_Localhost_First_Item();
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem.Exists);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            DialogsUIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationInvalidChars()
        {
            DialogsUIMap.I_Enter_Invalid_Service_Name_Into_SaveDialog("Inv@lid N&m#");            
            Assert.IsFalse(DialogsUIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is ENABLED.");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationNameEndsWithNumber()
        {
            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog("TestingWF1");
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }
        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationNameEndsWithEmptySpace()
        {
            DialogsUIMap.I_Enter_Invalid_Service_Name_With_Whitespace_Into_SaveDialog("Test ");
            Assert.IsFalse(DialogsUIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void CloseSaveDialogRemovesExplorerFilter()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("Hello World");
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.Exists);
            DialogsUIMap.Click_SaveDialog_CancelButton();
            Playback.Wait(2000);
            ExplorerUIMap.ExplorerItemsAppearOnTheExplorerTree();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void RenameFolderFromSaveDialog()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderToRename");
            DialogsUIMap.RenameItemUsingShortcut();
            DialogsUIMap.Rename_Folder_From_Save_Dialog("FolderToRename_Renamed");
            DialogsUIMap.Click_SaveDialog_CancelButton();
            ExplorerUIMap.Filter_Explorer("FolderToRename_Renamed");
            ExplorerUIMap.ExplorerContainItem("FolderToRename_Renamed");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveFolderToSameLocationFromSaveDialog()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderToRename");
            DialogsUIMap.MoveFolderToRenameIntoLocalhost();
            DialogsUIMap.ResourceIsChildOfLocalhost("FolderToRename");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveFolderToFolderToRenameFromSaveDialog()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderTo");
            DialogsUIMap.MoveFolderToMoveIntoFolderToRename();
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderToMove");
            DialogsUIMap.FolderIsChildOfParentFolder("FolderToMove", "FolderToRename");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveResourceToLocalhostFromSaveDialog()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("ResourceToMove");
            DialogsUIMap.MoveResourceToLocalhost();
            DialogsUIMap.Filter_Save_Dialog_Explorer("FolderToMove");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Explorer does not contain a first item after filter.");
            ExplorerUIMap.ExplorerDoesNotContainFirstItemFirstSubItem();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void DoubleClickItemInSaveDialogDoesNotOpenResource()
        {
            DialogsUIMap.Filter_Save_Dialog_Explorer("Hello World");
            DialogsUIMap.DoubleClickResourceOnTheSaveDialog();
            DialogsUIMap.Click_SaveDialog_CancelButton();
            UIMap.ResourceDidNotOpen();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void PressEnterSavesResourceAndClosesSaveDialog()
        {
            var resourceFolder = "EnterSavesResourceFolder";
            DialogsUIMap.RightClick_Save_Dialog_Localhost();
            DialogsUIMap.Select_NewFolder_From_SaveDialogContextMenu();
            DialogsUIMap.Name_New_Folder_From_Save_Dialog(resourceFolder);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.Exists);
            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog("EnterSavesResource");
            WorkflowTabUIMap.Enter_Using_Shortcut();
            Point point;
            DialogsUIMap.SaveDialogWindow.WaitForControlCondition(control => !control.TryGetClickablePoint(out point), 60000);
            Assert.IsFalse(DialogsUIMap.SaveDialogWindow.Exists);
        }


        [TestMethod]
        [TestCategory("Save Dialog")]
        public void ClickingSave_ThenPressEnter_SavesResource_AndClosesSaveDialog()
        {
            WorkflowTabUIMap.Escape_Using_Shortcut();
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);

            var resourceFolder = "ClickSaveEnterSavesResourceFolder";
            DialogsUIMap.RightClick_Save_Dialog_Localhost();
            DialogsUIMap.Select_NewFolder_From_SaveDialogContextMenu();
            DialogsUIMap.Name_New_Folder_From_Save_Dialog(resourceFolder);
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.Exists);
            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog("ClickSaveEnterSavesResource");
            WorkflowTabUIMap.Enter_Using_Shortcut();
            Point point;
            DialogsUIMap.SaveDialogWindow.WaitForControlCondition(control => !control.TryGetClickablePoint(out point), 60000);
            Assert.IsFalse(DialogsUIMap.SaveDialogWindow.Exists);
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            ExplorerUIMap.Create_New_Workflow_In_LocalHost_With_Shortcut();
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            WorkflowTabUIMap.Save_Workflow_Using_Shortcut();
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
