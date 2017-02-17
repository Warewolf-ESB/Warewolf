using System.IO;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.DialogsUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;
using Warewolf.UITests.Tools.ToolsUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DotNetPluginSourceTests
    {
        const string DLLAssemblySourceName = "CodedUITestDLLDotNetPluginSource";
        const string GACAssemblySourceName = "CodedUITestGACDotNetPluginSource";
        const string SourceNameToEdit = "DotNetPluginSourceToEdit";

        [TestMethod]
        [TestCategory("Plugin Sources")]
        public void Select_GACAssembly_DotNetPluginSource_UITests()
        {
            ExplorerUIMap.Click_NewDotNetPluginSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.Enabled, "Assembly Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyDirectoryButton.Enabled, "Assembly Combobox Button is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Combobox Button is enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton.Enabled, "GAC Assembly Combobox Button is not enabled");
            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton);
            DialogsUIMap.Select_GACAssemblyFile_From_ChooseDLLWindow("Microsoft");
            UIMap.Save_With_Ribbon_Button_And_Dialog(GACAssemblySourceName);
            ExplorerUIMap.Filter_Explorer(GACAssemblySourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");

        }

        [TestMethod]
        [TestCategory("Plugin Sources")]
        public void Edit_DotNetSource_Keeps_The_Changes_On_ReOpen_UITests()
        {
            const string newDll = @"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading.dll";
            const string newDll2 = @"C:\ProgramData\Warewolf\Resources\TestingDotnetDllCascading2.dll";
            ExplorerUIMap.RightClick_Localhost();
            ExplorerUIMap.Click_NewDotNetPluginSource_From_ExplorerContextMenu();            
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text = newDll;
            UIMap.Save_With_Ribbon_Button_And_Dialog("NewDotnetPluginSource_Explorer");
            UIMap.Click_Close_DotNetPlugin_Source_Tab();
            ExplorerUIMap.Filter_Explorer("NewDotnetPluginSource_Explorer");
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual(newDll, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text, "Assembly is not equal to updated text.");
            UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text = newDll2;            
            UIMap.Click_Save_Ribbon_Button_Without_Expecting_A_Dialog();
            UIMap.Click_Close_DotNetPlugin_Source_Tab();
            ExplorerUIMap.DoubleClick_Explorer_Localhost_First_Item();
            Assert.AreEqual(newDll2, UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text, "Assembly is not equal to updated text.");
        }

       
        [TestMethod]
        [TestCategory("Plugin Sources")]
        public void Select_AssemblyAndConfigFile_Then_Validate_Clear_On_GACSelection_UITests()
        {
            string filePath = @"C:\UITestAssembly.dll";
            var fileStream = File.Create(filePath);
            fileStream.Close();

            ExplorerUIMap.Click_NewDotNetPluginSource_From_ExplorerContextMenu();
            UIMap.Click_AssemblyDirectoryButton_On_DotnetPluginSourceTab();
            Assert.IsTrue(DialogsUIMap.ChooseDLLWindow.Exists, "Choose DLL Window does not exist.");
            DialogsUIMap.Select_DLLAssemblyFile_From_ChooseDLLWindow(filePath);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File ComboBox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileDirectoryButton.Enabled, "Config File Directory button is not enabled");
            UIMap.Click_ConfigFileDirectoryButton_On_DotnetPluginSourceTab();
            DialogsUIMap.Enter_ConfigFile_In_SelectFilesWindow();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyComboBox.Enabled, "GAC Assembly Combobox is enabled");
            Mouse.Click(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.GACAssemblyDirectoryButton);
            DialogsUIMap.Select_GACAssemblyFile_From_ChooseDLLWindow("Microsoft");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.ConfigFileComboBox.Enabled, "Config File Combobox is enabled.");
            Assert.IsTrue(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DotNetPluginSourceTab.WorkSurfaceContext.AssemblyComboBox.TextEdit.Text), "Assembly Combobox did not clear text.");

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