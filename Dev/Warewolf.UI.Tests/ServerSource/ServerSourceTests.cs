using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Net.Mime;
using System.Windows.Input;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;
using Warewolf.UI.Tests.WorkflowTab.WorkflowTabUIMapClasses;

namespace Warewolf.UI.Tests.ServerSource
{
    [CodedUITest]
    public class ServerSourceTests
    {
        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewServer_GivenTabIsOpened_ShouldHaveDefaultControls()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab does not exist");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.Enabled, "Protocol Combobox not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.Enabled, "Address Combobox not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Enabled, "Public Radio button not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows Radio button not enabled");
            Assert.IsFalse(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button is enabled");
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewServerSource_GivenTabIsOpenedUserButtonSelected_ShouldHaveCredentialsControls()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Select_Server_Authentication_User();
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton.Selected, "User Radio Button not selected");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UsernameTextBox.Enabled, "Username Textbox not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password Textbox not enabled");
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        [Ignore] //TODO: Re-introduce this test once the move to the new domain (premier.local) is done
        public void SaveNewServerSource_GivenSourceName()
        {
            using (var _containerOps = new Depends(Depends.ContainerType.CIRemote))
            {
                //Create Source
                ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
                ServerSourceUIMap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
                ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote.premier.local");
                Keyboard.SendKeys("{ESC}");
                ServerSourceUIMap.Select_Server_Authentication_User();
                ServerSourceUIMap.Enter_RunAsUser_On_ServerSourceTab("WarewolfAdmin", "W@rEw0lf@dm1n");
                Assert.IsTrue(
                    ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan
                        .ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled,
                    "Test Connection button not enabled");
                ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source();
                //Save Source
                const string sourceName = "NewCodedUITestServerSource";
                UIMap.Save_With_Ribbon_Button_And_Dialog(sourceName);
                ExplorerUIMap.Filter_Explorer(sourceName);
                Assert.IsTrue(
                    ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem
                        .Exists, "Source did not save in the explorer UI.");
            }
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void EditServerSource_LoadCorrectly()
        {
            const string ExistingSourceName = "ExistingUITestServerSource";
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(ExistingSourceName);
            ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WaitForControlReady(60000);
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab does not exist after clicking edit on an explorer server source context menu and waiting 1 minute (60000ms).");
            UIMap._window.Get(SearchCriteria.ByAutomationId("Text")).Enter("localhost");
            UIMap._window.Get<RadioButton>(SearchCriteria.ByAutomationId("WindowsRadioButton")).Click();
            ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(ExistingSourceName);
            Assert.AreEqual("localhost", UIMap._window.Get<TextBox>(SearchCriteria.ByAutomationId("Text")).Text, "The server name Texbox value is not set to localhost.");
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Pieter Terblanche")]
        public void DuplicateServerSource_AddsToConnectControl()
        {
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu("ExistingUITestServerSource");
            const string newName = "DuplicatedServerSource";
            WorkflowTabUIMap.Enter_Duplicate_workflow_name(newName);
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            UIMap.WaitForSpinner(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.Checkbox.Spinner);
            ExplorerUIMap.Filter_Explorer(newName);
            Assert.AreEqual(newName, ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text, "First Item is not the same as Filtered input.");
            ExplorerUIMap.Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsDuplicatedConnection.Exists, "New server source does not exist in the connect control dropdown list.");
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        public void DuplicateServerSource_Then_Delete_Removes_Item_From_Dropdown()
        {
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu("ExistingUITestServerSource");
            const string newName = "CodedUITestServerSourceDuplicated";
            WorkflowTabUIMap.Enter_Duplicate_workflow_name(newName);
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton);
            Mouse.MoveScrollWheel(-10);
            Mouse.MoveScrollWheel(10);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.CodedUITestServerSourceDuplicated.Exists, "This UI test expects ExistingUITestServerSource to exist in the connect control dropdown list.");
            ExplorerUIMap.Filter_Explorer(newName);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_Yes_On_The_Confirm_Delete();
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton);
            try
            {
                Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.CodedUITestServerSourceDuplicated), "Server exists in connect control dropdown list after it was deleted from the explorer.");
            }
            finally
            {
                Keyboard.SendKeys("{ESC}", ModifierKeys.None);
            }
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        public void Try_Create_Server_Source_On_Restricted_Server()
        {
            try
            {
                UIMap.Click_Settings_RibbonButton();
                SettingsUIMap.AddNewServerPermissionsUser();

                ExplorerUIMap.Click_NewServerButton_From_ExplorerConnectControl();
                ServerSourceUIMap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
                ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("localhost");
                ServerSourceUIMap.Select_Server_Authentication_Public();
                Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button not enabled");
                ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button();
                Assert.IsFalse(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Can create new server source on server without permission to do so.");
            }
            finally
            {
                Keyboard.SendKeys(UIMap.MainStudioWindow, "^%{F4}");
            }
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Pieter Terblanche")]
        public void CreateNewServer_GivenTabHasChanges_ClosingStudioPromptsChanges()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote.premier.local");
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            DialogsUIMap.Click_MessageBox_Cancel();
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists);
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Pieter Terblanche")]
        public void CreateNewServer_GivenExistingOpenTabHasNoChanges_ClosingStudioPromptsChanges()
        {
            const string ExistingSourceName = "ExistingUITestServerSource";
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(ExistingSourceName);
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote.premier.local");
            Keyboard.SendKeys("{ESC}");
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            DialogsUIMap.Click_MessageBox_Cancel();
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists);
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Owner("Pieter Terblanche")]
        public void CreateNewServer_CreateNewWorkflow_ClosingWorkflowDoesNotError()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote.premier.local");
            Keyboard.SendKeys("{ESC}");
            UIMap.Click_NewWorkflow_RibbonButton();
            WorkflowTabUIMap.Make_Workflow_Savable_By_Dragging_Start();
            Mouse.Click(WorkflowTabUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WorkflowTab.CloseButton);
            DialogsUIMap.Click_MessageBox_Yes();
            UIMap.Save_With_Ribbon_Button_And_Dialog("WorkflowSaveError");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists);
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        [Ignore] //TODO: Re-introduce this test once the move to the new domain (premier.local) is done
        public void ClickingSave_ThenPressEnter_SavesServerResource_AndClosesSaveDialog()
        {
            _containerOps = new Depends(Depends.ContainerType.CIRemote);
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote.premier.local");
            Keyboard.SendKeys("{ESC}");
            ServerSourceUIMap.Select_Server_Authentication_User();
            ServerSourceUIMap.Enter_RunAsUser_On_ServerSourceTab("WarewolfAdmin", "W@rEw0lf@dm1n");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button not enabled");
            ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button_For_Valid_Server_Source();

            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);

            DialogsUIMap.Enter_Valid_Service_Name_Into_Save_Dialog("ClickSaveEnterSavesServerResource");
            WorkflowTabUIMap.Enter_Using_Shortcut();
            DialogsUIMap.SaveDialogWindow.WaitForControlCondition(control => !control.TryGetClickablePoint(out Point point), 60000);
            Assert.IsFalse(DialogsUIMap.SaveDialogWindow.Exists);
        }

        [TestMethod]
        [TestCategory("Server Sources")]
        public void DoubleClicking_ErrorMessage_SelectsWholeMessage()
        {
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("invalid address!");
            while (!ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.ContentManager.ErrorTextBlock.TryGetClickablePoint(out Point point))
            {
                ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            }
            Mouse.DoubleClick(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.ContentManager.ErrorTextBlock);
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.ContentManager.ErrorTextBlock.SelectionText.Length > 10);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        static Depends _containerOps;

        [TestCleanup]
        public void CleanupContainer() => _containerOps?.Dispose();

        public UIMap UIMap
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