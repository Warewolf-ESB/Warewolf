using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.RabbitMQSource
{
    [CodedUITest]
    public class RabbitMQSourceTests
    {
        const string SourceName = "CodedUITestRabbitMQSource";

        [TestMethod]
        [TestCategory("Database Sources")]
        // ReSharper disable once InconsistentNaming
        public void Open_RabbitMQSource_From_ExplorerContextMenu_UITests()
        {
            UIMap.Select_NewRabbitMQSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.HostTextBoxEdit.Enabled, "Host Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PortTextBoxEdit.Enabled, "Port Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.UserNameTextBoxEdit.Enabled, "Username Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PasswordTextBoxEdit.Enabled, "Password Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Enabled, "Virtual Host Textbox is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is enabled");
            UIMap.Click_Close_RabbitMQ_Source_Tab_Button();
        }

        [TestMethod]
        [TestCategory("Database Sources")]
        // ReSharper disable once InconsistentNaming
        public void Create_RabbitMQSource_UITests()
        {
            UIMap.Select_NewRabbitMQSource_From_ExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.Exists, "RabbitMQ Source Tab does not exist");
            UIMap.Enter_Text_On_RabbitMQSourceTab();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is not enabled");
            UIMap.Click_RabbitMQSource_TestConnectionButton();
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            UIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            UIMap.Click_Close_RabbitMQ_Source_Tab_Button();
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