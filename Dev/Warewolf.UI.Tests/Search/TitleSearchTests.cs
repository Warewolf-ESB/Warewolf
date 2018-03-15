using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.Search.SearchUIMapClasses;

namespace Warewolf.UI.Tests.Search
{
    /// <summary>
    /// Summary description for SearchViewTests
    /// </summary>
    [CodedUITest]
    public class TitleSearchTests
    {
        [TestMethod]
        [TestCategory("Search View")]
        public void Shortcut_Cntr_Shift_F_Opens_Search_View()
        {
            ExplorerUIMap.Click_Explorer_Refresh_Button();
            Keyboard.SendKeys("^+F");
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after using shortcut Cntr+Shift+F.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Clicking_Search_Menu_Item_Opens_Search_View()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
        }
        [TestMethod]
        [TestCategory("Search View")]
        public void Search_For_Hello_World_On_Remote_Server()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchConnectControlCustom.ServerComboBox.ServersToggleButton);
            Mouse.Click(SearchUIMap.MainStudioWindow.RemoteConnectionInteCustom);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name), "Hello search did not return from Remote Server.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Given_InputVariable_IsTrue_Message_Returns_Workflows_ContaingMessage_Variable()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "[[SomeComplicatedVariable]]";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name), "Hello search did not return from Remote Server.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Given_Match_WholeWord_And_Case_And_Hello_World_Returns_Row()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchConnectControlCustom.ServerComboBox.ServersToggleButton);
            Mouse.Click(SearchUIMap.MainStudioWindow.RemoteConnectionInteCustom);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchwholewordCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name), "Hello search did not return from Remote Server.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Open_Search_Window_Has_All_Options_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.OutputVariableCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Click_Search_Button_With_Nothing_Filtered()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1));
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_WholeWord_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name), "Hello search did not return a row as had expected.");
        }


        [TestMethod]
        [TestCategory("Search View")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_Case_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "hello";
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.MatchcaseCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name), "Hello search did not return a row as had expected.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Search_Hello_And_Only_Service_Name_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchInputEdit.Text = "Hello";
            Mouse.Click(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchButton);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchResultsTable.ResultRow1.Name.Exists, "Hello search did not return a row as had expected,");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_Service_CheckBox_Then_AllCheckBox_Checkes_AllCheckBox()
        {
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_Service_CheckBox_Then_AllCheckBox_CheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.OutputVariableCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
        }


        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_AllCheckBox_UnCheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.OutputVariableCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.OutputVariableCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Checking_AllCheckBox_CheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Assert.IsFalse(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SearchTab.SearchOptionsExpander.OutputVariableCheckBox.Checked);
        }

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
    }
}
