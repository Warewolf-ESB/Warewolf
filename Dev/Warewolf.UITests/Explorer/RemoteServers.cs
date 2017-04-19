using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.UITests.Explorer.ExplorerUIMapClasses;
using Warewolf.UITests.Settings.SettingsUIMapClasses;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class RemoteServers
    {
        [TestMethod]
        [TestCategory("Explorer")]
        public void Server_DropDown_Has_Remote_Servers_UITest()
        {
            ExplorerUIMap.Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists);
        }

        [TestMethod]
        [TestCategory("Explorer")]
        public void Edit_Server_Removes_Server_From_Explorer()
        {         
            ExplorerUIMap.Click_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(UIMap.MainStudioWindow.ComboboxListItemAsRemoteConnectionIntegration.Exists);
            ExplorerUIMap.Select_Explorer_Remote_Server_Dropdown_List();
            Assert.IsTrue(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer.Exists, "Remote server is not loaded in the Explorer.");
            ExplorerUIMap.Click_EditServerButton_From_ExplorerConnectControl();
            SettingsUIMap.ChangeServerAuthenticationType();
            Assert.IsFalse(UIMap.ControlExistsNow(ExplorerUIMap.MainStudioWindow.DockManager.SplitPaneLeft.Explorer.ExplorerTree.FirstRemoteServer), "Remote server is not loaded in the Explorer.");
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.AssertStudioIsRunning();
        }

        UIMap UIMap
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

        SettingsUIMap SettingsUIMap
        {
            get
            {
                if (_SettingsUIMap == null)
                {
                    _SettingsUIMap = new SettingsUIMap();
                }

                return _SettingsUIMap;
            }
        }

        private SettingsUIMap _SettingsUIMap;
        #endregion
    }
}
