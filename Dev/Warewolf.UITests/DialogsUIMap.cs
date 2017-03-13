using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using System;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UITests.Common;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UITests.WebSource.WebSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.Tools.ControlFlow.ControlFlowToolsUIMapClasses;

namespace Warewolf.UITests.DialogsUIMapClasses
{
    [Binding]
    public partial class DialogsUIMap
    {
        public void ServicePickerDialog_CancelButton()
        {
            Mouse.Click(ServicePickerDialog.Cancel, new Point(57, 6));
        }

        public void Select_Attachments_From_SelectFilesWindow()
        {
            Mouse.DoubleClick(SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder);
            SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder.attachment1.CheckBox.Checked = true;
            SelectFilesWindow.DrivesDataTree.CTreeItem.AttachmentsForEmailFolder.attachment2.CheckBox.Checked = true;
            Assert.IsNotNull(SelectFilesWindow.FileNameTextBox.Text, "Files Name is empty even after selecting Files..");
            Mouse.Click(SelectFilesWindow.SelectButton);
        }

        public void Select_DLLAssemblyFile_From_ChooseDLLWindow(string fileName)
        {
            ChooseDLLWindow.FilterTextBox.Text = fileName.Replace(@"C:\", "");
            Mouse.Click(ChooseDLLWindow.DLLDataTree.CDrive, new Point(11, 14));
            Mouse.Click(ChooseDLLWindow.DLLDataTree.CDrive.FirstItem, new Point(69, 34));
            Assert.AreEqual(fileName, ChooseDLLWindow.FilesTextBox.Text);
            Mouse.Click(ChooseDLLWindow.SelectButton);
        }

        [Given(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [When(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        [Then(@"Explorer Items appear on the Save Dialog Explorer Tree")]
        public void ExplorerItemsAppearOnTheSaveDialogExplorerTree()
        {
            Assert.IsTrue(UIMap.ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem));
            Assert.IsTrue(UIMap.ControlExistsNow(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem));
        }

        public void Enter_ConfigFile_In_SelectFilesWindow()
        {
            Mouse.Click(SelectFilesWindow.DrivesDataTree.CTreeItem.swapfile);
            Mouse.Click(SelectFilesWindow.SelectButton);
        }

        public void Select_GACAssemblyFile_From_ChooseDLLWindow(string filter)
        {
            ChooseDLLWindow.FilterTextBox.Text = filter;
            ChooseDLLWindow.DLLDataTree.GAC.DataTreeItem.DrawHighlight();
            Mouse.Click(ChooseDLLWindow.DLLDataTree.GAC.DataTreeItem, new Point(122, 6));
            Assert.IsFalse(string.IsNullOrEmpty(ChooseDLLWindow.FilesTextBox.Text), "Files Textbox is empty.");
            ChooseDLLWindow.SelectButton.DrawHighlight();
            Mouse.Click(ChooseDLLWindow.SelectButton);
        }

        [Given(@"I Click Close Critical Error Dialog")]
        [When(@"I Click Close Critical Error Dialog")]
        [Then(@"I Click Close Critical Error Dialog")]
        public void Click_Close_Critical_Error_Dialog()
        {
            Mouse.Click(CriticalErrorWindow.CloseButton, new Point(9, 11));
        }

        [When(@"I Click Web Browser Error Messagebox OK Button")]
        public void Click_Web_Browser_Error_Messagebox_OK_Button()
        {
            Mouse.Click(WebBrowserErrorWindow.Pane.OKButton, new Point(30, 8));
        }

        [Given(@"I Click Close Error Dialog")]
        [When(@"I Click Close Error Dialog")]
        [Then(@"I Click Close Error Dialog")]
        public void Click_Close_Error_Dialog()
        {
            Mouse.Click(ErrorWindow.CloseButton, new Point(8, 9));
        }

        [When(@"I Click Select Windows Group OK Button")]
        public void Click_Select_Windows_Group_OK_Button()
        {
            Mouse.Click(SelectWindowsGroupDialog.OKPanel.OK, new Point(37, 9));
        }

        public void Resize_Decision_LargeTool()
        {
            Mouse.StartDragging(DecisionOrSwitchDialog, new Point(396, 387));
            Mouse.StopDragging(DecisionOrSwitchDialog, new Point(0, 450));
        }

        [Given(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        [When(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        [Then(@"I Hit Escape Key On The Keyboard on Activity Default Window")]
        public void WhenIHitEscapeKeyOnTheKeyboardOnActivityDefaultWindow()
        {
            Keyboard.SendKeys(DecisionOrSwitchDialog, "{Escape}", ModifierKeys.None);
        }

        [Then(@"The Case Dialog Must Be Open")]
        public void ThenTheCaseDialogMustBeOpen()
        {
            Mouse.DoubleClick(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch);
            Assert.IsTrue(DecisionOrSwitchDialog.Exists, "Switch case dialog does not exist after dragging onto switch case arm.");
            Mouse.Click(DecisionOrSwitchDialog.DoneButton);
        }

        [Given(@"I Click Decision Dialog Cancel Button")]
        [When(@"I Click Decision Dialog Cancel Button")]
        [Then(@"I Click Decision Dialog Cancel Button")]
        public void Click_Decision_Dialog_Cancel_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.CancelButton, new Point(10, 14));
        }

        [Given(@"I Click Decision Dialog Done Button")]
        [When(@"I Click Decision Dialog Done Button")]
        [Then(@"I Click Decision Dialog Done Button")]
        public void Click_Decision_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(10, 14));
            Assert.IsFalse(UIMap.ControlExistsNow(DecisionOrSwitchDialog), "Decision large view dialog still exists after the done button is clicked.");
        }

        [Given(@"I Click Switch Dialog Done Button")]
        [When(@"I Click Switch Dialog Done Button")]
        [Then(@"I Click Switch Dialog Done Button")]
        public void Click_Switch_Dialog_Done_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.DoneButton, new Point(24, 7));
            Assert.IsTrue(ControlFlowToolsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.WorkflowDesignerView.DesignerView.ScrollViewerPane.ActivityTypeDesigner.WorkflowItemPresenter.Flowchart.Switch.Exists, "Switch on the design surface does not exist");
        }

        [When(@"I Click Switch Dialog Cancel Button")]
        public void Click_Switch_Dialog_Cancel_Button()
        {
            Mouse.Click(DecisionOrSwitchDialog.CancelButton, new Point(23, 10));
        }

        [Given(@"Filtered Item Exists")]
        [When(@"Filtered Item Exists")]
        [Then(@"Filtered Item Exists")]
        public void FilteredItemExists()
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.Exists);
        }

        [Given(@"""(.*)"" is child of ""(.*)""")]
        [When(@"""(.*)"" is child of ""(.*)""")]
        [Then(@"""(.*)"" is child of ""(.*)""")]
        public void FolderIsChildOfParentFolder(string child, string parent)
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text.Contains(parent));
            Assert.AreEqual(child, SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text);
        }

        [Given(@"""(.*)"" is child of localhost")]
        [When(@"""(.*)"" is child of localhost")]
        [Then(@"""(.*)"" is child of localhost")]
        public void ResourceIsChildOfLocalhost(string child)
        {
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Exists);
            Assert.IsTrue(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text.Contains(child));
        }

        [Given(@"I Move resource to localhost")]
        [When(@"I Move resource to localhost")]
        [Then(@"I Move resource to localhost")]
        public void MoveResourceToLocalhost()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, new Point(90, 11));
        }

        [Given(@"I Move FolderToMove into FolderToRename")]
        [When(@"I Move FolderToMove into FolderToRename")]
        [Then(@"I Move FolderToMove into FolderToRename")]
        public void MoveFolderToMoveIntoFolderToRename()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.ThirdItem.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.SecondItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.ThirdItem, new Point(90, 11));
        }

        [Given(@"I Move FolderToRename into localhost")]
        [When(@"I Move FolderToRename into localhost")]
        [Then(@"I Move FolderToRename into localhost")]
        public void MoveFolderToRenameIntoLocalhost()
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(94, 11));
            Mouse.StopDragging(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, new Point(90, 11));
        }

        [Given(@"Context Menu Has Two Items")]
        [When(@"Context Menu Has Two Items")]
        [Then(@"Context Menu Has Two Items")]
        public void ThenContextMenuHasTwoItems()
        {
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem.Exists);
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.UINewFolderMenuItem.Exists);
            Point point;
            Assert.IsFalse(SaveDialogWindow.SaveDialogContextMenu.SourcesMenuItem.TryGetClickablePoint(out point));
            Assert.IsFalse(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem.TryGetClickablePoint(out point));
        }

        [Given(@"I Enter New Folder Name as ""(.*)""")]
        [When(@"I Enter New Folder Name as ""(.*)""")]
        [Then(@"I Enter New Folder Name as ""(.*)""")]
        public void EnterNewFolderNameAs(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Enter New Sub Folder Name as ""(.*)""")]
        [When(@"I Enter New Sub Folder Name as ""(.*)""")]
        [Then(@"I Enter New Sub Folder Name as ""(.*)""")]
        public void ThenIEnterNewSubFolderNameAs(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Name New Sub Folder as ""(.*)""")]
        [When(@"I Name New Sub Folder as ""(.*)""")]
        [Then(@"I Name New Sub Folder as ""(.*)""")]
        public void I_Name_New_Sub_Folder_As(string name)
        {
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Back}", ModifierKeys.None);
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.FirstSubItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Dont Name The Created Folder")]
        [When(@"I Dont Name The Created Folder")]
        [Then(@"I Dont Name The Created Folder")]
        public void ThenIDontNameTheCreatedFolder()
        {
            WpfEdit newFolderEdit = this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit;
            WpfButton saveButton = this.SaveDialogWindow.SaveButton;

            Keyboard.SendKeys(newFolderEdit, "{Right}{Enter}", ModifierKeys.None);
            // Click 'Save' button
            Mouse.Click(saveButton, new Point(22, 16));
        }

        [Given(@"I Name New Folder as ""(.*)""")]
        [When(@"I Name New Folder as ""(.*)""")]
        [Then(@"I Name New Folder as ""(.*)""")]
        public void Name_New_Folder_From_Save_Dialog(string name)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = name;
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Right}{Enter}", ModifierKeys.None);
        }

        [Given(@"I Hit Escape Key On The Keyboard")]
        [When(@"I Hit Escape Key On The Keyboard")]
        [Then(@"I Hit Escape Key On The Keyboard")]
        public void ThenIHitEscapeKeyOnTheKeyboard()
        {
            Keyboard.SendKeys(this.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit, "{Escape}", ModifierKeys.None);
        }

        [Given(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [When(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        [Then(@"I Rename Save Dialog Explorer First Item To ""(.*)""")]
        public void Rename_Folder_From_Save_Dialog(string filterText)
        {
            SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text = filterText;
        }

        [Given(@"I Click Save Ribbon Button to Open Save Dialog")]
        [When(@"I Click Save Ribbon Button to Open Save Dialog")]
        [Then(@"I Click Save Ribbon Button to Open Save Dialog")]
        public void Click_Save_Ribbon_Button_to_Open_Save_Dialog()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist after clicking save ribbon button.");
        }

        [Given(@"I Select New Folder From SaveDialog Context Menu")]
        [When(@"I Select New Folder From SaveDialog Context Menu")]
        [Then(@"I Select New Folder From SaveDialog Context Menu")]
        public void Select_NewFolder_From_SaveDialogContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem);
        }

        [Given(@"I RightClick Save Dialog Localhost First Item")]
        [When(@"I RightClick Save Dialog Localhost First Item")]
        [Then(@"I RightClick Save Dialog Localhost First Item")]
        public void RightClick_Save_Dialog_Localhost_First_Item()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I Rename Item using Shortcut")]
        [When(@"I Rename Item using Shortcut")]
        [Then(@"I Rename Item using Shortcut")]
        public void RenameItemUsingShortcut()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(77, 9));
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, "{F2}");
        }

        [Given(@"I Create New Folder Item using Shortcut")]
        [When(@"I Create New Folder Item using Shortcut")]
        [Then(@"I Create New Folder Item using Shortcut")]
        public void ThenICreateNewFolderItemUsingShortcut()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, new Point(77, 9));
            Keyboard.SendKeys(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem, "F", (ModifierKeys.Control | ModifierKeys.Shift));
        }

        [Given(@"I RightClick Save Dialog Localhost")]
        [When(@"I RightClick Save Dialog Localhost")]
        [Then(@"I RightClick Save Dialog Localhost")]
        public void RightClick_Save_Dialog_Localhost()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Assert.IsTrue(SaveDialogWindow.SaveDialogContextMenu.NewFolderMenuItem.Exists);
        }

        [Given(@"I Select Rename From SaveDialog Context Menu")]
        [When(@"I Select Rename From SaveDialog Context Menu")]
        [Then(@"I Select Rename From SaveDialog Context Menu")]
        private void Select_Rename_From_SaveDialog_ContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.RenameMenuItem);
        }

        [Given(@"I Remove WorkflowName From Save Dialog")]
        [When(@"I Remove WorkflowName From Save Dialog")]
        [Then(@"I Remove WorkflowName From Save Dialog")]
        public void Remove_WorkflowName_From_Save_Dialog()
        {
            SaveDialogWindow.ServiceNameTextBox.Text = "";
            Assert.AreEqual("Cannot be null", SaveDialogWindow.ErrorLabel.DisplayText, "Name cannot be null validation message does not appear");
            Assert.AreEqual(false, SaveDialogWindow.SaveButton.Enabled, "Save button on the Save dialog is enabled");
        }

        [Given(@"I Select Delete From SaveDialog Context Menu")]
        [When(@"I Select Delete From SaveDialog Context Menu")]
        [Then(@"I Select Delete From SaveDialog Context Menu")]
        public void Select_Delete_From_SaveDialog_ContextMenu()
        {
            Mouse.Click(SaveDialogWindow.SaveDialogContextMenu.DeleteMenuItem);
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeleteConfirmation.Exists);
        }

        [Given(@"I Click SaveDialog CancelButton")]
        [When(@"I Click SaveDialog CancelButton")]
        [Then(@"I Click SaveDialog CancelButton")]
        public void Click_SaveDialog_CancelButton()
        {
            Mouse.Click(SaveDialogWindow.CancelButton, new Point(6, 7));
        }

        [Given(@"I Click Duplicate From Duplicate Dialog")]
        [When(@"I Click Duplicate From Duplicate Dialog")]
        [Then(@"I Click Duplicate From Duplicate Dialog")]
        public void Click_Duplicate_From_Duplicate_Dialog()
        {
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Exists, "Duplicate button does not exist");
            Mouse.Click(SaveDialogWindow.DuplicateButton, new Point(26, 10));
        }

        [Given(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        [When(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        [Then(@"I Enter Service Name Into Save Dialog As ""(.*)"" and Append Unique Guid")]
        public void Enter_Service_Name_Into_Save_Dialog_and_Append_Unique_Guid(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName + Guid.NewGuid().ToString().Substring(0, 8);
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Click SaveDialog Save Button")]
        [When(@"I Click SaveDialog Save Button")]
        [Then(@"I Click SaveDialog Save Button")]
        public void Click_SaveDialog_Save_Button()
        {
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save button on the Save Dialog is not Enabled");
            Mouse.Click(SaveDialogWindow.SaveButton, new Point(25, 4));
        }

        [When(@"I Wait For Save Dialog Explorer Spinner")]
        public void WaitForSaveDialogExplorerSpinner()
        {
            UIMap.WaitForSpinner(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.Checkbox.Spinner);
        }

        [Given(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name With Whitespace Into Save Dialog As ""(.*)""")]
        public void I_Enter_Invalid_Service_Name_With_Whitespace_Into_SaveDialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name With Whitespace Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_With_Whitespace_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name Into Save Dialog As ""(.*)""")]
        public void I_Enter_Invalid_Service_Name_Into_SaveDialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains invalid characters", errorLabel.DisplayText, "Error is not the same as expected");
        }

        [Given(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Invalid Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Invalid_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.AreEqual("'Name' contains leading or trailing whitespace characters.", errorLabel.DisplayText, "Error is not the same as expected");
            Assert.IsFalse(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        [When(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        [Then(@"I Enter Service Name Into Duplicate Dialog As ""(.*)""")]
        public void Enter_Service_Name_Into_Duplicate_Dialog(string ServiceName)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.IsTrue(SaveDialogWindow.DuplicateButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [When(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        [Then(@"I Enter Service Name Into Save Dialog As ""(.*)""")]
        public void Enter_Valid_Service_Name_Into_Save_Dialog(string ServiceName)
        {
            Assert.IsTrue(SaveDialogWindow.Exists, "Save dialog does not exist on the Surface.");
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
        }

        [Given(@"same name error message is shown")]
        public void GivenSameNameErrorMessageIsShown()
        {
            Assert.AreEqual("An item with this name already exists in this folder.", SaveDialogWindow.ErrorLabel.DisplayText);
        }

        public void Enter_Service_Name_Into_Save_Dialog(string ServiceName, bool duplicate = false, bool invalid = false, bool nameHasWhiteSpace = false, SaveOrDuplicate saveOrDuplicate = SaveOrDuplicate.Save)
        {
            WpfText errorLabel = SaveDialogWindow.ErrorLabel;
            SaveDialogWindow.ServiceNameTextBox.Text = ServiceName;
            Assert.IsTrue(SaveDialogWindow.SaveButton.Enabled, "Save dialog save button is not enabled. Check workflow name is valid and that another workflow by that name does not already exist.");
        }

        [Given(@"I Double Click Resource On The Save Dialog")]
        [When(@"I Double Click Resource On The Save Dialog")]
        [Then(@"I Double Click Resource On The Save Dialog")]
        public void DoubleClickResourceOnTheSaveDialog()
        {
            Mouse.DoubleClick(SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I Select LocalHost on the Save Dialog")]
        [When(@"I Select LocalHost on the Save Dialog")]
        [Then(@"I Select LocalHost on the Save Dialog")]
        public void WhenISelectLocalHostOnTheSaveDialog()
        {
            Mouse.Click(SaveDialogWindow.ExplorerView.ExplorerTree.localhost);
        }

        [Given(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [When(@"I Filter Save Dialog Explorer with ""(.*)""")]
        [Then(@"I Filter Save Dialog Explorer with ""(.*)""")]
        public void Filter_Save_Dialog_Explorer(string FilterText)
        {
            var searchTextBox = SaveDialogWindow.ExplorerView.SearchTextBox;
            searchTextBox.Text = FilterText;
        }

        [When(@"I Click UpdateDuplicateRelationships")]
        public void Click_UpdateDuplicateRelationships()
        {
            SaveDialogWindow.UpdateDuplicatedRelat.Checked = true;
        }

        [Given(@"I Click MessageBox Cancel")]
        [When(@"I Click MessageBox Cancel")]
        [Then(@"I Click MessageBox Cancel")]
        public void ThenIClickMessageBoxCancel()
        {
            Mouse.Click(MessageBoxWindow.CancelButton);
        }

        [Then(@"Deploy Version Conflict Window Shows")]
        public void ThenDeployVersionConflictWindowShows()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.DeployVersionConflicText.Exists);
        }

        [Then(@"Deploy is Successfully")]
        [When(@"Deploy is Successfully")]
        [Given(@"Deploy is Successfully")]
        public void ThenDeployIsSuccessfully()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
            Assert.IsTrue(MessageBoxWindow.ResourcesDeployedSucText.Exists);
        }

        [Then(@"Message box window appears")]
        [When(@"Message box window appears")]
        [Given(@"Message box window appears")]
        public void ThenMessageBoxWindowAppears()
        {
            Assert.IsTrue(MessageBoxWindow.Exists);
        }

        [Given(@"I Click MessageBox No")]
        [When(@"I Click MessageBox No")]
        [Then(@"I Click MessageBox No")]
        public void Click_MessageBox_No()
        {
            MessageBoxWindow.NoButton.DrawHighlight();
            Mouse.Click(MessageBoxWindow.NoButton);
        }

        [Given(@"I Click MessageBox OK")]
        [When(@"I Click MessageBox OK")]
        [Then(@"I Click MessageBox OK")]
        [Given(@"I Click MessageBox OK")]
        public void Click_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Duplicate_Test_Name_MessageBox_Ok()
        {
            Assert.IsTrue(MessageBoxWindow.DuplicateTestNameText.Exists, "Duplicate test name message box does not appear on the surface.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Save Before Continuing MessageBox OK")]
        public void Click_Save_Before_Continuing_MessageBox_OK()
        {
            Assert.IsTrue(MessageBoxWindow.SaveBeforeAddingNewTestText.Exists, "Messagebox does not warn about unsaved tests after clicking create new test.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Click_DropNotAllowed_MessageBox_OK()
        {
            Assert.IsTrue(MessageBoxWindow.DropnotallowedText.Exists, "The Shown dialog is not Drop Not 'Allowed MessageBox'");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        public void Click_DeleteAnyway_MessageBox_OK()
        {
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy version conflicts MessageBox OK")]
        [When(@"I Click Deploy version conflicts MessageBox OK")]
        public void ClickDeployVersionConflictsMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.DeployVersionConflicText.Exists, "Deploy Version Conflicts MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy conflicts MessageBox OK")]
        [When(@"I Click Deploy conflicts MessageBox OK")]
        public void ClickDeployConflictsMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.DeployConflictsText.Exists, "Deploy Conflicts MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Then(@"I Click Deploy Successful MessageBox OK")]
        [When(@"I Click Deploy Successful MessageBox OK")]
        public void ClickDeploySuccessfulMessageBoxOK()
        {
            Assert.IsTrue(MessageBoxWindow.ResourcesDeployedSucText.Exists, "Deploy Successful MessageBox does not Exist");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
        }

        [Given(@"I Click MessageBox DeleteAnyway")]
        [When(@"I Click MessageBox DeleteAnyway")]
        [Then(@"I Click MessageBox DeleteAnyway")]
        [Given(@"I Click MessageBox DeleteAnyway")]
        public void Click_MessageBox_DeleteAnyway()
        {
            Mouse.Click(MessageBoxWindow.DeleteAnyway, new Point(35, 11));
        }

        [Given(@"I Click MessageBox Yes")]
        [When(@"I Click MessageBox Yes")]
        [Then(@"I Click MessageBox Yes")]
        public void Click_MessageBox_Yes()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(32, 5));
        }

        public void Click_Assign_Tool_url()
        {
            Assert.IsTrue(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist");
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
            Assert.IsTrue(MessageBoxWindow.OKButton.Exists, "Did you know popup does not exist after clicking workflow hyperlink.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(38, 12));
        }

        public void Click_Assign_Tool_url_On_Unpinned_Tab()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink.Exists, "Url hyperlink does not exist on unpinned tab.");
            Mouse.Click(UIMap.MainStudioWindow.UnpinnedTab.SplitPane.WorkSurfaceContext.TopScrollViewerPane.UnsavedWorkflowLinkText.Hyperlink, new Point(201, 10));
            Assert.IsTrue(MessageBoxWindow.OKButton.Exists, "Did you know popup does not exist after clicking workflow hyperlink on unpinned tab.");
            Mouse.Click(MessageBoxWindow.OKButton, new Point(38, 12));
        }

        [When(@"I Click Yes On The Confirm Delete")]
        public void Click_Yes_On_The_Confirm_Delete()
        {
            Mouse.Click(MessageBoxWindow.YesButton, new Point(39, 17));
        }

        [Given(@"I Try Click MessageBox No")]
        [When(@"I Try Click MessageBox No")]
        [Then(@"I Try Click MessageBox No")]
        public void TryClickMessageBoxNo()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (UIMap.ControlExistsNow(MessageBoxWindow.NoButton))
                {
                    Click_MessageBox_No();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging message box before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging message box to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }

        }

        [When(@"I Try Click Message Box OK")]
        [Then(@"I Try Click Message Box OK")]
        [Given(@"I Try Click Message Box OK")]
        public void TryClickMessageBoxOK()
        {
            var TimeBefore = System.DateTime.Now;
            try
            {
                if (UIMap.ControlExistsNow(MessageBoxWindow.OKButton))
                {
                    Mouse.Click(MessageBoxWindow.OKButton, new Point(35, 11));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught a " + e.Message + " trying to close a hanging message box before the test starts.");
            }
            finally
            {
                Console.WriteLine("No hanging message box to clean up after trying for " + (System.DateTime.Now - TimeBefore).Milliseconds.ToString() + "ms.");
            }
        }

        public void Filter_ServicePicker_Explorer(string FilterText)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = FilterText;
            UIMap.WaitForControlVisible(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void Click_Service_Picker_Dialog_OK()
        {
            Mouse.Click(ServicePickerDialog.OK, new Point(52, 10));
        }

        public void Click_ServicePickerDialog_CancelButton()
        {
            Mouse.Click(ServicePickerDialog.Cancel, new Point(57, 6));
        }

        public void Click_Service_Picker_Dialog_Refresh_Button()
        {
            Mouse.Click(ServicePickerDialog.Explorer.Refresh, new Point(10, 11));
            UIMap.WaitForSpinner(ServicePickerDialog.Explorer.ExplorerTree.Localhost.Checkbox.Spinner);
        }

        public void Select_First_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
            Playback.Wait(500);
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        [Given(@"I Select ""(.*)"" From Service Picker")]
        [When(@"I Select ""(.*)"" From Service Picker")]
        [Then(@"I Select ""(.*)"" From Service Picker")]
        public void Select_SubItem_Service_From_Service_Picker_Dialog(string ServiceName)
        {
            ServicePickerDialog.Explorer.FilterTextbox.Text = ServiceName;
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1.TreeItem11);
            Assert.IsTrue(ServicePickerDialog.OK.Enabled, "Service picker dialog OK button is not enabled.");
            Click_Service_Picker_Dialog_OK();
        }

        [Given(@"I Double Click Resource On The Service Picker")]
        [When(@"I Double Click Resource On The Service Picker")]
        [Then(@"I Double Click Resource On The Service Picker")]
        public void DoubleClick_FirstItem_From_ServicePicker_Tree()
        {
            var firstItem = ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1;
            Mouse.DoubleClick(firstItem);
        }

        public void Select_FirstItem_From_ServicePicker_Tree()
        {
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1);
        }

        [When(@"I Click Service Picker Dialog First Service In Explorer")]
        public void Click_Service_Picker_Dialog_First_Service_In_Explorer()
        {
            Mouse.Click(ServicePickerDialog.Explorer.ExplorerTree.Localhost.TreeItem1, new Point(91, 9));
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

        WorkflowServiceTestingUIMap WorkflowServiceTestingUIMap
        {
            get
            {
                if (_WorkflowServiceTestingUIMap == null)
                {
                    _WorkflowServiceTestingUIMap = new WorkflowServiceTestingUIMap();
                }

                return _WorkflowServiceTestingUIMap;
            }
        }

        private WorkflowServiceTestingUIMap _WorkflowServiceTestingUIMap;

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

        ControlFlowToolsUIMap ControlFlowToolsUIMap
        {
            get
            {
                if (_ControlFlowToolsUIMap == null)
                {
                    _ControlFlowToolsUIMap = new ControlFlowToolsUIMap();
                }

                return _ControlFlowToolsUIMap;
            }
        }

        private ControlFlowToolsUIMap _ControlFlowToolsUIMap;
    }
}
