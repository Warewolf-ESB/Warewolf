using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class ConfigureSettingsPermissionsTests
    {
        [TestMethod]
        [TestCategory("Settings")]
        public void Check_SettingsView_Then_SetPublicPermissions_And_SaveEnabled()
        {
            UIMap.Click_Settings_RibbonButton();
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "Security tab does not exist in the settings window");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Exists, "Logging tab does not exist in the settings window");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Exists, "Resource Permissions does not exist in the settings window");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Server Permissions does not exist in the settings window");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.Exists, "Settings security tab resource permissions row1 does not exist");
            //Check Public Deploy To
            SettingsUIMap.Check_Public_Deploy_To();
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking DeployTo.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            SettingsUIMap.Uncheck_Public_Deploy_To();
            //Check Public Deploy From
            SettingsUIMap.Check_Public_Deploy_From();
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking DeployFrom.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            SettingsUIMap.Uncheck_Public_Deploy_From();
            //Check Public Deploy View
            SettingsUIMap.Check_Public_View();
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            SettingsUIMap.Uncheck_Public_View();
            //Check Public Execute
            SettingsUIMap.Check_Public_Execute();
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            SettingsUIMap.Uncheck_Public_Execute();
            //Check Public Contribute
            SettingsUIMap.Check_Public_Contribute();
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            SettingsUIMap.Uncheck_Public_Contribute();
            SettingsUIMap.Uncheck_Public_Execute();
            SettingsUIMap.Uncheck_Public_View();
            //Check Public Administrator
            SettingsUIMap.Check_Public_Administrator();
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
            Keyboard.SendKeys(UIMap.MainStudioWindow, "^%{F4}");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void AddRemoveResourcePermission()
        {
            UIMap.Click_Settings_RibbonButton();
            SettingsUIMap.Set_FirstResource_ResourcePermissions("Hello World", "Public", true, true, true);
            SettingsUIMap.Click_Settings_Resource_Permissions_Row1_Delete_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after clicking delete row button on existing resource permission in the security tab of the settings tab.");
            SettingsUIMap.Click_Close_Settings_Tab_Button();
            DialogsUIMap.Click_MessageBox_Yes();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        [Ignore] //TODO: Re-introduce this test once the move to the new domain (premier.local) is done
        public void Edit_Server_Removes_Server_From_Explorer()
        {
            using (var _containerOps = new Depends(Depends.ContainerType.CIRemote))
            {
                ExplorerUIMap.Click_Explorer_Remote_Server_Dropdown_List();
                Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists);
                ExplorerUIMap.Select_Explorer_Remote_Server_Dropdown_List();
                Assert.IsTrue(
                    ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer
                        .Exists,
                    "Remote server is not loaded in the Explorer after selecting it from the connect control dropdown list.");
                ExplorerUIMap.Click_EditServerButton_From_ExplorerConnectControl();
                SettingsUIMap.ChangeServerAuthenticationType();
                Assert.IsFalse(
                    UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer
                        .ExplorerTree.FirstRemoteServer),
                    "Remote server is still loaded in the Explorer after clicking edit in the connect control.");
            }
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Save_With_Duplicate_Server_Permission()
        {
            UIMap.Click_Settings_RibbonButton();
            SettingsUIMap.Set_FirstResource_ResourcePermissions("Hello World", "", true, true, true);
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.Exists);
            Assert.IsTrue(DialogsUIMap.MessageBoxWindow.UITosavearesourcepersmText.Exists);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void PublicApisJson_Requires_ExecutePermission()
        {
            UIMap.Click_Settings_RibbonButton();
            SettingsUIMap.Uncheck_Public_Administrator();
            if (UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled)
            {
                Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);
            }
            using (var client = new WebClient
            {
                UseDefaultCredentials = true
            })
            {
                try
                {
                    client.DownloadString(new Uri("http://localhost:3142/public/Hello%20World/apis.json"));
                }
                catch (WebException e)
                {
                    StringAssert.Contains(e.Message, "Forbidden");
                }
            }
            SettingsUIMap.Check_Public_Administrator();
            if (UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled)
            {
                Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SaveButton);
                Playback.Wait(3000);
            }
            using (var client = new WebClient
            {
                Credentials = CredentialCache.DefaultNetworkCredentials
            })
            {
                StringAssert.Contains(client.DownloadString(new Uri("http://localhost:3142/public/Hello%20World/apis.json")), "/Hello World/apis.json");
            }
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
