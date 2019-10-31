using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
// ReSharper disable InconsistentNaming

namespace Warewolf.UI.Tests
{
    [CodedUITest]
    public class Filter
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void Search_ExplorerResource()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            Keyboard.SendKeys("{ENTER}");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem), "Second Item exists in the Explorer Exists");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Search_ExplorerFolder()
        {
            ExplorerUIMap.Filter_Explorer("Acceptance Testing Resources");
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists);
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.SecondItem), "Second Item exists in the Explorer Exists");
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Clear_Filter_With_Esc_Key()
        {
            ExplorerUIMap.Filter_Explorer("Hello World");
            Keyboard.SendKeys("{ESCAPE}");
            Assert.AreEqual(string.Empty, ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.SearchTextBox.Text, "Filter textbox not cleared by ESC key.");
        }


        #region Additional test attributes

        [TestInitialize]
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

        #endregion
    }
}