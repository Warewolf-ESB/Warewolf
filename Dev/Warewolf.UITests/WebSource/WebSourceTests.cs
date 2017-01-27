using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.WebSource
{
    [CodedUITest]
    public class WebSourceTests
    {
        [TestMethod]
        [TestCategory("Web Source")]
        // ReSharper disable once InconsistentNaming
        public void Open_WebSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Click_NewWebSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AddressTextbox.Enabled, "Web server address textbox not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button is enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.AnonymousRadioButton.Enabled, "Anonymous Radio button is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.DefaultQueryTextBox.Enabled, "Default Query Textbox is not enabled");
            UIMap.Click_Close_Web_Source_Wizard_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Web Source")]
        // ReSharper disable once InconsistentNaming
        public void Create_WebSource_UITests()
        {
            UIMap.Click_NewWebSource_From_ExplorerContextMenu();
            UIMap.Click_UserButton_On_WebServiceSourceTab();
            UIMap.Enter_TextIntoAddress_On_WebServiceSourceTab();
            UIMap.Enter_RunAsUser_On_WebServiceSourceTab();
            UIMap.Enter_DefaultQuery_On_WebServiceSourceTab();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.WebSourceTab.WorkSurfaceContext.TestConnectionButton.Enabled, "Test Connection button not enabled");
            UIMap.Click_NewWebSource_TestConnectionButton();
            UIMap.Click_NewWebSource_CancelConnectionButton();
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