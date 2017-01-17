using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.SaveDialog
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
            UIMap.Filter_Save_Dialog_Explorer(HelloWorld);
            Assert.IsTrue(UIMap.ControlExistsNow(UIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem));
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem));
            UIMap.Click_SaveDialog_CancelButton();
        }
        
        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Server_Context_Menu_Has_New_Folder_Only()
        {
            UIMap.RightClick_Save_Dialog_Localhost();
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem.Exists);
            UIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Folder_Items_Context_Menu_Has_New_Folder_And_Rename()
        {
            UIMap.Filter_Save_Dialog_Explorer(FolderToRename);
            UIMap.RightClick_Save_Dialog_Localhost_First_Item();
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem.Exists);
            UIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void Resources_Items_Context_Menu_Has_Delete_And_Rename()
        {            
            UIMap.Filter_Save_Dialog_Explorer(HelloWorld);
            UIMap.RightClick_Save_Dialog_Localhost_First_Item();
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem.Exists);
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            UIMap.Click_SaveDialog_CancelButton();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationInvalidChars()
        {
            UIMap.I_Enter_Invalid_Service_Name_Into_SaveDialog("Inv@lid N&m#");            
            Assert.IsFalse(UIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is ENABLED.");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationNameEndsWithNumber()
        {
            UIMap.Enter_Valid_Service_Name_Into_Save_Dialog("TestingWF1");
            Assert.IsTrue(UIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }
        [TestMethod]
        [TestCategory("Save Dialog")]
        public void SaveDialogServiceNameValidationNameEndsWithEmptySpace()
        {
            UIMap.I_Enter_Invalid_Service_Name_With_Whitespace_Into_SaveDialog("Test ");
            Assert.IsFalse(UIMap.SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void CloseSaveDialogRemovesExplorerFilter()
        {
            UIMap.Filter_Save_Dialog_Explorer("Hello World");
            Assert.IsTrue(UIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.Exists);
            UIMap.Click_SaveDialog_CancelButton();
            UIMap.ExplorerItemsAppearOnTheExplorerTree();
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void RenameFolderFromSaveDialog()
        {
            UIMap.Filter_Save_Dialog_Explorer("FolderToRename");
            UIMap.RenameItemUsingShortcut();
            UIMap.Rename_Folder_From_Save_Dialog("FolderToRename_Renamed");
            UIMap.Click_SaveDialog_CancelButton();
            UIMap.Filter_Explorer("FolderToRename_Renamed");
            UIMap.ExplorerContainItem("FolderToRename_Renamed");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveFolderToSameLocationFromSaveDialog()
        {
            UIMap.Filter_Save_Dialog_Explorer("FolderToRename");
            UIMap.MoveFolderToRenameIntoLocalhost();
            UIMap.ResourceIsChildOfLocalhost("FolderToRename");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveFolderToFolderToRenameFromSaveDialog()
        {
            UIMap.Filter_Save_Dialog_Explorer("FolderTo");
            UIMap.MoveFolderToMoveIntoFolderToRename();
            UIMap.Filter_Save_Dialog_Explorer("FolderToMove");
            UIMap.FolderIsChildOfParentFolder("FolderToMove", "FolderToRename");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void MoveResourceToLocalhostFromSaveDialog()
        {
            UIMap.Filter_Save_Dialog_Explorer("ResourceToMove");
            UIMap.MoveResourceToLocalhost();
            UIMap.Filter_Save_Dialog_Explorer("FolderToMove");
            UIMap.ExplorerDoesNotContainItem("ResourceToMove");
        }

        [TestMethod]
        [TestCategory("Save Dialog")]
        public void DoubleClickItemInSaveDialogDoesNotOpenResource()
        {
            UIMap.Filter_Save_Dialog_Explorer("Hello World");
            UIMap.DoubleClickResourceOnTheSaveDialog();
            UIMap.Click_SaveDialog_CancelButton();
            UIMap.ResourceDidNotOpen();
        }
        
        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
            UIMap.Create_New_Workflow_In_LocalHost_With_Shortcut();
            UIMap.Make_Workflow_Savable_By_Dragging_Start();
            UIMap.Save_Workflow_Using_Shortcut();
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

        #endregion
    }
}
