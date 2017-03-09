using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.ServerSource.ServerSourceUIMapClasses;

namespace Warewolf.UITests.ServerSource
{
    [CodedUITest]
    public class ServerSourceTests
    {
        private const string SourceName = "CodedUITestServerSource";
        private const string ExistingSourceName = "ExistingCodedUITestServerSource";

        [TestMethod]
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
        [Owner("Nkosinathi Sangweni")]
        public void SaveNewServerSource_GivenSourceName()
        {
            //Create Source
            ExplorerUIMap.Select_NewServerSource_From_ExplorerContextMenu();
            ServerSourceUIMap.Click_UserButton_On_ServerSourceTab();
            ServerSourceUIMap.Enter_TextIntoAddress_On_ServerSourceTab("RSAKLFSVRGENDEV");
            ServerSourceUIMap.Enter_RunAsUser_On_ServerSourceTab("IntegrationTester", "I73573r0");
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
        [Owner("Nkosinathi Sangweni")]
        public void EditServerSource_LoadCorrectly()
        {
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
        

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
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

        #endregion
    }
}