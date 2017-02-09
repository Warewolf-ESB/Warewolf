using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.SharepointSource
{
    [CodedUITest]
    public class SharepointSourceTests
    {
        private const string editSourceName = "SharePointServiceSourceToEdit";

        [TestMethod]
        [TestCategory("Sharepoint Source")]
        // ReSharper disable once InconsistentNaming
        public void Create_SharepointSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewSharepointSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.Exists, "Sharepoint Source Tab does not exist.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.ServerNameEdit.Enabled,"Server Name Textbox is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.WindowsRadioButton.Enabled, "Windows Radio button is not enabled.");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserRadioButton.Enabled, "User Radio button is not enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.TestConnectionButton.Enabled, "Test Connection button is enabled.");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.CancelTestButton.Enabled, "Cancel Test button is  enabled.");
            UIMap.Enter_TextIntoAddress_In_SharepointServiceSourceTab();
            UIMap.Click_UserButton_On_SharepointSource();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.UserNameTextBox.Exists);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.SharepointServerSourceTab.SharepointServerSourceView.SharepointView.PasswordTextBox.Exists);
            UIMap.Enter_Sharepoint_ServerSource_User_Credentials();
            UIMap.Click_Sharepoint_Server_Source_TestConnection();
        }

        [TestMethod]
        [TestCategory("Sharepoint Source")]
        // ReSharper disable once InconsistentNaming
        public void Edit_SharepointSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_Source_From_ExplorerContextMenu(editSourceName);
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