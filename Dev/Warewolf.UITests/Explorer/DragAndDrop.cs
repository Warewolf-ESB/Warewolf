using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DragAndDrop
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void ShowDependencies_ExplorerContextMenuItem_UITest()
        {            
            UIMap.Select_ShowDependencies_In_ExplorerContextMenu("Hello World");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Selected, "Dependency graph show dependencies radio button is not selected.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.NestingLevelsText.Textbox.Exists, "Dependency graph nesting levels textbox does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.RefreshButton.Exists, "Refresh button does not exist on dependency graph");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Exists, "Show what depends on workflow does not exist after Show Dependencies is selected");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.DependencyGraphTab.WorksurfaceContext.DependencyView.ScrollViewer.ShowwhatdependsonthisRadioButton.Selected, "Show what depends on workflow radio button is not selected after Show dependecies is selected");
            UIMap.Click_Close_Dependecy_Tab();
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Moving_Resource_Into_The_Same_Folder_Does_Not_Create_Duplicate_UITest()
        {
            UIMap.Filter_Explorer("LoopTest");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);
            UIMap.Move_FirstSubItem_Into_FirstItem_Folder();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.FirstSubItem.Exists);            
            Assert.IsFalse(UIMap.ControlExistsNow(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.SecondSubItem));
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

        #endregion
    }
}
