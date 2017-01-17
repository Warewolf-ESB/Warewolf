using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests.RabbitMQSource
{
    [CodedUITest]
    public class RabbitMQSourceTests
    {
        const string SourceName = "CodedUITestRabbitMQSource";

        [TestMethod]
        // ReSharper disable once InconsistentNaming
        public void RabbitMQSource_CreateSourceUITests()
        {
            UIMap.Select_NewRabbitMQSource_FromExplorerContextMenu();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.HostTextBoxEdit.Enabled, "Host Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PortTextBoxEdit.Enabled, "Port Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.UserNameTextBoxEdit.Enabled, "Username Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.PasswordTextBoxEdit.Enabled, "Password Textbox is not enabled");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Enabled, "Virtual Host Textbox is not enabled");
            Assert.IsFalse(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is enabled");
            UIMap.Enter_Text_On_RabbitMQSourceTab();
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTabPage.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is not enabled");
            UIMap.Click_RabbitMQSource_TestConnectionButton();
            UIMap.Save_With_Ribbon_Button_And_Dialog("TestRabbitMQSource");
            UIMap.Filter_Explorer("TestRabbitMQSource");
            Assert.IsTrue(UIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Database did not save in the explorer UI.");
            UIMap.Click_Close_RabbitMQ_Source_Wizard_Tab_Button();
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