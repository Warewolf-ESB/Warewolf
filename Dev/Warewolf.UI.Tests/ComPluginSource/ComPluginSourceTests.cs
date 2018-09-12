﻿using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.ComPluginSource.ComPluginSourceUIMapClasses;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class COMPluginSourceTests
    {
        const string SourceName = "CodedUITestCOMPluginSource";

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [TestCategory("Plugin Sources")]
        public void Create_ComPluginSource_From_ExplorerContextMenu_UITests()
        {
            //Create Source
            ExplorerUIMap.Select_NewCOMPluginSource_From_ExplorerContextMenu();
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Enabled, "Search Textbox is not enabled");
            UIMap.WaitForSpinner(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Spinner);
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

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        [TestCategory("Plugin Sources")]
        [Owner("Pieter Terblanche")]
        public void CreateComPluginSource_GivenTabHasChanges_ClosingStudioPromptsChanges()
        {
            //Create Source
            ExplorerUIMap.Select_NewCOMPluginSource_From_ExplorerContextMenu();
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.SearchTextBox.Enabled, "Search Textbox is not enabled");
            UIMap.WaitForSpinner(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Spinner);
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.DataTree.Enabled, "Data Tree is not enabled");
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.WorkSurfaceContext.RefreshButton.Enabled, "Refresh Button is not enabled");
            ComPluginSourceUIMap.Select_AssemblyFile_From_COMPluginDataTree("Microsoft");

            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            DialogsUIMap.Click_MessageBox_Cancel();
            Assert.IsTrue(ComPluginSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.COMPlugInSourceTab.Exists);
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