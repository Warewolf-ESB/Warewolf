using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Settings.SettingsUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ConfigureSettingsLoggingTests
    {
        [TestMethod]
        [TestCategory("Settings")]
        public void Open_SettingsTab_Then_ConfigureLogging_UITest()
        {
            UIMap.Click_ConfigureSetting_From_Menu();
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.Exists, "Settings tab does not exist after the Configure/Setting Menu button is clicked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Exists, "Logging tab does not exist after the Configure/Setting Menu button is clicked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.Enabled, "Logging tab is disabled after the Configure/Setting Menu button is clicked");
            Assert.AreEqual("SECURITY", SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SECURITY.Name, "Current selected tab page is not Security after Configure/Setting Menu button is clicked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Current selected tab page is not Security after Configure/Setting Menu button is clicked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.Exists, "Warewolf Administrators row does not exist");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.DeleteCell.DeleteButton.Exists, "DeleteButton does not exist");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.DeployToCell.DeployToCheckBox.Checked, "DeployTo checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.DeployFromCell.DeployFromCheckBox.Checked, "DeployFrom checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.AdministratorCell.AdministratorCheckBox.Checked, "Administrator checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.ExecuteCell.ExecuteCheckBox.Checked, "Execute checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.ViewCell.ViewCheckBox.Checked, "View checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.WarewolfAdminROW.ContributeCell.ContributeCheckBox.Checked, "Contribute checkbox is Unchecked");
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.PublicROW.Exists, "Public row does not exist");
            SettingsUIMap.Select_LoggingTab();
            SettingsUIMap.Click_Server_Log_File_Button();
            SettingsUIMap.Click_Studio_Log_File();
        }

        #region Additional test attributes

        [TestInitialize()]
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

        #endregion
    }
}
