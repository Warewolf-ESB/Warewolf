using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DotNetPluginSourceTests
    {
        const string SourceName = "CodedUITestDotNetPluginSource";

        [TestMethod]
        [TestCategory("DotNetPluginSource")]
        public void CreateDotNetPluginSource()
        {
            UIMap.Click_NewDotNetPluginSource_From_Explorer_Context_Menu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.AssemblyComboBox.Enabled, "Assembly Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.AssemblyDirectoryButton.Enabled, "Assembly Combobox Button is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Combobox Button is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.GACAssemblyDirectoryButton.Enabled, "GAC Assembly Combobox Button is not enabled");
        }

        [TestMethod]
        [TestCategory("DotNetPluginSource")]
        public void SelectAssemblyDLL()
        {
            string filePath = @"C:\UITestAssembly.dll";
            var fileStream = File.Create(filePath);
            fileStream.Close();

            UIMap.Click_NewDotNetPluginSource_From_Explorer_Context_Menu();
            UIMap.Click_AssemblyDirectoryButton_On_DotnetPluginSourceTab();
            Assert.IsTrue(UIMap.ChooseDLLWindow.FilterTextBox.Enabled, "Filter Combobox is not enabled.");
            Assert.IsTrue(UIMap.ChooseDLLWindow.DLLDataTree.Enabled, "DLL Data Tree is not enabled.");
            Assert.IsTrue(UIMap.ChooseDLLWindow.FilesTextBox.Enabled, "Files Textbox is not enabled.");
            Assert.IsTrue(UIMap.ChooseDLLWindow.SelectButton.Enabled, "Select button is not enabled.");
            Assert.IsTrue(UIMap.ChooseDLLWindow.CancelButton.Enabled, "Cancel button is not enabled.");
            UIMap.Select_DLLAssemblyFile_From_ChooseDLLWindow(filePath);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File ComboBox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Directory button is not enabled");
            UIMap.Click_ConfigFileDirectoryButton_On_DotnetPluginSourceTab();
            UIMap.Enter_ConfigFile_In_SelectFilesWindow();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            UIMap.Save_With_Ribbon_Button_And_Dialog("DotNet Plugin Source Assembly");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [TestMethod]
        [TestCategory("DotNetPluginSource")]
        public void SelectGACClearsDLLAssembly()
        {
            string filePath = @"C:\UITestAssembly.dll";
            var fileStream = File.Create(filePath);
            fileStream.Close();

            UIMap.Click_NewDotNetPluginSource_From_Explorer_Context_Menu();
            UIMap.Click_AssemblyDirectoryButton_On_DotnetPluginSourceTab();
            Assert.IsTrue(UIMap.ChooseDLLWindow.FilterTextBox.Enabled, "Filter Combobox is not enabled.");
            UIMap.Select_DLLAssemblyFile_From_ChooseDLLWindow(filePath);
            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.GACAssemblyDirectoryButton);
            UIMap.Select_GACAssemblyFile_From_ChooseDLLWindow();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled.");
            Assert.IsTrue(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceWizardTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text), "Assembly Combobox did not clear text.");
            UIMap.Save_With_Ribbon_Button_And_Dialog("DotNet Plugin Source GAC Assembly");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
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

        #endregion
    }
}