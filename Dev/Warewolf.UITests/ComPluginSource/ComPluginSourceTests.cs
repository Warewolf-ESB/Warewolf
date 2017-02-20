using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class COMPluginSourceTests
    {
        const string SourceName = "CodedUITestCOMPluginSource";

        [TestMethod]
        [TestCategory("Plugin Sources")]
        public void Create_ComPluginSource_From_ExplorerContextMenu_UITests()
        {
            //Create Source
            ExplorerUIMap.Select_NewCOMPluginSource_From_ExplorerContextMenu();
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Enabled, "Search Textbox is not enabled");
            UIMap.WaitForSpinner(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.RefreshSpinner);
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Enabled, "Data Tree is not enabled");
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.Enabled, "Refresh Button is not enabled");
            ComPluginSourceUIMap.Select_AssemblyFile_From_COMPluginDataTree("Microsoft");
            Assert.IsFalse(string.IsNullOrEmpty(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.AssemblyNameTextBox.Text), "Assembly Name Textbox is empty after selecting an assembly.");
            //Save Source
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            ExplorerUIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            ComPluginSourceUIMap.Click_COMPluginSource_CloseTabButton();
            //Open Source
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            UIMap.WaitForControlVisible(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree);
            Assert.IsFalse(string.IsNullOrEmpty(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.AssemblyNameTextBox.Text), "Assembly Name Textbox is empty after selecting an assembly.");
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

        ComPluginSourceUIMap ComPluginSourceUIMap
        {
            get
            {
                if (_ComPluginSourceUIMap == null)
                {
                    _ComPluginSourceUIMap = new ComPluginSourceUIMap();
                }

                return _ComPluginSourceUIMap;
            }
        }

        private ComPluginSourceUIMap _ComPluginSourceUIMap;

        #endregion
    }
}