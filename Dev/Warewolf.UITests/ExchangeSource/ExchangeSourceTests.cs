using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class ExchangeSourceTests
    {
        const string SourceName = "CodedUITestExchangeSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void ExchangeSource_CreateSourceUITests()
        {
            UIMap.Select_NewExchangeSource_FromExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.AutoDiscoverUrlTxtBox.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.UserNameTextBox.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.PasswordTextBox.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.ToTextBox.Exists, "Port textbox does not exist after opening Email source tab");
            UIMap.Enter_Text_Into_Exchange_Tab();
            UIMap.Click_ExchangeSource_TestConnection_Button();
            Assert.AreEqual("Passed", UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ExchangeSourceTabPage.SendTestModelsCustom.PassedText.DisplayText, "Connection test Failed");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new e-mail source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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

        #endregion
    }
}