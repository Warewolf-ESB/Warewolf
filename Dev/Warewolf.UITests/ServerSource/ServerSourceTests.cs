using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;
using Warewolf.UITests.WorkflowTab.WorkflowTabUIMapClasses;
using Warewolf.Web.UI.Tests.ScreenRecording;

namespace Warewolf.UITests.ServerSource
{
    [CodedUITest]
    public class ServerSourceTests
    {
        public TestContext TestContext { get; set; }
        private FfMpegVideoRecorder screenRecorder = new FfMpegVideoRecorder();

        private const string SourceName = "CodedUITestServerSource";

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
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
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewServerSource_GivenTabIsOpenedUserButtonSelected_ShouldHaveCredentialsControls()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Click_UserButton_On_ServerSourceTab();
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton.Selected, "User Radio Button not selected");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UsernameTextBox.Enabled, "Username Textbox not enabled");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PasswordTextBox.Enabled, "Password Textbox not enabled");
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void SaveNewServerSource_GivenSourceName()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Select_http_From_Server_Source_Wizard_Address_Protocol_Dropdown();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("tst-ci-remote");
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button not enabled");
            ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            //Save Source
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            ExplorerUIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Server Sources")]
        [Owner("Nkosinathi Sangweni")]
        public void EditServerSource_LoadCorrectly()
        {
            const string ExistingSourceName = "ExistingCodedUITestServerSource";
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(ExistingSourceName);
            ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WaitForControlReady(60000);
            Assert.IsTrue(ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab does not exist after clicking edit on an explorer server source context menu and waiting 1 minute (60000ms).");
            ServerSourceUIMap.Click_UserButton_On_ServerSourceTab();
            ServerSourceUIMap.Enter_RunAsUser_On_ServerSourceTab("IntegrationTester", "I73573r0");
            ServerSourceUIMap.Click_Server_Source_Wizard_Test_Connection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            ServerSourceUIMap.Click_Close_Server_Source_Wizard_Tab_Button();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(ExistingSourceName);
            Assert.AreEqual("IntegrationTester", ServerSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UsernameTextBox.Text, "The user name Texbox value is not set to Intergration Testet.");
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Server Sources")]
        [Owner("Pieter Terblanche")]
        public void DuplicateServerSource_AddsToConnectControl()
        {
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu(SourceName);
            const string newName = "DuplicatedCodedUITestServerSource";
            WorkflowTabUIMap.Enter_Duplicate_workflow_name(newName);
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            ExplorerUIMap.WaitForExplorerFirstRemoteServerSpinner();
            ExplorerUIMap.Filter_Explorer(newName);
            Assert.AreEqual(newName, ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.ItemEdit.Text, "First Item is not the same as Filtered input.");
            ExplorerUIMap.Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsDuplicatedConnection.Exists);
        }

        [TestMethod]
        [DeploymentItem(@"avformat-57.dll")]
        [DeploymentItem(@"avutil-55.dll")]
        [DeploymentItem(@"swresample-2.dll")]
        [DeploymentItem(@"swscale-4.dll")]
        [DeploymentItem(@"avcodec-57.dll")]
        [TestCategory("Server Sources")]
        public void DuplicateServerSource_Then_Delete_Removes_Item_From_Dropdown()
        {
            ExplorerUIMap.Click_Duplicate_From_ExplorerContextMenu("ExistingCodedUITestServerSource");
            const string newName = "CodedUITestServerSourceDuplicated";
            WorkflowTabUIMap.Enter_Duplicate_workflow_name(newName);
            DialogsUIMap.Click_Duplicate_From_Duplicate_Dialog();
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.CodedUITestServerSourceDuplicated.Exists, "This UI test expects ExistingCodedUITestServerSource to exist in the connect control dropdown list.");
            ExplorerUIMap.Filter_Explorer(newName);
            ExplorerUIMap.Delete_FirstResource_From_ExplorerContextMenu();
            DialogsUIMap.Click_Yes_On_The_Confirm_Delete();
            Mouse.Click(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ConnectControl.ServerComboBox.ToggleButton);
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.CodedUITestServerSourceDuplicated), "Server exists in connect control dropdown list after it was deleted from the explorer.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            screenRecorder.StartRecording(TestContext);
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        [TestCleanup]
        public void StopScreenRecording()
        {
            screenRecorder.StopRecording(TestContext);
        }

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