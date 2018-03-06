using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Windows.Input;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;

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
