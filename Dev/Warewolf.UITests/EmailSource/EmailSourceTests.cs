    using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Warewolf.UITests.EmailSource.EmailSourceUIMapClasses;
    using Warewolf.UITests.ExplorerUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class EmailSourceTests
    {
        const string SourceName = "CodedUITestEmailSource";

        [TestMethod]
        [TestCategory("Email Source")]
        public void Create_Save_And_Edit_EmailSource_From_ExplorerContextMenu_UITests()
        {
            //Create Source
            ExplorerUIMap.Select_NewEmailSource_From_ExplorerContextMenu();
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "Email Source Tab does not exist");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.HostTextBoxEdit.Exists, "Host textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.UserNameTextBoxEdit.Exists, "Username textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PasswordTextBoxEdit.Exists, "Password textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.PortTextBoxEdit.Exists, "Port textbox does not exist after opening Email source tab");
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Exists, "Timeout textbox does not exist after opening Email source tab")  ;
            EmailSourceUIMap.Enter_Text_Into_EmailSource_Tab();
            EmailSourceUIMap.Click_EmailSource_TestConnection_Button();
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.ToTextBoxEdit.ItemImage.Exists, "Connection test Failed");
            //Save Source
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save ribbon button is not enabled after successfully testing new source.");
            UIMap.Save_With_Ribbon_Button_And_Dialog(SourceName);
            ExplorerUIMap.Filter_Explorer(SourceName);
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.localhost.FirstItem.Exists, "Source did not save in the explorer UI.");
            EmailSourceUIMap.Click_Close_EmailSource_Tab();
            //Edit Source
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.IsTrue(EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.Exists, "Email Source Tab does not exist");
            EmailSourceUIMap.Edit_Timeout_On_EmailSource();
            EmailSourceUIMap.Click_EmailSource_TestConnection_Button();
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
            EmailSourceUIMap.Click_Close_EmailSource_Tab();
            ExplorerUIMap.Select_Source_From_ExplorerContextMenu(SourceName);
            Assert.AreEqual("2000", EmailSourceUIMap.MainStudioWindow.DockManager.SplitPaneMiddle.TabManSplitPane.TabMan.EmailSourceTab.SendTestModelsCustom.TimeoutTextBoxEdit.Text);
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

        EmailSourceUIMap EmailSourceUIMap
        {
            get
            {
                if (_EmailSourceUIMap == null)
                {
                    _EmailSourceUIMap = new EmailSourceUIMap();
                }

                return _EmailSourceUIMap;
            }
        }

        private EmailSourceUIMap _EmailSourceUIMap;

        #endregion
    }
}
