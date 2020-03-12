using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Search.SearchUIMapClasses;

namespace Warewolf.UI.Tests.Search
{
    [CodedUITest]
    public class ServiceSearchTests
    {
        [TestMethod]
        [TestCategory("Service Search")]
        [Ignore]//TODO: Re-introduce this ui test once the build rig has been moved to the new domain hosted at premier.local
        public void Search_For_Hello_World_On_Remote_Server()
        {
            using (var _containerOps = new Depends(Depends.ContainerType.CIRemote))
            {
                Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
                Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchConnectControlCustom.ServerComboBox.ServersToggleButton);
                Mouse.Click(SearchUIMap.MainStudioWindow.ComboboxItemAsRemoteConnectionIntegration);
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchOptionsExpander.ServiceCheckBox.Checked = true;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .MatchcaseCheckBox.Checked = false;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .MatchwholewordCheckBox.Checked = false;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchInputEdit.Text = "Hello";
                Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchButton);
                Assert.IsTrue(
                    UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane
                        .TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable
                        .ResultRow1), "Expected at least 1 Row, but got 0 Rows.");
            }
        }

        [TestMethod]
        [TestCategory("Service Search")]
        [Ignore]//TODO: Re-introduce this ui test once the build rig has been moved to the new domain hosted at premier.local
        public void Given_Match_WholeWord_And_Case_And_Hello_World_Returns_Row()
        {
            using (var _containerOps = new Depends(Depends.ContainerType.CIRemote))
            {
                Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
                Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchConnectControlCustom.ServerComboBox.ServersToggleButton);
                Mouse.Click(SearchUIMap.MainStudioWindow.ComboboxItemAsRemoteConnectionIntegration);
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchOptionsExpander.ServiceCheckBox.Checked = true;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .MatchcaseCheckBox.Checked = true;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .MatchwholewordCheckBox.Checked = true;
                SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchInputEdit.Text = "Hello World";
                Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab
                    .SearchButton);
                Assert.IsTrue(
                    UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane
                        .TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable
                        .ResultRow1), "Expected at least 1 Row, but got 0 Rows.");
            }
        }        

        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_hello_And_No_Search_Option_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected 0 Rows but got Rows.");
        }

        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_Unexisting_Service_Name()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "Th1 S3rvice W1ll N3ver 3xist";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected 0 Rows but got Rows.");
        }

        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_WholeWord_Is_True()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchwholewordCheckBox.Checked = true;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected 0 Rows but got Rows.");
        }



        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_hello_And_Only_Service_Name_Is_False_And_Match_WholeWord_Is_False()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchwholewordCheckBox.Checked);
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected at least 1 Row, but got 0 Rows.");
        }


        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_Case_Is_True()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = true;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Hello search did not return a row as had expected.");
        }


        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_Case_Is_False()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected at least 1 Row, but got 0 Rows.");
        }

        [TestMethod]
        [TestCategory("Service Search")]
        public void Search_Hello_And_Only_Service_Name_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "Hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1.Exists, "Hello search did not return a row as had expected,");
        }

        #region Additional test attributes

        [TestInitialize]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        public ExplorerUIMap ExplorerUIMap
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
        public SearchUIMap SearchUIMap
        {
            get
            {
                if (_SearchUIMap == null)
                {
                    _SearchUIMap = new SearchUIMap();
                }

                return _SearchUIMap;
            }
        }

        private SearchUIMap _SearchUIMap;
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
