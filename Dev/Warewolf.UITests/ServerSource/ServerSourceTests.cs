using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.ServerSource
{
    [CodedUITest]
    public class ServerSourceTests
    {
        [TestMethod]
        [TestCategory("Server Source")]
        // ReSharper disable once InconsistentNaming
        public void Open_ServerSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewServerSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.ProtocolCombobox.Enabled, "Protocol Combobox not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.AddressComboBox.Enabled, "Address Combobox not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.PublicRadioButton.Enabled, "Public Radio button not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.UserRadioButton.Enabled, "User Radio button not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.WindowsRadioButton.Enabled, "Windows Radio button not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button is enabled");
            UIMap.Click_Close_Server_Source_Wizard_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Server Source")]
        // ReSharper disable once InconsistentNaming
        public void Create_ServerSource_UITests()
        {
            UIMap.Select_NewServerSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.Exists, "Server Source Tab does not exist");
            UIMap.Click_UserButton_On_ServerSourceTab();
            UIMap.Enter_TextIntoAddress_On_ServerSourceTab();
            UIMap.Enter_RunAsUser_On_ServerSourceTab();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.ServerSourceTab.WorkSurfaceContext.NewServerSource.TestConnectionButton.Enabled, "Test Connection button not enabled");
            UIMap.Click_Server_Source_Wizard_Test_Connection_Button();
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