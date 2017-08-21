
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExchangeSource.ExchangeSourceUIMapClasses;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ExchangeSourceTests
    {
        const string SourceName = "CodedUITestExchangeSource";

        [TestMethod]
        [TestCategory("Source Wizards")]
        // ReSharper disable once InconsistentNaming
        public void Create_Save_And_Edit_ExchangeSource_From_ExplorerContextMenu_UITests()
        {
            //Create Source
            ExplorerUIMap.Select_NewExchangeSource_From_ExplorerContextMenu();
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "Exchange Source Tab does not exist.");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.UserNameTextBox.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.PasswordTextBox.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.Exists, "Port textbox does not exist after opening Email source tab");
            ExchangeSourceUIMap.Enter_Text_Into_Exchange_Tab();
            ExchangeSourceUIMap.Click_ExchangeSource_TestConnection_Button();
            System.Drawing.Point point;
            Assert.IsFalse(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.ItemImage.TryGetClickablePoint(out point), "Connection test Passed");
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

        ExchangeSourceUIMap ExchangeSourceUIMap
        {
            get
            {
                if (_ExchangeSourceUIMap == null)
                {
                    _ExchangeSourceUIMap = new ExchangeSourceUIMap();
                }

                return _ExchangeSourceUIMap;
            }
        }

        private ExchangeSourceUIMap _ExchangeSourceUIMap;

        #endregion
    }
}