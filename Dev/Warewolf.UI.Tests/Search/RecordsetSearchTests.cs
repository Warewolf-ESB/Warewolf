using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Search.SearchUIMapClasses;

namespace Warewolf.UI.Tests.Search
{
    [CodedUITest]
    public class RecordsetSearchTests
    {
        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Recordset_IsTrue_Message_Returns_Workflows_Containg_Given_Variable()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "ComputerProgrammer";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Variable search did not expected Result.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Existing_Recordset_All_Lower_Case_And_Match_Case_Is_False()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "computerprogrammer";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected Results 1 Row to be returned.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Existing_Recordset_All_Upper_Case_And_Match_Case_Is_False()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "COMPUTERPROGRAMMER";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected Results 1 Row to be returned.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Existing_Recordset_All_Lower_Case_And_Match_Case_Is_True()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "computerprogrammer";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = true;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Unexpected Results were returned after seaching.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Existing_Recordset_All_Upper_Case_And_Match_Case_Is_True()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "COMPUTERPROGRAMMER";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = true;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Unexpected Results were returned after seaching.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Partial_Existing_Recordset_And_Match_WholeWord_Is_False()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "ComputerProgr";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchwholewordCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected Results 1 Row to be returned.");
        }


        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_Partial_Existing_Recordset_And_Match_WholeWord_Is_True()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "ComputerProgr";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchwholewordCheckBox.Checked = true;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Expected Results 1 Row to be returned.");
        }

        [TestMethod]
        [TestCategory("Recordset Search")]
        public void Given_UnExisting_Recordset_IsTrue_Message_Returns_Workflows_Containg_Given_Variable()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "This Recordset W1ll N3ver Ex1sts!";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.WorkSurfaceContent.ContentDockManager.SearchViewUserControl.SearchResultsTable.ResultRow1), "Unexpected Results were returned after seaching.");
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
