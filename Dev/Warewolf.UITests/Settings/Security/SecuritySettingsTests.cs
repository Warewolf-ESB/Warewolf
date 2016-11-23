using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.UITests
{
    [CodedUITest]
    public class SecuritySettingsTests
    {
        [TestMethod]
        public void AddRemoveResourcePermission()
        {
            UIMap.SetResourcePermissions("Hello World", "Public", true, true, true);
            UIMap.Click_Settings_Resource_Permissions_Row1_Delete_Button();
            Assert.IsTrue(UIMap.MainStudioWindow.SideMenuBar.SaveButton.Enabled, "Save button is not enabled after clicking delete row button on existing resource permission in the security tab of the settings tab.");
            UIMap.Click_Save_Ribbon_Button_With_No_Save_Dialog();
        }

        #region Additional test attributes

        [TestInitialize()]
        public void MyTestInitialize()
        {
            UIMap.SetPlaybackSettings();
            UIMap.CloseHangingDialogs();
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

        #endregion
    }
}
