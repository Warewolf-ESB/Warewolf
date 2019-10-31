using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.UI.Tests.Settings.SettingsUIMapClasses;

namespace Warewolf.UI.Tests
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
            Assert.IsTrue(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.SecurityTab.SecurityWindow.ServerPermissions.Exists, "Current selected tab page is not Security after Configure/Setting Menu button is clicked");
            SettingsUIMap.Select_LoggingTab();
            Assert.IsTrue(UIMap.ControlExistsNow(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsView.LoggingTypesComboBox));
            SettingsUIMap.Click_Server_Log_File_Button();
            SettingsUIMap.Click_Studio_Log_File();
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void ChangeServerLoggingType_ThenSave_PersistsChanges_UITest()
        {
            UIMap.Click_ConfigureSetting_From_Menu();
            SettingsUIMap.Select_LoggingTab();
            SettingsUIMap.Select_Fatal_Event_Log();
            UIMap.Click_Save_RibbonButton();
            SettingsUIMap.Click_Close_Settings_Tab_Button();
            UIMap.Click_ConfigureSetting_From_Menu();
            SettingsUIMap.Select_LoggingTab();
            Assert.IsTrue(UIMap.ControlExistsNow(SettingsUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SettingsTab.WorksurfaceContext.SettingsView.TabList.LoggingTab.LogSettingsView.LoggingTypesComboBox.FatalOnlylogeventsthText));
        }

        [TestMethod]
        [TestCategory("Settings")]
        public void ChangeAuditsFilePath_ThenSave_PersistsChanges_UITest()
        {
            //Prepare for test
            var DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create), "Warewolf");
            var defaultPath = Path.Combine(DataPath, @"Audits");
            var changedPath = Path.Combine(DataPath, @"NewAudits");
            GivenAuditsFilePathIsDefaultValue(defaultPath);
            GivenDirectoryExistsAndIsEmpty(changedPath);

            //Execute test
            UIMap.Click_ConfigureSetting_From_Menu();
            SettingsUIMap.Select_LoggingTab();
            SettingsUIMap.Assert_Audits_File_Path(defaultPath);
            SettingsUIMap.Update_Audits_File_Path(changedPath);
            UIMap.Click_Save_RibbonButton();
            SettingsUIMap.Click_Close_Settings_Tab_Button();

            //Assert
            UIMap.Click_ConfigureSetting_From_Menu();
            SettingsUIMap.Select_LoggingTab();
            SettingsUIMap.Assert_Audits_File_Path(changedPath);
        }

        private void GivenAuditsFilePathIsDefaultValue(string defaultPath)
        {
            // RESET TO DEFAULT
            UIMap.Click_ConfigureSetting_From_Menu();
            SettingsUIMap.Select_LoggingTab();
            SettingsUIMap.Update_Audits_File_Path(defaultPath);
            UIMap.Click_Save_RibbonButton();
            SettingsUIMap.Click_Close_Settings_Tab_Button();
        }

        private static void GivenDirectoryExistsAndIsEmpty(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
                Assert.IsFalse(Directory.Exists(dirPath));
            }
            
            Directory.CreateDirectory(dirPath);
            Assert.IsTrue(Directory.Exists(dirPath));
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
