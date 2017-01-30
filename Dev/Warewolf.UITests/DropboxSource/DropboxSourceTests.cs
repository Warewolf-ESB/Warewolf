using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class DropboxSourceTests
    {
        [TestMethod]
        [TestCategory("Dropbox Source")]
        // ReSharper disable once InconsistentNaming
        public void Open_DropboxSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewDropboxSource_From_ExplorerContextMenu();
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.AuthoriseButton.Enabled, "Authorise button is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.ServerTypeComboBox.Enabled, "Server Type Combobox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.WorkSurfaceContext.OAuthKeyTextBox.Enabled, "OAuth Key Textbox is not enabled");
            UIMap.Click_OAuthSource_CloseTabButton();
        }

        [TestMethod]
        [TestCategory("Dropbox Source")]
        // ReSharper disable once InconsistentNaming
        public void Create_DropboxSource_UITests()
        {
            UIMap.Select_NewDropboxSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.OAuthSourceWizardTab.Exists, "OAuth Source Tab does not exist");
            UIMap.Enter_TextIntoOAuthKey_On_OAuthSourceTab();
            UIMap.Click_OAuthSource_AuthoriseButton();
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

        #endregion
    }
}