
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.ExchangeSource.ExchangeSourceUIMapClasses;
using Warewolf.UITests.ExplorerUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ExchangeSourceTests
    {
        const string SourceName = "CodedUITestExchangeSource";

        [TestMethod]
        [TestCategory("Exchange Source")]
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
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.ToTextBox.ItemImage.Exists, "Connection test Failed");
            //Save Source
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            ExplorerUIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            ExchangeSourceUIMap.Click_ExchangeSource_CloseTabButton();
            //Edit Source
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.IsTrue(ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.Exists, "Exchange Source Tab does not exist.");
            ExchangeSourceUIMap.Edit_Timeout_On_ExchangeSource();
            ExchangeSourceUIMap.Click_ExchangeSource_TestConnection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            ExchangeSourceUIMap.Click_ExchangeSource_CloseTabButton();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.AreEqual("2000", ExchangeSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text);
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