using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
using System.Windows.Input;
using MouseButtons = System.Windows.Forms.MouseButtons;
using System;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;
using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
using System.Drawing;
using System.IO;
using TechTalk.SpecFlow;
using Warewolf.UI.Tests.WorkflowServiceTesting.WorkflowServiceTestingUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;
using Warewolf.UI.Tests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UI.Tests.WebSource.WebSourceUIMapClasses;
using Warewolf.UI.Tests.EmailSource.EmailSourceUIMapClasses;
using Warewolf.UI.Tests.ExchangeSource.ExchangeSourceUIMapClasses;
using Warewolf.UI.Tests.Deploy.DeployUIMapClasses;
using Warewolf.UI.Tests.DependencyGraph.DependencyGraphUIMapClasses;

namespace Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses
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
            Assert.AreEqual(resource, MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem.ItemEdit.Text);
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
            Point point;
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.WaitForControlCondition((control) => { return control.TryGetClickablePoint(out point); });
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem);
        }

        [Given(@"I Select Make Current Version")]
        [When(@"I Select Make Current Version")]
        [Then(@"I Select Make Current Version")]
        public void Select_Make_Current_Version()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.MakeCurrentVersionMenuItem);
            Mouse.Click(DialogsUIMap.MessageBoxWindow.YesButton, new Point(32, 5));
        }

        [Given(@"I Select Delete Version")]
        [When(@"I Select Delete Version")]
        [Then(@"I Select Delete Version")]
        public void Select_Delete_Version()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete);
            Mouse.Click(DialogsUIMap.MessageBoxWindow.YesButton, new Point(32, 5));
        }

        [Given(@"I DoubleClick Explorer Localhost Second Item")]
        [When(@"I DoubleClick Explorer Localhost Second Item")]
        [Then(@"I DoubleClick Explorer Localhost Second Item")]
        public void DoubleClick_Explorer_Localhost_Second_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem);
        }

        [When(@"I DoubleClick Explorer Localhost First Item First SubItem")]
        public void DoubleClick_Explorer_Localhost_First_Item_First_SubItem_Item()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem);
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

        [Given(@"Explorer Does Not Contain First Item First Sub Item")]
        [Then(@"Explorer Does Not Contain First Item First Sub Item")]
        public void ExplorerDoesNotContainFirstItemFirstSubItem()
        {
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem), "Explorer contains a first item with one sub item under it after moving the sub item out.");
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
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "First item does not Exist on the Explorer");
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.Exists, "Second item does not Exist on the Explorer");
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
        
        [When(@"I Select Remote Connection Integration From Explorer")]
        public void Select_RemoteConnectionIntegration_From_Explorer()
        {
            var toggleButton = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton;
            Mouse.Click(toggleButton, new Point(136, 7));
            UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text.WaitForControlExist(60000);
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text.Exists, "Remote Connection Integration does not appear in the explorer connect control.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(138, 6));
        }

        [When(@"I Select Local Server Source From Explorer")]
        public void Select_LocalServerSource_From_Explorer()
        {
            var toggleButton = MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton;
            Mouse.Click(toggleButton, new Point(136, 7));
            UIMap.MainStudioWindow.ComboboxListItemAsLocalServerSource.Text.WaitForControlExist(60000);
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsLocalServerSource.Text.Exists, "Local Server Source does not appear in the explorer connect control.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsLocalServerSource.Text, new Point(138, 6));
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

        [Given(@"I open ""(.*)"" workflow")]
        [When(@"I open ""(.*)"" workflow")]
        public void IOpenWorkflow(string resourceName)
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = resourceName;
            Open_Explorer_First_Item_With_Double_Click();
            WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.WaitForControlExist();
        }

        [Given(@"I RightClick Explorer First Remote Server First Item")]
        [When(@"I RightClick Explorer First Remote Server First Item")]
        [Then(@"I RightClick Explorer First Remote Server First Item")]
        public void RightClick_Explorer_FirstRemoteServer_FirstItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
        }
        
        [When(@"I RightClick Explorer Localhost First Item")]
        public void RightClick_Explorer_Localhost_FirstItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [Given(@"I RightClick Explorer Localhost Second Item")]
        [When(@"I RightClick Explorer Localhost Second Item")]
        [Then(@"I RightClick Explorer Localhost Second Item")]
        public void RightClick_Explorer_Localhost_SecondItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [When(@"I RightClick Explorer Localhost First Item First SubItem")]
        public void RightClick_Explorer_Localhost_First_Item_First_SubItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
        }

        [When(@"I RightClick Explorer Localhost Second Item First SubItem")]
        public void RightClick_Explorer_Localhost_Second_Item_First_SubItem()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 9));
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
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.ClearFilterButton);
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

        [Given(@"I Select Explorer Remote Server Dropdown List")]
        [When(@"I Select Explorer Remote Server Dropdown List")]
        [Then(@"I Select Explorer Remote Server Dropdown List")]
        public void Select_Explorer_Remote_Server_Dropdown_List()
        {
            Mouse.Click(MainStudioWindow.RemoteConnectionItem);
        }

        [Given(@"I Click Explorer ServerCombobox ToggleButton")]
        [When(@"I Click Explorer ServerCombobox ToggleButton")]
        [Then(@"I Click Explorer ServerCombobox ToggleButton")]
        public void Click_Explorer_ServerCombobox_ToggleButton()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(217, 8));
        }

        [Given(@"I Debug Using Play Icon")]
        [When(@"I Debug Using Play Icon")]
        [Then(@"I Debug Using Play Icon")]
        public void Debug_Using_Play_Icon()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon.DrawHighlight();
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ExecuteIcon);
        }

        [Given(@"I Click New Server Button From Explorer Connect Control")]
        [When(@"I Click New Server Button From Explorer Connect Control")]
        [Then(@"I Click New Server Button From Explorer Connect Control")]
        public void Click_NewServerButton_From_ExplorerConnectControl()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.NewServerButton, new Point(11, 10));
        }
        
        [When(@"I Click Edit Server Button From Explorer Connect Control")]
        public void Click_EditServerButton_From_ExplorerConnectControl()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.EditServerButton, new Point(11, 10));
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab was not open.");
        }

        [When(@"I Refresh Explorer")]
        public void Click_Explorer_Refresh_Button()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerRefreshButton, new Point(10, 10));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [Given(@"I setup Public Permissions for ""(.*)"" for Remote Server")]
        public void SetupPublicPermissionsForForRemoteServer(string resource)
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost);
            Select_RemoteConnectionIntegration_From_Explorer();
            Playback.Wait(1000);
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

        public void Click_First_Remote_Server_On_Explorer_Tree()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer);
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
        
        [When(@"I Filter the Explorer with ""(.*)""")]
        public void Filter_Explorer(string FilterText)
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text != FilterText)
                MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text = FilterText;
        }

        [When(@"I validate and delete the existing resource with ""(.*)""")]
        public void WhenIValidateAndDeleteTheExistingResourceWith(string resourceName)
        {
            string resourcePath =  @"\\TST-CI-REMOTE\C$\ProgramData\Warewolf\Resources\" + resourceName;

            if (File.Exists(resourcePath))
            {
                File.Delete(resourcePath);
            }
        }

        [When(@"I Wait For Explorer Localhost Spinner")]
        public void WaitForExplorerLocalhostSpinner()
        {
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
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

        [Given(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        [When(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        [Then(@"I Try Remove ""(.*)"" From Remote Server Explorer")]
        public void I_Try_Remove_From_Remote_Server_Explorer(string ResourceName)
        {
            Select_RemoteConnectionIntegration_From_Explorer();
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

        public void TryClearExplorerFilter()
        {
            if (MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text != string.Empty)
            {
                Click_Explorer_Filter_Clear_Button();
                Click_Explorer_Refresh_Button();
            }
            Assert.IsTrue(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text == string.Empty, "Explorer filter textbox text value of " + MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text + " is not empty after clicking clear filter button.");
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

        [Given(@"I Select NewOracleSource From Explorer Context Menu")]
        [When(@"I Select NewOracleSource From Explorer Context Menu")]
        [Then(@"I Select NewOracleSource From Explorer Context Menu")]
        public void Select_NewOracleSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewOracleSource);
        }

        [When(@"I Connect To Remote Server")]
        public void ConnectToRemoteServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists, "Remote Connection Integration option does not exist in Source server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Text, new Point(226, 13));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner, 180000);
            Point point;
            Assert.IsFalse(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Checkbox.Spinner.TryGetClickablePoint(out point), "Timed out waiting for remote server resources to load after 3 minutes.");
        }

        [When(@"I Connect To Restricted Remote Server")]
        public void ConnectToRestrictedRemoteServer()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRestrictedRemoteConnection.Text.Exists, "Restricted Remote Connection option does not exist in Source server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsRestrictedRemoteConnection.Text, new Point(226, 13));
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [Given(@"I Connect To Server With Changed Auth")]
        [Then(@"I Connect To Server With Changed Auth")]
        public void ConnectToChangingServerAuth()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton, new Point(136, 7));
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsChangingServerAuthUITest.Exists, "ChangingServerAuthUITest option does not exist in Source server combobox.");
            Mouse.Click(UIMap.MainStudioWindow.ComboboxListItemAsChangingServerAuthUITest.Text, new Point(226, 13));
            Assert.IsFalse(UIMap.ControlExistsNow(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.SecondRemoteServer), "Server is duplicated in the explorer after changing auth.");
        }

        [When(@"I Select NewODBCSource From Explorer Context Menu")]
        [When(@"I Select NewODBCSource From Explorer Context Menu")]
        [When(@"I Select NewODBCSource From Explorer Context Menu")]
        public void Select_NewODBCSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewODBCSource);
        }

        [When(@"I Select NewCOMPluginSource From Explorer Context Menu")]
        [When(@"I Select NewCOMPluginSource From Explorer Context Menu")]
        [When(@"I Select NewCOMPluginSource From Explorer Context Menu")]
        public void Select_NewCOMPluginSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewCOMPluginSource);
        }

        [When(@"I Select NewRabbitMQSource From Explorer Context Menu")]
        [When(@"I Select NewRabbitMQSource From Explorer Context Menu")]
        [When(@"I Select NewRabbitMQSource From Explorer Context Menu")]
        public void Select_NewRabbitMQSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewRabbitMQSource);
        }

        [When(@"I Select NewWcfSource From Explorer Context Menu")]
        [When(@"I Select NewWcfSource From Explorer Context Menu")]
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

        [Given(@"I Select NewDropboxSource From Explorer Context Menu")]
        [When(@"I Select NewDropboxSource From Explorer Context Menu")]
        [Then(@"I Select NewDropboxSource From Explorer Context Menu")]
        public void Select_NewDropboxSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewDropboxSource);
        }

        [Given(@"I Select NewEmailSource From Explorer Context Menu")]
        [When(@"I Select NewEmailSource From Explorer Context Menu")]
        [Then(@"I Select NewEmailSource From Explorer Context Menu")]
        public void Select_NewEmailSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewEmailSource);
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "New email source tab does not exist after opening Email source tab");
        }

        [Given(@"I Select NewExchangeSource From Explorer Contex tMenu")]
        [When(@"I Select NewExchangeSource From Explorer Contex tMenu")]
        [Then(@"I Select NewExchangeSource From Explorer Contex tMenu")]
        public void Select_NewExchangeSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(77, 13));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Exists, "Explorer Context Menu did not appear after Right click on localhost");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewExchangeSource);
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "New exchange source tab does not exist after opening Email source tab");
        }

        [Given(@"I Select NewServerSource From Explorer Context Menu")]
        [When(@"I Select NewServerSource From Explorer Context Menu")]
        [Then(@"I Select NewServerSource From Explorer Context Menu")]
        public void Select_NewServerSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewServerSource);
        }

        [Given(@"I Select NewSharepointSource From Explorer Context Menu")]
        [When(@"I Select NewSharepointSource From Explorer Context Menu")]
        [Then(@"I Select NewSharepointSource From Explorer Context Menu")]
        public void Select_NewSharepointSource_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(72, 8));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.SourcesMenuItem.NewSharepointSource);
        }

        [Given(@"I Click Show Server Version From Explorer Context Menu")]
        [When(@"I Click Show Server Version From Explorer Context Menu")]
        [Then(@"I Click Show Server Version From Explorer Context Menu")]
        public void Click_ShowServerVersion_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowServerVersion, new Point(45, 13));
        }

        [Given(@"I Create New Folder ""(.*)"" In Explorer Second Item With Context Menu")]
        [When(@"I Create New Folder ""(.*)"" In Explorer Second Item With Context Menu")]
        [Then(@"I Create New Folder ""(.*)"" In Explorer Second Item With Context Menu")]
        public void Create_NewFolder_In_ExplorerSecondItem_With_ExplorerContextMenu(string FolderName)
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(126, 12));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.NewFolderMenuItem, new Point(78, 15));
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem.ItemEdit.Text = FolderName;
            Keyboard.SendKeys(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem.FirstSubItem, "{Enter}", ModifierKeys.None);
            UIMap.WaitForSpinner(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.Spinner);
        }

        [Given(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        [When(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        [Then(@"I Duplicate Explorer Localhost First Item With Context Menu")]
        public void Duplicate_ExplorerLocalhostFirstItem_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate.Exists, "Duplicate does not exist in explorer context menu.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Duplicate, new Point(62, 10));
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

        [Given(@"I Open Explorer First Item Tests With Context Menu")]
        [When(@"I Open Explorer First Item Tests With Context Menu")]
        [Then(@"I Open Explorer First Item Tests With Context Menu")]
        public void Open_ExplorerFirstItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests does not exist in explorer context menu.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "Workflow service tests tab is not open after clicking Tests context menu item in the explorer context menu.");
        }

        [Given(@"I Open Explorer First Item Version History From Explorer Context Menu")]
        [When(@"I Open Explorer First Item Version History From Explorer Context Menu")]
        [Then(@"I Open Explorer First Item Version History From Explorer Context Menu")]
        public void Open_ExplorerFirstItemVersionHistory_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(69, 10));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ShowVersionHistory, new Point(66, 15));
        }

        [Given(@"I Open Explorer First SubItem With Context Menu")]
        [When(@"I Open Explorer First SubItem With Context Menu")]
        [Then(@"I Open Explorer First SubItem With Context Menu")]
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

        [Given(@"I Select Show Version History From Explorer SecondItem Context Menu")]
        [When(@"I Select Show Version History From Explorer SecondItem Context Menu")]
        [Then(@"I Select Show Version History From Explorer SecondItem Context Menu")]
        public void Select_ShowVersionHistory_From_Explorer_SecondItem_ContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(77, 12));
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
        
        [When(@"I Select Deploy From Explorer Context Menu")]
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

        [Given(@"I Select Tests From Context Menu")]
        [When(@"I Select Tests From Context Menu")]
        [Then(@"I Select Tests From Context Menu")]
        public void Select_Tests_From_ExplorerContextMenu()
        {
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.Exists, "TestsTab does not exist after clicking view tests in the explorer context menu.");
        }

        [Given(@"I Click Run All Tests On First Localhost Item From Explorer Context Menu")]
        [When(@"I Click Run All Tests On First Localhost Item From Explorer Context Menu")]
        [Then(@"I Click Run All Tests On First Localhost Item From Explorer Context Menu")]
        public void Click_RunAllTests_On_FirstLocalhostItem_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem.Exists);
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.RunAllTestsMenuItem, new Point(82, 16));
        }

        [Given(@"I Run All Hello World Tests")]
        [When(@"I Run All Hello World Tests")]
        [Then(@"I Run All Hello World Tests")]
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
            DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.WaitForControlExist(60000);
            Assert.IsTrue(DependencyGraphUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.Exists, "Dependency graph tab is not showen after clicking show dependancies explorer content menu item.");
        }

        [Given(@"I Create New Workflow Of Explorer FirstItem With Explorer Context Menu")]
        [When(@"I Create New Workflow Of Explorer FirstItem With Explorer Context Menu")]
        [Then(@"I Create New Workflow Of Explorer FirstItem With Explorer Context Menu")]
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

        [Given(@"I Click Show Explorer First Item Tests From Explorer Context Menu")]
        [When(@"I Click Show Explorer First Item Tests From Explorer Context Menu")]
        [Then(@"I Click Show Explorer First Item Tests From Explorer Context Menu")]

        public void Show_ExplorerFirstItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after openning it by clicking the explorer context menu item.");
        }

        [Given(@"I Show Explorer First SubItem Tests From Explorer Context Menu")]
        [When(@"I Show Explorer First SubItem Tests From Explorer Context Menu")]
        [Then(@"I Show Explorer First SubItem Tests From Explorer Context Menu")]

        public void Show_ExplorerFirstSubItemTests_With_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.DrawHighlight();
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after opening it by clicking the explorer context menu item.");
        }

        [Given(@"I Show Explorer Second Item Tests From Explorer Context Menu")]
        [When(@"I Show Explorer Second Item Tests From Explorer Context Menu")]
        [Then(@"I Show Explorer Second Item Tests From Explorer Context Menu")]

        public void Show_ExplorerSecondItemTests_With_ExplorerContextMenu(string filter)
        {
            Filter_Explorer(filter);
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Assert.IsTrue(UIMap.MainStudioWindow.ExplorerContextMenu.Tests.Exists, "View tests option does not exist in context menu after right clicking an item in the explorer.");
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Tests);
            Assert.IsTrue(WorkflowServiceTestingUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.TestsTab.WorkSurfaceContext.ServiceTestView.Exists, "Workflow test tab does not exist after openning it by clicking the explorer context menu item.");
        }

        [Given(@"I Have ""(.*)"" Open")]
        [When(@"I Open ""(.*)"" With Double Click")]
        [Then(@"""(.*)"" is Open")]
        public void Open_Item_With_Double_Click(string ItemName)
        {
            Filter_Explorer(ItemName);
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Left, ModifierKeys.None, new Point(40, 9));
        }

        [Given(@"I Open Explorer First Item With Double Click")]
        [When(@"I Open Explorer First Item With Double Click")]
        [Then(@"I Open Explorer First Item With Double Click")]
        public void Open_Explorer_First_Item_With_Double_Click()
        {
            Mouse.DoubleClick(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Left, ModifierKeys.None, new Point(40, 9));
        }

        [Given(@"I Click View Api From Context Menu")]
        [When(@"I Click View Api From Context Menu")]
        [Then(@"I Click View Api From Context Menu")]
        public void Click_View_Api_From_Context_Menu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost, MouseButtons.Right, ModifierKeys.None, new Point(85, 11));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerEnvironmentContextMenu.ViewApisJsonMenuItem, new Point(71, 13));
        }

        [Given(@"I Click ViewSwagger From ExplorerContextMenu")]
        [When(@"I Click ViewSwagger From ExplorerContextMenu")]
        [Then(@"I Click ViewSwagger From ExplorerContextMenu")]
        public void Click_ViewSwagger_From_ExplorerContextMenu()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem, MouseButtons.Right, ModifierKeys.None, new Point(107, 9));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.ViewSwagger, new Point(82, 16));
        }

        [Given(@"I Delete Nested Hello World")]
        [When(@"I Delete Nested Hello World")]
        [Then(@"I Delete Nested Hello World")]
        public void Delete_Nested_Hello_World()
        {
            Mouse.Click(MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem, MouseButtons.Right, ModifierKeys.None, new Point(93, 14));
            Mouse.Click(UIMap.MainStudioWindow.ExplorerContextMenu.Delete, new Point(61, 15));
            Mouse.Click(DialogsUIMap.MessageBoxWindow.YesButton, new Point(7, 12));
        }
        
        [When(@"I Collapse Localhost")]
        public void Collapse_Localhost()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Expanded = false;
        }

        [When(@"I Expand Localhost")]
        public void Expand_Localhost()
        {
            MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Expanded = true;
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
