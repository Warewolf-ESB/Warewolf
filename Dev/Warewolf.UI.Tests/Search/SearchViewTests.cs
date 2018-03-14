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
    public class SearchViewTests
    {
        [TestMethod]
        [TestCategory("Search View")]
        public void Shortcut_Cntr_Shift_F_Opens_Search_View()
        {
            ExplorerUIMap.Click_Explorer_Refresh_Button();
            Keyboard.SendKeys("^+F");
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after using shortcut Cntr+Shift+F.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Clicking_Search_Menu_Item_Opens_Search_View()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
        }
        [TestMethod]
        [TestCategory("Search View")]
        public void Open_Search_Window_Has_All_Options_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.OutputVariableCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Click_Search_Button_With_Nothing_Filtered()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.UISearchViewWindow.SearchResultsTable.ResultRow1));
        }
        [TestMethod]
        [TestCategory("Search View")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_WholeWord_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = false;
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.UISearchViewWindow.SearchInputEdit.Text = "hello";
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.UISearchViewWindow.SearchResultsTable.ResultRow1.Name), "Hello search did not return a row as had expected.");
        }


        [TestMethod]
        [TestCategory("Search View")]
        public void Search_hello_And_Only_Service_Name_Is_Selected_And_Match_Case_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = false;
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.UISearchViewWindow.SearchInputEdit.Text = "hello";
            SearchUIMap.UISearchViewWindow.MatchcaseCheckBox.Checked = false;
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchButton);
            Assert.IsFalse(UIMap.ControlExistsNow(SearchUIMap.UISearchViewWindow.SearchResultsTable.ResultRow1.Name), "Hello search did not return a row as had expected.");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void Search_Hello_And_Only_Service_Name_Is_Selected()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = false;
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked = true;
            SearchUIMap.UISearchViewWindow.SearchInputEdit.Text = "Hello";
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchResultsTable.ResultRow1.Name.Exists, "Hello search did not return a row as had expected,");
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_Service_CheckBox_Then_AllCheckBox_Checkes_AllCheckBox()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);            
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_Service_CheckBox_Then_AllCheckBox_CheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);            
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.OutputVariableCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
        }


        [TestMethod]
        [TestCategory("Search View")]
        public void Checking_AllCheckBox_CheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.OutputVariableCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = true;
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.OutputVariableCheckBox.Checked);
        }

        [TestMethod]
        [TestCategory("Search View")]
        public void UnChecking_AllCheckBox_UnCheckesAll_Check_Boxes()
        {
            Mouse.Click(UIMap.MainStudioWindow.SideMenuBar.SearchButton, new Point(16, 11));
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.Exists, "Search View Window did not Open after clicking Seacrch Menu Item.");
            Mouse.Click(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.SearchOptionsExpanderButton);
            Assert.IsTrue(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked);
            SearchUIMap.UISearchViewWindow.SearchOptionsExpander.AllCheckBox.Checked = false;
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ServiceCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ScalarCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.TestNameCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.RecordsetCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ToolTitleCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.ObjectCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.InputVariableCheckBox.Checked);
            Assert.IsFalse(SearchUIMap.UISearchViewWindow.SearchOptionsExpander.OutputVariableCheckBox.Checked);
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
