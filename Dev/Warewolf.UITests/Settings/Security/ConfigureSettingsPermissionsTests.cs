using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsPermissionsTests
    {
        [TestMethod]
        [TestCategory("Settings")]
        public void Open_Settings_Tab_With_Ribbon_Button()
        {
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.Exists, "Security tab does not exist in the settings window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Exists, "Logging tab does not exist in the settings window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Exists, "Resource Permissions does not exist in the settings window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Server Permissions does not exist in the settings window");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ResourcePermissions.Row1.Exists, "Settings security tab resource permissions row1 does not exist");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_Administrator_And_Save()
        {
            UIMap.Check_Public_Administrator();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_Deploy_To_And_Save()
        {
            UIMap.Check_Public_Deploy_To();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking DeployTo.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking DeployTo.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_Deploy_From()
        {
            UIMap.Check_Public_Deploy_From();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking DeployFrom.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking DeployFrom.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_View_And_Save()
        {
            UIMap.Check_Public_View();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_Execute_And_Save()
        {
            UIMap.Check_Public_Execute();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ContributeCell.Public_ContributeCheckBox.Checked, "Public Contribute checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void Check_Public_Contribute_And_Save()
        {
            UIMap.Check_Public_Contribute();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_AdministratorCell.Public_AdministratorCheckBox.Checked, "Public Administrator checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployToCell.Public_DeployToCheckBox.Checked, "Public DeployTo checkbox IS checked after Checking Administrator.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_DeployFromCell.Public_DeployFromCheckBox.Checked, "Public DeployFrom checkbox IS checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ViewCell.Public_ViewCheckBox.Checked, "Public View checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Public_ExecuteCell.Public_ExecuteCheckBox.Checked, "Public Execute checkbox is NOT checked after Checking Administrator.");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled.");
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void AddRemoveResourcePermission()
        {
            UIMap.SetResourcePermissions("Hello World", "Public", true, true, true);
            UIMap.Click_Settings_Resource_Permissions_Row1_Delete_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after clicking delete row button on existing resource permission in the security tab of the settings tab.");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
            UIMap.Click_Settings_Ribbon_Button();
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
