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
using Warewolf.UITests.WorkflowTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UITests.WebSource.WebSourceUIMapClasses;
using Warewolf.UITests.EmailSource.EmailSourceUIMapClasses;
using Warewolf.UITests.ExchangeSource.ExchangeSourceUIMapClasses;
using Warewolf.UITests.Deploy.DeployUIMapClasses;
using Warewolf.UITests.DependencyGraph.DependencyGraphUIMapClasses;

namespace Warewolf.UITests.ExplorerUIMapClasses
{
    [Binding]
    public partial class ExplorerUIMap
    {
        [When(@"I Drag Explorer First Item Onto The Second Item")]
        public void Drag_Explorer_First_Item_Onto_The_Second_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.EnsureClickable(new Point(90, 11));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(94, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 11));
        }

        [When(@"I Move Dice Roll To Localhost")]
        public void Move_Dice_Roll_To_Localhost()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.EnsureClickable(new Point(10, 10));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(92, 4));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(10, 10));
        }

        public void Create_New_Workflow_In_Explorer_First_Item_With_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, "W", (ModifierKeys.Control));
        }

        public void Create_New_Workflow_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, "W", (ModifierKeys.Control));
        }

        [Given(@"I have Hello World workflow on the Explorer")]
        [When(@"I have Hello World workflow on the Explorer")]
        [Then(@"I have Hello World workflow on the Explorer")]
        public void GivenIHaveHelloWorldWorkflowOnTheExplorer()
        {
            Filter_Explorer("Hello World");
        }

        [Given(@"First remote Item should be ""(.*)""")]
        [When(@"First remote Item should be ""(.*)""")]
        [Then(@"First remote Item should be ""(.*)""")]
        public void FirstRemoteItemShouldBe(string resource)
        {
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text == resource);
        }

        public void Select_Source_From_ExplorerContextMenu(String sourceName)
        {
            Filter_Explorer(sourceName);
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I DoubleClick Explorer Localhost First Item")]
        [When(@"I DoubleClick Explorer Localhost First Item")]
        [Then(@"I DoubleClick Explorer Localhost First Item")]
        public void DoubleClick_Explorer_Localhost_First_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I DoubleClick Explorer Localhost Second Item")]
        [When(@"I DoubleClick Explorer Localhost Second Item")]
        [Then(@"I DoubleClick Explorer Localhost Second Item")]
        public void DoubleClick_Explorer_Localhost_Second_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem);
        }

        [Given(@"Folder Is Removed From Explorer")]
        [When(@"Folder Is Removed From Explorer")]
        [Then(@"Folder Is Removed From Explorer")]
        public void ThenFolderIsRemovedFromExplorer()
        {
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem));
        }

        [Then(@"""(.*)"" Resource Exists In Windows Directory ""(.*)""")]
        public void ResourceExistsInWindowsDirectory(string serviceName, string path)
        {
            Filter_Explorer(serviceName);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);
        }

        [Given(@"Explorer Contain Item ""(.*)""")]
        [When(@"Explorer Contain Item ""(.*)""")]
        [Then(@"Explorer Contain Item ""(.*)""")]
        public void ExplorerContainItem(string itemName)
        {
            Assert.AreEqual(itemName, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text);
        }

        [Given(@"Save Dialog Explorer Contain Item ""(.*)""")]
        [When(@"Save Dialog Explorer Contain Item ""(.*)""")]
        [Then(@"Save Dialog Explorer Contain Item ""(.*)""")]
        public void ThenSaveDialogExplorerContainItem(string itemName)
        {
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.ExplorerView.ExplorerTree.localhost.FirstItem.UIItemEdit.Text.Contains(itemName));
        }

        [Given(@"Explorer Does Not Contain Item ""(.*)""")]
        [When(@"Explorer Does Not Contain Item ""(.*)""")]
        [Then(@"Explorer Does Not Contain Item ""(.*)""")]
        public void ExplorerDoesNotContainItem(string itemName)
        {
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem));
        }

        [Given(@"Explorer Contain Sub Item ""(.*)""")]
        [When(@"Explorer Contain Sub Item ""(.*)""")]
        [Then(@"Explorer Contain Sub Item ""(.*)""")]
        public void ExplorerContainSubFolder(string itemName)
        {
            Assert.AreEqual(itemName, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit.Text);
        }

        [Given(@"Explorer Items appear on the Explorer Tree")]
        [When(@"Explorer Items appear on the Explorer Tree")]
        [Then(@"Explorer Items appear on the Explorer Tree")]
        public void ExplorerItemsAppearOnTheExplorerTree()
        {
            Assert.IsTrue(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem), "First item does not Exist on the Explorer");
            Assert.IsTrue(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem), "Second item does not Exist on the Explorer");
        }

        [Given(@"Filter Textbox is cleared")]
        [When(@"Filter Textbox is cleared")]
        [Then(@"Filter Textbox is cleared")]
        public void ThenFilterTextboxIsCleared()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Assert.IsTrue(string.IsNullOrEmpty(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text), "Explorer Filter Textbox text is not blank after clicking the clear button.");
        }

        [Given(@"Filter Textbox has ""(.*)""")]
        [When(@"Filter Textbox has ""(.*)""")]
        [Then(@"Filter Textbox has ""(.*)""")]
        public void ThenFilterTextboxHas(string filterText)
        {
            Assert.AreEqual(filterText, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text);
        }

        [Given(@"I am connected on a remote server")]
        [When(@"I am connected on a remote server")]
        [Then(@"I am connected on a remote server")]
        public void GivenIAmConnectedOnARemoteServer()
        {
            Select_RemoteConnectionIntegration_From_Explorer();
            Click_Explorer_RemoteServer_Connect_Button();
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Spinner);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists);
        }

        [Then(@"Remote Server Refreshes")]
        public void ThenRemoteServerRefreshes()
        {
            Assert.IsTrue(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner));
            Assert.IsTrue(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Spinner));
            Point point;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Spinner.TryGetClickablePoint(out point));
        }

        [Given(@"I Create New Workflow using shortcut")]
        [When(@"I Create New Workflow using shortcut")]
        [Then(@"I Create New Workflow using shortcut")]
        public void Create_New_Workflow_In_LocalHost_With_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, "W", (ModifierKeys.Control));
        }

        public void Create_New_Folder_Using_Shortcut()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, new Point(74, 8));
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, "F", (ModifierKeys.Control | ModifierKeys.Shift));
        }

        [Given(@"I Double Click Localhost Server")]
        [When(@"I Double Click Localhost Server")]
        [Then(@"I Double Click Localhost Server")]
        public void DoubleClick_Localhost_Server()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost);
        }

        [When(@"I Drag Explorer First Sub Item Onto Second Sub Item")]
        public void Drag_Explorer_First_Sub_Item_Onto_Second_Sub_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, new Point(94, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem, new Point(90, 7));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Drag Explorer First Item Onto Second Sub Item")]
        public void Drag_Explorer_First_Item_Onto_Second_Item()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.EnsureClickable(new Point(90, 7));
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, new Point(94, 11));
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, new Point(90, 7));
        }

        [Given(@"I Select RemoteConnectionIntegration From Explorer")]
        [When(@"I Select RemoteConnectionIntegration From Explorer")]
        [Then(@"I Select RemoteConnectionIntegration From Explorer")]
        public void Select_RemoteConnectionIntegration_From_Explorer()
        {
            var toggleButton = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton;
            Mouse.Click(toggleButton, new Point(136, 7));
            Playback.Wait(1000);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(138, 6));
        }

        [Given(@"I Select Connected RemoteConnectionIntegration From Explorer")]
        [When(@"I Select Connected RemoteConnectionIntegration From Explorer")]
        [Then(@"I Select Connected RemoteConnectionIntegration From Explorer")]
        public void Select_ConnectedRemoteConnectionIntegration_From_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Playback.Wait(500);
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Text, new Point(138, 6));
        }

        [Then(@"Remote ""(.*)"" is open")]
        public void RemoteResourceIsOpen(string tabName)
        {
            Playback.Wait(500);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists);
            Assert.AreEqual(tabName, WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText);
        }

        [Then(@"Local ""(.*)"" is open")]
        public void LocalResourceIsOpen(string tabName)
        {
            Playback.Wait(1000);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "First Item does not exist in tree.");
            Assert.AreEqual(tabName, WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.TabDescription.DisplayText);
        }

        [When(@"I Select NewWorkFlowService From ContextMenu")]
        public void Select_NewWorkFlowService_From_ContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerEnvironmentContextMenu.NewWorkflow.Enabled, "NewWorkFlowService button is disabled.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerEnvironmentContextMenu.NewWorkflow, new Point(79, 13));
        }

        [When(@"I Search And Select DiceRoll")]
        public void Search_And_Select_DiceRoll()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox, new Point(165, 9));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = "Dice Roll";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }

        [When(@"I Search And Select HelloWolrd")]
        public void Search_And_Select_HelloWolrd()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox, new Point(165, 9));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = "Hello World";
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(101, 9));
        }

        [Given(@"I RightClick Explorer First Remote Server First Item")]
        [When(@"I RightClick Explorer First Remote Server First Item")]
        [Then(@"I RightClick Explorer First Remote Server First Item")]
        public void RightClick_Explorer_FirstRemoteServer_FirstItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
        }

        [Given(@"I RightClick Explorer Localhost First Item")]
        [When(@"I RightClick Explorer Localhost First Item")]
        [Then(@"I RightClick Explorer Localhost First Item")]
        public void RightClick_Explorer_Localhost_FirstItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I RightClick Localhost")]
        [When(@"I RightClick Localhost")]
        [Then(@"I RightClick Localhost")]
        public void RightClick_Localhost()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I Rename LocalFolder To SecondFolder")]
        [When(@"I Rename LocalFolder To SecondFolder")]
        [Then(@"I Rename LocalFolder To SecondFolder")]
        public void Rename_LocalFolder_To_SecondFolder(string newName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Rename);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Rename LocalWorkflow To SecodWorkFlow")]
        [When(@"I Rename LocalWorkflow To SecodWorkFlow")]
        [Then(@"I Rename LocalWorkflow To SecodWorkFlow")]
        public void Rename_LocalWorkflow_To_SecodWorkFlow()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Rename, new Point(73, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = "SecondWorkflow";
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Then(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Given(@"I Rename Folder to ""(.*)"" Using Shortcut KeyF2")]
        public void Rename_Folder_Using_Shortcut(string newName)
        {
            var firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            Mouse.Click(firstItem);
            Keyboard.SendKeys(firstItem, "{F2}");
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I Rename Explorer First item")]
        [Then(@"I Rename Explorer First item")]
        [Given(@"I Rename Explorer First item")]
        public void Rename_Explorer_First_Item(string newName)
        {
            var firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem;
            Mouse.Click(firstItem);
            Keyboard.SendKeys(firstItem, "{F2}");
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Then(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        [Given(@"I Rename Remote Folder to ""(.*)"" Using Shortcut KeyF2")]
        public void Rename_Remote_Folder_Using_Shortcut(string newName)
        {
            var firstItem = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem;
            Mouse.Click(firstItem);
            Keyboard.SendKeys(firstItem, "{F2}");
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Rename FolderItem ToNewFolderItem")]
        [When(@"I Rename FolderItem ToNewFolderItem")]
        [Then(@"I Rename FolderItem ToNewFolderItem")]
        public void Rename_FolderItem_ToNewFolderItem(string newName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            UIMap.Select_Rename_From_Explorer_ContextMenu();
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [When(@"I DoubleClick Explorer First Remote Server First Item")]
        public void DoubleClick_Explorer_First_Remote_Server_First_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, new Point(63, 11));
        }

        [Given(@"I Click Pin Toggle Explorer")]
        [When(@"I Click Pin Toggle Explorer")]
        [Then(@"I Click Pin Toggle Explorer")]
        public void Click_Pin_Toggle_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerUnpinBtn, new Point(12, 9));
        }

        [Given(@"I Click Explorer Filter Clear Button")]
        [When(@"I Click Explorer Filter Clear Button")]
        [Then(@"I Click Explorer Filter Clear Button")]
        public void Click_Explorer_Filter_Clear_Button()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.ClearFilterButton, new Point(6, 8));
        }

        [When(@"I Click Explorer Localhost First Item")]
        [Given(@"I Click Explorer Localhost First Item")]
        [Then(@"I Click Explorer Localhost First Item")]
        public void Click_Explorer_Localhost_First_Item()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I Click Explorer Remote Server Dropdown List")]
        [When(@"I Click Explorer Remote Server Dropdown List")]
        [Then(@"I Click Explorer Remote Server Dropdown List")]
        public void Click_Explorer_Remote_Server_Dropdown_List()
        {
            UIMap.WaitForControlVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(167, 10));
        }

        [Given(@"I Click Explorer Connect Remote Server Button")]
        [When(@"I Click Explorer Connect Remote Server Button")]
        [Then(@"I Click Explorer Connect Remote Server Button")]
        public void Click_Explorer_RemoteServer_Connect_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ConnectServerButton, new Point(11, 10));            
        }

        [Given(@"I Click Connect Control InExplorer")]
        [When(@"I Click Connect Control InExplorer")]
        [Then(@"I Click Connect Control InExplorer")]
        public void Click_Connect_Control_InExplorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
        }

        public void Debug_Using_Play_Icon()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
        }

        public void Click_Explorer_RemoteServer_Edit_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton, new Point(11, 10));
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab was not open.");
        }

        [Given(@"I Refresh Explorer")]
        [When(@"I Refresh Explorer")]
        [Then(@"I Refresh Explorer")]
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Refresh Explorer Withpout Waiting For Spinner")]
        public void RefreshExplorerWithpoutWaitingForSpinner()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
        }

        [Given(@"I setup Public Permissions for ""(.*)"" for Remote Server")]
        public void SetupPublicPermissionsForForRemoteServer(string resource)
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost);
            Select_RemoteConnectionIntegration_From_Explorer();
            Click_Explorer_RemoteServer_Connect_Button();
            Playback.Wait(1000);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer);
            UIMap.Click_Settings_RibbonButton();
            var deleteFirstResourceButton = SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.RemovePermissionButton;
            if (deleteFirstResourceButton.Enabled)
            {
                var isViewChecked = SettingsUIMap.FindViewPermissionsCheckbox(
                    SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                var isExecuteChecked = SettingsUIMap.FindExecutePermissionsCheckbox(
                    SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext
                        .SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1).Checked;

                if (isViewChecked && isExecuteChecked)
                {
                    SettingsUIMap.Click_Close_Settings_Tab_Button();
                    return;
                }
            }
            SettingsUIMap.Set_FirstResource_ResourcePermissions(resource, "Public", true, true);
            SettingsUIMap.Click_Close_Settings_Tab_Button();
        }

        [Given(@"I Save Valid Service With Ribbon Button And Dialog As ""(.*)""")]
        [When(@"I Save Valid Service With Ribbon Button And Dialog As ""(.*)""")]
        [Then(@"I Save Valid Service With Ribbon Button And Dialog As ""(.*)""")]
        public void Save_Valid_Service_With_Ribbon_Button_And_Dialog(string Name)
        {
            UIMap.Click_Save_RibbonButton();
            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog(Name);
            DialogsUIMap.Click_SaveDialog_Save_Button();
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        public void TryRefreshExplorerUntilOneItemOnly(int retries = 3)
        {
            while ((UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem) || UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.SecondItem)) && retries-- > 0)
            {
                Click_Explorer_Refresh_Button();
            }
        }

        public void Select_RemoteConnectionIntegration_From_Explorer_Remote_Server_Dropdown_List(WpfText comboboxListItem)
        {
            Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(comboboxListItem.Exists, "Server does not exist in explorer remote server drop down list.");
            Mouse.Click(comboboxListItem, new Point(79, 8));
        }

        public void Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected, new Point(80, 13));
        }

        [When(@"I Select NewRemoteServer From Explorer Server Dropdownlist")]
        public void Select_NewRemoteServer_From_Explorer_Server_Dropdownlist()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsNewRemoteServer.Exists, "New Remote Server... does not exist in explorer remote server drop down list");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsNewRemoteServer.NewRemoteServerItemText, new Point(114, 10));
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.Exists, "Server source wizard does not exist.");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.ToggleDropdown.Exists, "Server source wizard protocol dropdown does not exist.");
        }

        public void Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected.Exists, "localhost (connected) does not exist in explorer remote server drop down list");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected, new Point(94, 10));
            Assert.AreEqual("localhost", MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsLocalhost.DisplayText, "Selected remote server is not localhost");
        }

        public void Select_localhost_From_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(174, 8));
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected.Text);
        }

        [When(@"I Move FirstSubItem Into FirstItem Folder")]
        public void Move_FirstSubItem_Into_FirstItem_Folder()
        {
            Mouse.StartDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem);
            Mouse.StopDragging(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [When(@"I Click Localhost")]
        [Then(@"I Click Localhost")]
        [Given(@"I Click Localhost")]
        public void Click_LocalHost_Once()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost);
        }

        [Given(@"I Filter the Explorer with ""(.*)""")]
        [When(@"I Filter the Explorer with ""(.*)""")]
        [Then(@"I Filter the Explorer with ""(.*)""")]
        public void Filter_Explorer(string FilterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
        }

        [When(@"I validate and delete the existing resource with ""(.*)""")]
        public void WhenIValidateAndDeleteTheExistingResourceWith(string filterText)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = filterText;

            if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.Exists)
            {
                Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));

                Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete, new Point(87, 12));
                Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
                Assert.IsTrue(DialogsUIMap.MessageBoxWindow.YesButton.Exists, "Message box Yes button does not exist");

                Mouse.Click(DialogsUIMap.MessageBoxWindow.YesButton, new Point(32, 5));
            }
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = string.Empty;
        }

        [Given(@"I Try DisConnect To Remote Server")]
        [When(@"I Try DisConnect To Remote Server")]
        [Then(@"I Try DisConnect To Remote Server")]
        public void TryDisConnectToRemoteServer()
        {
            if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected))
            {
                Click_Explorer_RemoteServer_Connect_Button();
                Click_Connect_Control_InExplorer();
                Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected.Text);
            }
            else
            {
                Click_Connect_Control_InExplorer();
                if (UIMap.ControlExistsNow(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected))
                {
                    Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Text);
                    Click_Explorer_RemoteServer_Connect_Button();
                    Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegration.Exists);
                    Click_Connect_Control_InExplorer();
                    Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected.Text);
                }
                Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalhostConnected.Text);
            }
        }

        [When(@"I Wait For Explorer Localhost Spinner")]
        public void WaitForExplorerLocalhostSpinner()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
        }

        [Given(@"I Wait For Explorer First Remote Server Spinner")]
        [When(@"I Wait For Explorer First Remote Server Spinner")]
        [Then(@"I Wait For Explorer First Remote Server Spinner")]
        public void WaitForExplorerFirstRemoteServerSpinner()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner);
        }

        [Given(@"I Try Remove ""(.*)"" From Explorer")]
        [When(@"I Try Remove ""(.*)"" From Explorer")]
        [Then(@"I Try Remove ""(.*)"" From Explorer")]
        public void WhenITryRemoveFromExplorer(string ResourceName)
        {
            Filter_Explorer(ResourceName);
            try
            {
                var resourcesFolder = Environment.ExpandEnvironmentVariables("%programdata%") + @"\Warewolf\Resources";
                if (File.Exists(resourcesFolder + @"\" + ResourceName + ".xml"))
                {
                    UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                    if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem))
                    {
                        RightClick_Explorer_Localhost_FirstItem();
                        Select_Delete_From_ExplorerContextMenu();
                        DialogsUIMap.Click_MessageBox_Yes();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove resource " + ResourceName + " from the explorer.\n" + e.Message);
            }
            finally
            {
                TryClearExplorerFilter();
            }
        }

        [When(@"I Connect To Remote Server")]
        public void ConnectToRemoteServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Source server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(226, 13));
            Click_Explorer_RemoteServer_Connect_Button();
        }

        [When(@"I Try Connect To Remote Server")]
        public void TryConnectToRemoteServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            if (UIMap.ControlExistsNow(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected))
            {
                Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Exists, "Remote Connection Integration option does not exist in Source server combobox.");
                Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegrationConnected.Text, new Point(226, 13));
            }
            else
            {
                Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "RemoteConnectionIntegration item does not exist in remote server combobox list.");
                Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(138, 6));
                Click_Explorer_RemoteServer_Connect_Button();
            }
        }

        [Given(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        [When(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        [Then(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        public void I_Try_Remove_From_Remote_Server_Explorer(string ResourceName)
        {
            TryConnectToRemoteServer();
            Filter_Explorer(ResourceName);
            try
            {
                if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem))
                {
                    MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.DrawHighlight();
                    UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                    RightClick_Explorer_FirstRemoteServer_FirstItem();
                    Select_Delete_From_ExplorerContextMenu();
                    DialogsUIMap.Click_MessageBox_Yes();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove resource " + ResourceName + " from the explorer.\n" + e.Message);
            }
            finally
            {
                if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.ClearFilterButton))
                    TryClearExplorerFilter();
            }
        }

        public void TryDisconnectFromRemoteServerAndRemoveSourceFromExplorer(string SourceName)
        {
            try
            {
                if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.SelectedItemAsRemoteConnectionIntegrationConnected))
                {
                    Click_Explorer_RemoteServer_Connect_Button();
                }
                else
                {
                    Click_Connect_Control_InExplorer();
                    if (UIMap.ControlExistsNow(UIMap.MainStudioWindow.ComboboxListItemAsTSTCIREMOTEConnected))
                    {
                        Select_TSTCIREMOTEConnected_From_Explorer_Remote_Server_Dropdown_List();
                        Click_Explorer_RemoteServer_Connect_Button();
                    }
                }
                Select_LocalhostConnected_From_Explorer_Remote_Server_Dropdown_List();
                Filter_Explorer(SourceName);
                UIMap.WaitForControlNotVisible(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
                if (UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem))
                {
                    RightClick_Explorer_Localhost_FirstItem();
                    Select_Delete_From_ExplorerContextMenu();
                    DialogsUIMap.Click_MessageBox_Yes();
                }
                TryClearExplorerFilter();
            }
            catch (Exception e)
            {
                Console.WriteLine("Cleanup failed to remove remote server " + SourceName + ". Test may have crashed before remote server " + SourceName + " was connected.\n" + e.Message);
                TryClearExplorerFilter();
            }
        }

        public void TryClearExplorerFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text != string.Empty)
            {
                Click_Explorer_Filter_Clear_Button();
                Click_Explorer_Refresh_Button();
            }
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text == string.Empty, "Explorer filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text + " is not empty after clicking clear filter button.");
        }

        [When(@"I Click UnDock Explorer")]
        public void Click_UnDock_Explorer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerUnpinBtn, new Point(177, -13));
        }

        [Given(@"I Click New Web Source Explorer Context Menu Button")]
        [When(@"I Click New Web Source Explorer Context Menu Button")]
        [Then(@"I Click New Web Source Explorer Context Menu Button")]
        public void Click_NewWebSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewWebServiceSource);
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.LocalhostConnectedText.Exists);
            Assert.IsTrue(WebSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.Exists, "Web server address textbox does not exist on new web source wizard tab.");
        }

        [Given(@"I Click New SQLServerSource Explorer Context Menu")]
        [When(@"I Click New SQLServerSource Explorer Context Menu")]
        [Then(@"I Click New SQLServerSource Explorer Context Menu")]
        public void Click_NewSQLServerSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(75, 9));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewSQLServerSource);
        }

        [When(@"I Select NewMySQLSource From Explorer Context Menu")]
        public void Select_NewMySQLSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewMySQLSource);
        }

        [When(@"I Select NewPostgreSQLSource From Explorer Context Menu")]
        public void Select_NewPostgreSQLSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewPostgreSQLSource);
        }

        [When(@"I Select NewOracleSource From Explorer Context Menu")]
        public void Select_NewOracleSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewOracleSource);
        }

        [When(@"I Select NewODBCSource From Explorer Context Menu")]
        public void Select_NewODBCSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewODBCSource);
        }

        [When(@"I Select NewCOMPluginSource From Explorer Context Menu")]
        public void Select_NewCOMPluginSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewCOMPluginSource);
        }

        [When(@"I Select NewRabbitMQSource From Explorer Context Menu")]
        public void Select_NewRabbitMQSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewRabbitMQSource);
        }

        [When(@"I Select NewWcfSource From Explorer Context Menu")]
        public void Select_NewWcfSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewWcfSource);
        }

        [Given(@"I Click New DotNetPluginSource Explorer Context Menu")]
        [When(@"I Click New DotNetPluginSource Explorer Context Menu")]
        [Then(@"I Click New DotNetPluginSource Explorer Context Menu")]
        public void Click_NewDotNetPluginSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(67, 9));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDotNetPluginSource);
        }

        [When(@"I Select NewDropboxSource From Explorer Context Menu")]
        public void Select_NewDropboxSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDropboxSource);
        }

        [When(@"I Select NewEmailSource From Explorer Context Menu")]
        public void Select_NewEmailSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewEmailSource);
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "New email source tab does not exist after opening Email source tab");
        }

        [When(@"I Select NewExchangeSource From Explorer Contex tMenu")]
        public void Select_NewExchangeSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewExchangeSource);
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "New exchange source tab does not exist after opening Email source tab");
        }

        [When(@"I Select NewPluginSource From Explorer Context Menu")]
        public void Select_NewPluginSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDotNetPluginSource);
        }

        [When(@"I Select NewServerSource From Explorer Context Menu")]
        public void Select_NewServerSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewServerSource);
        }

        [When(@"I Select NewSharepointSource From Explorer Context Menu")]
        public void Select_NewSharepointSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewSharepointSource);
        }

        [When(@"I Click Show Server Version From Explorer Context Menu")]
        public void Click_ShowServerVersion_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowServerVersion, new Point(45, 13));
        }

        [When(@"I Create New Folder ""(.*)"" In Explorer Second Item With Context Menu")]
        public void Create_NewFolder_In_ExplorerSecondItem_With_ExplorerContextMenu(string FolderName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(126, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem, new Point(78, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.ItemEdit.Text = FolderName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, "{Enter}", ModifierKeys.None);
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [When(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        public void Duplicate_ExplorerLocalhostFirstItem_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate does not exist in explorer context menu.");
            Mouse.Click (UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate, new Point(62, 10));
            Assert.IsTrue(DialogsUIMap.SaveDialogWindow.Exists, "Duplicate dialog does not exist after clicking duplicate in the explorer context menu.");
        }

        [Given(@"I Open Explorer First Item Context Menu")]
        [Then(@"I Open Explorer First Item Context Menu")]
        [When(@"I Open Explorer First Item Context Menu")]
        public void Open_ExplorerFirstItem_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Open);
        }

        public void Click_AssignStep_InDebugOutput()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.AssignOnDebugOutput);
        }

        public void Click_DesicionStep_InDebugOutput()
        {
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.ContentPane.ContentDockManager.SplitPaneRight.DebugOutput.DebugOutputTree.DecisionOnDebugOutput);
        }

        [When(@"I Open Explorer First Item Tests With Context Menu")]
        [Then(@"I Open Explorer First Item Tests With Context Menu")]
        [Given(@"I Open Explorer First Item Tests With Context Menu")]
        public void Open_ExplorerFirstItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests does not exist in explorer context menu.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.RunAllButton.Exists, "Run all button does not exist on tests tab");
        }

        [When(@"I Open Explorer First Item Version History From Explorer Context Menu")]
        public void Open_ExplorerFirstItemVersionHistory_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory, new Point(66, 15));
        }

        [When(@"I Open Explorer First SubItem With Context Menu")]
        public void Open_ExplorerFirstSubItem_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(40, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists, "Open does not exist in explorer context menu.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Open);
        }

        [Given(@"I Delete FirstResource FromContextMenu")]
        [When(@"I Delete FirstResource FromContextMenu")]
        [Then(@"I Delete FirstResource FromContextMenu")]
        public void Delete_FirstResource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete);
        }

        [Given(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        [When(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        [Then(@"I Rename First Remote Resource FromContextMenu to ""(.*)""")]
        public void Rename_FirstRemoteResource_From_ExplorerContextMenu(string newName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Rename);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text = newName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit, "{Enter}", ModifierKeys.None);
        }

        [Given(@"I Select Show Version History From Explorer Context Menu")]
        [When(@"I Select Show Version History From Explorer Context Menu")]
        [Then(@"I Select Show Version History From Explorer Context Menu")]
        public void Select_ShowVersionHistory_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory);
        }

        [Given(@"I Duplicate FirstResource From Explorer Context Menu")]
        [When(@"I Duplicate FirstResource From Explorer Context Menu")]
        [Then(@"I Duplicate FirstResource From Explorer Context Menu")]
        public void Duplicate_FirstResource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate);
        }

        [Given(@"I Select Delete From Explorer Context Menu")]
        [When(@"I Select Delete From Explorer Context Menu")]
        [Then(@"I Select Delete From Explorer Context Menu")]
        public void Select_Delete_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete, new Point(87, 12));
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists, "Message box does not exist");
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.YesButton.Exists, "Message box Yes button does not exist");
        }

        [Given(@"I Select Deploy From Explorer Context Menu")]
        [When(@"I Select Deploy From Explorer Context Menu")]
        [Then(@"I Select Deploy From Explorer Context Menu")]
        public void Select_Deploy_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.DeployItem, new Point(57, 11));
            Playback.Wait(2000);
            Assert.IsTrue(DeployUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DeployTab.Exists, "DeployTab does not exist after clicking Deploy");
        }

        [Given(@"I Select NewWorkflow From Explorer Context Menu")]
        [When(@"I Select NewWorkflow From Explorer Context Menu")]
        [Then(@"I Select NewWorkflow From Explorer Context Menu")]
        public void Select_NewWorkflow_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.NewWorkflowItem);
        }

        [Given(@"I Select Open From Explorer Context Menu")]
        [When(@"I Select Open From Explorer Context Menu")]
        [Then(@"I Select Open From Explorer Context Menu")]
        public void Select_Open_From_ExplorerContextMenu()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Open.Exists);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Open);
        }

        [When(@"I Select Tests From Context Menu")]
        public void Select_Tests_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "TestsTab does not exist after clicking view tests in the explorer context menu.");
        }

        public void Click_RunAllTests_On_FirstLocalhostItem_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem.Exists);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem, new Point(82, 16));
        }

        [When(@"I Run All Hello World Tests")]
        public void WhenIRunAllHelloWorldTests()
        {
            Filter_Explorer("Hello World");
            Click_RunAllTests_On_FirstLocalhostItem_From_ExplorerContextMenu();
        }

        [Given(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        [When(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        [Then(@"I Select Show Dependencies In Explorer Context Menu for service ""(.*)""")]
        public void Select_ShowDependencies_In_ExplorerContextMenu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowDependencies);
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.Exists, "Dependency graph tab is not showen after clicking show dependancies explorer content menu item.");
        }

        public void Create_NewWorkflow_Of_ExplorerFirstItem_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(75, 10));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.NewWorkflowItem, new Point(79, 13));
        }

        [Given(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        [When(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        [Then(@"I Click Duplicate From Explorer Context Menu for Service ""(.*)""")]
        public void Click_Duplicate_From_ExplorerContextMenu(string ServiceName)
        {
            Filter_Explorer(ServiceName);
            Assert.AreEqual(ServiceName, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text, "First Item is not the same as Filtered input.");
            Duplicate_ExplorerLocalhostFirstItem_With_ExplorerContextMenu();
        }

        public void Show_ExplorerFirstItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after openning it by clicking the explorer context menu item.");
        }

        public void Show_ExplorerFirstSubItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.DrawHighlight();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after opening it by clicking the explorer context menu item.");
        }

        public void Show_ExplorerSecondItemTests_With_ExplorerContextMenu(string filter)
        {
            Filter_Explorer(filter);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after openning it by clicking the explorer context menu item.");
        }

        [When(@"I Open Explorer First Item With Double Click")]
        public void Open_Explorer_First_Item_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Left, ModifierKeys.None, new Point(40, 9));
        }

        [When(@"I Click View Api From Context Menu")]
        public void Click_View_Api_From_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(85, 11));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerEnvironmentContextMenu.ViewApisJsonMenuItem, new Point(71, 13));
        }

        [When(@"I Click ViewSwagger From ExplorerContextMenu")]
        public void Click_ViewSwagger_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ViewSwagger, new Point(82, 16));
        }

        [When(@"I Delete Nested Hello World")]
        public void Delete_Nested_Hello_World()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(93, 14));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete, new Point(61, 15));
            Mouse.Click(DialogsUIMap.MessageBoxWindow.YesButton, new Point(7, 12));
        }
        #region UIMaps
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

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;

        ServerSourceUIMap ServerSourceUIMap
        {
            get
            {
                if (_ServerSourceUIMap == null)
                {
                    _ServerSourceUIMap = new ServerSourceUIMap();
                }

                return _ServerSourceUIMap;
            }
        }

        private ServerSourceUIMap _ServerSourceUIMap;

        WebSourceUIMap WebSourceUIMap
        {
            get
            {
                if (_WebSourceUIMap == null)
                {
                    _WebSourceUIMap = new WebSourceUIMap();
                }

                return _WebSourceUIMap;
            }
        }

        private WebSourceUIMap _WebSourceUIMap;

        EmailSourceUIMap EmailSourceUIMap
        {
            get
            {
                if (_EmailSourceUIMap == null)
                {
                    _EmailSourceUIMap = new EmailSourceUIMap();
                }

                return _EmailSourceUIMap;
            }
        }

        private EmailSourceUIMap _EmailSourceUIMap;

        ExchangeSourceUIMap ExchangeSourceUIMap
        {
            get
            {
                if (_ExchangeSourceUIMap == null)
                {
                    _ExchangeSourceUIMap = new ExchangeSourceUIMap();
                }

                return _ExchangeSourceUIMap;
            }
        }

        private ExchangeSourceUIMap _ExchangeSourceUIMap;

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

        DependencyGraphUIMap DependencyGraphUIMap
        {
            get
            {
                if (_DependencyGraphUIMap == null)
                {
                    _DependencyGraphUIMap = new DependencyGraphUIMap();
                }

                return _DependencyGraphUIMap;
            }
        }

        private DependencyGraphUIMap _DependencyGraphUIMap;

        #endregion
    }
}
