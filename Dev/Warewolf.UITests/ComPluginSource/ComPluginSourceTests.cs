using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class COMPluginSourceTests
    {
        const string newSourceName = "CodedUITestCOMPluginSource";
        private const string editSourceName = "COMPluginSourceToEdit";

        [TestMethod]
        [TestCategory("Plugin Sources")]
        public void Create_ComPluginSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewCOMPluginSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Enabled, "Search Textbox is not enabled");
            UIMap.WaitForSpinner(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.RefreshSpinner);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Enabled, "Data Tree is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.Enabled, "Refresh Button is not enabled");
            UIMap.Select_AssemblyFile_From_COMPluginDataTree("Microsoft");
            Assert.IsFalse(string.IsNullOrEmpty(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.AssemblyNameTextBox.Text), "Assembly Name Textbox is empty after selecting an assembly.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(newSourceName);
            UIMap.Filter_Explorer(newSourceName);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            UIMap.Click_COMPluginSource_CloseTabButton();
        }

        [TestMethod]
        [TestCategory("Plugin Sources")]
        // ReSharper disable once InconsistentNaming
        public void Edit_ComPluginSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_Source_From_ExplorerContextMenu(editSourceName);
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