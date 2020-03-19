using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using Warewolf.Test.Agent;
using Warewolf.UI.Tests.DialogsUIMapClasses;
using Warewolf.UI.Tests.Explorer.ExplorerUIMapClasses;
using Warewolf.UI.Tests.RabbitMQSource.RabbitMQSourceUIMapClasses;

namespace Warewolf.UI.Tests.RabbitMQSource
{
    [CodedUITest]
    public class RabbitMQSourceTests
    {
        const string SourceName = "CodedUITestRabbitMQSource";

        [TestMethod]
        [TestCategory("Database Sources")]
        // ReSharper disable once InconsistentNaming
        public void Create_Save_And_Open_RabbitMQSource_From_ExplorerContextMenu_UITests()
        {
            using (ContainerLauncher RabbitMQContainer = TestLauncher.StartLocalRabbitMQContainer(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestResults")))
            {
                //Create Source
                ExplorerUIMap.Select_NewRabbitMQSource_From_ExplorerContextMenu();
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.Exists, "RabbitMQ Source Tab does not exist");
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.HostTextBoxEdit.Enabled, "Host Textbox is not enabled");
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PortTextBoxEdit.Enabled, "Port Textbox is not enabled");
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.UserNameTextBoxEdit.Enabled, "Username Textbox is not enabled");
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.PasswordTextBoxEdit.Enabled, "Password Textbox is not enabled");
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.VirtualHostTextBoxEdit.Enabled, "Virtual Host Textbox is not enabled");
                Assert.IsFalse(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is enabled");
                RabbitMQSourceUIMap.Enter_Text_On_RabbitMQSourceTab();
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.TestConnectionButton.Enabled, "Test Connection button is not enabled");
                RabbitMQSourceUIMap.Click_RabbitMQSource_TestConnectionButton();
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.RabbitMQSourceCustom.ItemImage.Exists, "Test Connection successful image does not appear.");
                Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
                //Save Source
                UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
                ExplorerUIMap.Filter_Explorer(SourceName);
                Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
                RabbitMQSourceUIMap.Click_Close_RabbitMQSource_Tab_Button();
                //Open Source
                ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
                Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.Exists, "RabbitMQ Source Tab does not exist");
            }
        }

        [TestMethod]
        [TestCategory("RabbitMQ Sources")]
        [Owner("Pieter Terblanche")]
        public void CreateRabbitMQSource_GivenTabHasChanges_ClosingStudioPromptsChanges()
        {
            //Create Source
            ExplorerUIMap.Select_NewRabbitMQSource_From_ExplorerContextMenu();
            RabbitMQSourceUIMap.Enter_Text_On_RabbitMQSourceTab();
            Mouse.Click(UIMap.MainStudioWindow.CloseStudioButton);
            DialogsUIMap.Click_MessageBox_Cancel();
            Assert.IsTrue(RabbitMQSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.RabbitMqSourceTab.Exists);
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

        RabbitMQSourceUIMap RabbitMQSourceUIMap
        {
            get
            {
                if (_RabbitMQSourceUIMap == null)
                {
                    _RabbitMQSourceUIMap = new RabbitMQSourceUIMap();
                }

                return _RabbitMQSourceUIMap;
            }
        }

        private RabbitMQSourceUIMap _RabbitMQSourceUIMap;
        DialogsUIMap DialogsUIMap
        {
            get
            {
                if (_DialogsUIMap == null)
                {
                    _DialogsUIMap = new DialogsUIMap();
                }

                return _DialogsUIMap;
            }
        }

        private DialogsUIMap _DialogsUIMap;
        #endregion
    }
}