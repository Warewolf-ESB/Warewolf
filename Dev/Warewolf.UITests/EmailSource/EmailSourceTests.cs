using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class EmailSourceTests
    {
        const string SourceName = "CodedUITestEmailSource";

        [TestMethod]
        [TestCategory("Email Source")]
        public void Open_EmailSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewEmailSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.HostTextBoxEdit.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.UserNameTextBoxEdit.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PasswordTextBoxEdit.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PortTextBoxEdit.Exists, "Port textbox does not exist after opening Email source tab");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Exists, "Timeout textbox does not exist after opening Email source tab");
            UIMap.Click_Close_EmailSource_Tab();
        }

        [TestMethod]
        [TestCategory("Email Source")]
        public void Create_EmailSource_UITests()
        {
            UIMap.Select_NewEmailSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "Email Source Tab does not exist");
            UIMap.Enter_Text_Into_EmailSource_Tab();
            UIMap.Click_EmailSource_TestConnection_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.ItemImage.Exists, "Connection test Failed");
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new e-mail source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            UIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            UIMap.Click_Close_EmailSource_Tab();
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
